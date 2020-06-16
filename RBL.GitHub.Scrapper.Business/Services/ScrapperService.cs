using AutoMapper;
using CsQuery;
using CsQuery.ExtensionMethods;
using CsQuery.Promises;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Domain;
using RBL.GitHub.Scrapper.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Business.Services
{
    public class ScrapperService : NotificationService, IScrapperService
    {
        private readonly IMapper _mapper;
        public ScrapperService(INotifier notifier, IMapper mapper)
            : base(notifier)
        {
            _mapper = mapper;
        }

        private static SemaphoreSlim Throttler = new SemaphoreSlim(initialCount: 20);

        public async Task<ScrappingInfoViewModel> ScrapeGitHub(string gitHubRepository)
        {
            if(this.Validate(gitHubRepository))
            {
                DateTime startTime = DateTime.Now;
                List<GitHubFileViewModel> gitHubFiles = await GetGitHubFiles(gitHubRepository,true);
                DateTime endTime = DateTime.Now;
                TimeSpan processingTime = endTime - startTime;
                ScrappingInfoViewModel scrappingInfoViewModel = new ScrappingInfoViewModel(gitHubRepository,gitHubFiles, processingTime);

                return scrappingInfoViewModel;
            }

            return null;
        }

        private bool Validate(string gitHubRepository)
        {
            bool retorno = true;

            if (string.IsNullOrEmpty(gitHubRepository))
            {
                retorno = false;
                this.Notify("Undefined repository");
            }

            return retorno;
        }

        private async Task<List<GitHubFileViewModel>> GetGitHubFiles(string gitHubRepository,bool navigateSubFolders=true)
        {
            List<GitHubFileViewModel> gitHubFiles = new List<GitHubFileViewModel>();

            var files = await this.ProcessHTML(gitHubRepository,navigateSubFolders);
            gitHubFiles = _mapper.Map<IEnumerable<GitHubFileViewModel>>(files).ToList();

            return gitHubFiles;
        }

        //private async List<GitHubFile> ProcessHTMLPromise(string url, bool navigateSubFolders = true)
        //{
        //    List<GitHubFile> gitHubFiles = new List<GitHubFile>();
        //    var promise = CQ.CreateFromUrlAsync(url)
        //        .Then(responseSuccess =>
        //        {
        //            var tableFiles = responseSuccess.Dom[".files"];
        //            gitHubFiles = await this.ProcessTable(tableFiles, navigateSubFolders);
        //        },
        //        responseFail =>
        //        {
        //            this.Notify(responseFail.Error);
        //        });

        //    When.All(promise).Then(() => { });

        //    return gitHubFiles;
        //}

        private async Task<List<GitHubFile>> ProcessHTML(string url,bool navigateSubFolders=true)
        {
            List<GitHubFile> gitHubFiles = new List<GitHubFile>();
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    string data = readStream.ReadToEnd();



                    var doc = CQ.CreateDocument(data);
                    var table = doc[".files"];
                    gitHubFiles = await this.ProcessTable(table, navigateSubFolders);
                    
                    //userId = res.Val();
                    response.Close();
                    readStream.Close();
                }
                else
                {
                    this.Notify($"Invalid repository {url}");
                }
            }
            catch (Exception erro)
            {
                this.Notify($"Invalid repository {url}");
            }

            return gitHubFiles;
        }

        private async Task<List<GitHubFile>> ProcessTable(CQ tableFiles, bool navigateSubFolders=true)
        {
            List<GitHubFile> items = new List<GitHubFile>();
            var tableBody = tableFiles["tbody"].First();
            var rows = tableBody["tr"].ToList();

            int maximumSimultaneouslyTask = 4;
            //List<IDomObject> rowsExecuting = new List<IDomObject>();
            //IEnumerable<Task> doWork()
            //{
            //    foreach (var row in rows.Take(maximumSimultaneouslyTask))
            //        yield return Task.Run(async() => items.AddRange(await ProcessRow(row, navigateSubFolders)));
            //}


            //await Task.WhenAll(doWork());

            var allTasks = new List<Task>();
            
            foreach (var row in rows)
            {
                // do an async wait until we can schedule again
                await Throttler.WaitAsync();

                // using Task.Run(...) to run the lambda in its own parallel
                // flow on the threadpool
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            items.AddRange(await ProcessRow(row, navigateSubFolders));
                        }
                        finally
                        {
                            Throttler.Release();
                        }
                    }));
            }

            // won't get here until all urls have been put into tasks
            await Task.WhenAll(allTasks);



            return items;
        }

        private async Task<List<GitHubFile>> ProcessRow(IDomObject row, bool navigateSubFolders=true)
        {
            List<GitHubFile> items = new List<GitHubFile>();
            bool isFile = false;
            var columns = row.ChildElements;
            if (columns != null)
            {
                var iconColumn = columns.FirstOrDefault(c => c.Classes.Any(cl => cl == "icon"));
                if (iconColumn != null)
                {
                    var svgElement = iconColumn.ChildElements.FirstOrDefault(c => c.NodeName.ToUpper() == "SVG");
                    if (svgElement != null)
                    {
                        isFile = svgElement.Classes.Any(c => c == "octicon-file");

                        var contentColumn = columns.FirstOrDefault(c => c.Classes.Any(cl => cl == "content"));
                        if (contentColumn != null)
                        {
                            if (isFile)
                            {
                                var spanElement = contentColumn.ChildElements.FirstOrDefault(c => c.NodeName == "SPAN");
                                if(spanElement != null)
                                {
                                    var linkElement = spanElement.ChildElements.FirstOrDefault(c => c.NodeName == "A");
                                    //if(linkElement != null)
                                    {
                                        string fileName = linkElement.InnerText;
                                        var urlAttribute = linkElement.Attributes["href"];

                                        var fileInformations = await GetFileInformations(urlAttribute);
                                        var extension = GetExtension(fileName);
                                        items.Add(new GitHubFile(fileName, extension, fileInformations.Lines, fileInformations.Size));
                                    }
                                }
                            }
                            else if(navigateSubFolders)
                            {
                                var spanElement = contentColumn.ChildElements.FirstOrDefault(c => c.NodeName == "SPAN");
                                if(spanElement != null)
                                {
                                    var linkElement = spanElement.ChildElements.FirstOrDefault(c => c.NodeName == "A");
                                    //if (linkElement != null)
                                    {
                                        string fileName = linkElement.InnerText;
                                        var urlAttribute = linkElement.Attributes["href"];

                                        items.AddRange(await this.ProcessHTML($"https://github.com/{urlAttribute}",navigateSubFolders));
                                    }
                                }
                            }
                        }

                        
                    }
                }
            }

            return items;
        }

        private async Task<FileInformations> GetFileInformations(string urlAttribute)
        {
            int lines = 0;
            long size = 0;
            try
            {

                var url = $"https://github.com/{urlAttribute}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    string data = readStream.ReadToEnd();

                    var doc = CQ.CreateDocument(data);
                    var boxHeader = doc[".Box-header"];
                    if (boxHeader != null)
                    {
                        //var fileLines = boxHeader[".js-line-number"];
                        //lines = fileLines.Length;

                        var element = boxHeader
                                        .Elements
                                        .LastOrDefault()
                                        .ChildElements?
                                        .FirstOrDefault(c =>
                                            c.Classes.Any(c2 => c2.ToUpper() == "TEXT-MONO") &&
                                            c.Classes.Any(c2 => c2.ToUpper() == "F6"));

                        var text = element?.InnerText;
                        text = text?.Replace("\n", "")?.Trim();

                        //var fileInfoDivider = boxHeader[".text-mono f6"];
                        //var parentFileInfoDivider = fileInfoDivider.Parent();
                        //var text = parentFileInfoDivider.FirstOrDefault()?.InnerText;

                        lines = 0;
                        size = 0;
                        var split = text?.Split(" ");

                        if (split != null)
                        {
                            var linesItemArray = split.FirstOrDefault(s => s == "lines" || s == "line");
                            var indexItemArray = split.IndexOf(linesItemArray);
                            if (indexItemArray > 0)
                            {
                                lines = Convert.ToInt32(split[indexItemArray - 1]);
                            }

                            var units = split[split.Length - 1];
                            decimal sizeFile = Convert.ToDecimal(split[split.Length - 2]);
                            size = ConvertToBytes(sizeFile, units);
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                    response.Close();
                    readStream.Close();
                }
            }
            catch (Exception erro)
            {
                this.Notify($"{erro.Message}");
            }

            FileInformations fileInformations = new FileInformations(lines, size);
            return fileInformations;
        }

        private long ConvertToBytes(decimal size, string units)
        {
            long multiplier = 1;

            switch(units.ToUpper())
            {
                case "GBYTES":
                case "GBYTE":
                case "GB": { multiplier = 8589934592; break; }//1024*1024*1024*8
                case "MBYTES":
                case "MBYTE":
                case "MB": { multiplier = 8388608; break; }//1024*1024*8
                case "KBYTES": 
                case "KBYTE": 
                case "KB": { multiplier = 8192; break; }//1024*8
                case "BYTES":
                case "BYTE":
                case "B": { multiplier = 1; break; }
                default: multiplier = 0;break;
            }
            var result = size* multiplier;
            return (int)Math.Truncate(result);
        }

        private Extension GetExtension(string fileName)
        {
            var fileNameParts = fileName.Split(".");
            var extensionDescription = fileNameParts.Length > 1 ? fileNameParts[fileNameParts.Length - 1] : "Unknown";
            
            var extension = new Extension(extensionDescription.ToLower());
            
            return extension;
        }
    }
}
