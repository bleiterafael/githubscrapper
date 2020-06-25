using AutoMapper;
using CsQuery;
using CsQuery.ExtensionMethods;
using CsQuery.Promises;
using Newtonsoft.Json;
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
        private static List<ScrappingInfoViewModel> scrappings;
        private static readonly Object obj = new Object();
        private static SemaphoreSlim Throttler = new SemaphoreSlim(initialCount: 20);
        private static List<Task> AllTasks = new List<Task>();
        private static DateTime LastRequest = DateTime.Now;
        private static TimeSpan IntervalRequests = new TimeSpan(0, 0, 0, 0, 50);

        private readonly IMapper _mapper;
        public ScrapperService(INotifier notifier, IMapper mapper)
            : base(notifier)
        {
            _mapper = mapper;
            if (scrappings == null)
                scrappings = new List<ScrappingInfoViewModel>();
        }

        
        public async Task<ScrappingInfoViewModel> ScrapeGitHub(string gitHubRepository, bool navigateSubFolders=true)
        {
            if(this.Validate(gitHubRepository))
            {
                try
                {
                    
                    ScrappingInfoViewModel scrappingInfoViewModel = null;

                    DateTime startTime = DateTime.Now;
                    DateTime endTime;
                    TimeSpan processingTime;

                    scrappingInfoViewModel = scrappings.FirstOrDefault(s => s.GitHubRepository == gitHubRepository);
                    DateTime lastUpdate = await GetLastUpdate(gitHubRepository);

                    bool needLoadData = true;
                    if (scrappingInfoViewModel != null)
                    {
                        needLoadData = lastUpdate != scrappingInfoViewModel.LastUpdate;

                        if (!needLoadData)
                        {
                            endTime = DateTime.Now;
                            processingTime = endTime - startTime;
                            scrappingInfoViewModel.SetProcessTime(processingTime);
                            return scrappingInfoViewModel;
                        }
                    }

                    List<GitHubFileViewModel> gitHubFiles = await GetGitHubFiles(gitHubRepository, navigateSubFolders);
                    endTime = DateTime.Now;
                    processingTime = endTime - startTime;
                    scrappingInfoViewModel = new ScrappingInfoViewModel(gitHubRepository, gitHubFiles, lastUpdate, processingTime);

                    scrappings.Add(scrappingInfoViewModel);

                    return scrappingInfoViewModel;
                }
                catch (Exception error)
                {
                    base.Notify(error.Message);
                }
            }

            return null;
        }

        private async Task<DateTime> GetLastUpdate(string gitHubRepository)
        {
            DateTime lastUpdate = DateTime.Now;

            int lines = 0;
            decimal size = 0;
            try
            {
                var url = gitHubRepository;

                HttpWebRequest request = null;
                HttpWebResponse response = null;

                Request(url, ref request, ref response);

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
                    var relativeTimeTag = doc["relative-time"];
                    if(relativeTimeTag.Elements.Count() == 0)
                    {
                        relativeTimeTag = doc["time-ago"];
                    }

                    if (relativeTimeTag.Elements.Count() > 0)
                    {
                        var relativeTime = relativeTimeTag.FirstElement().Attributes["datetime"];
                        lastUpdate = Convert.ToDateTime(relativeTime);
                    }
                    response.Close();
                    readStream.Close();
                }
            }
            catch (Exception error)
            {
                throw error;
            }

            return lastUpdate;
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

            IEnumerable<Task> ProcessFiles()
            {
                foreach (var file in files)
                    yield return Task.Run(async () =>
                    {
                        FileInformations fileInformations = await GetFileInformations(file.UrlAttribute);
                        file.SetLines(fileInformations.Lines);
                        file.SetSize(fileInformations.Size);
                    });
            }
            await Task.WhenAll(ProcessFiles());

            gitHubFiles = _mapper.Map<IEnumerable<GitHubFileViewModel>>(files).ToList();

            return gitHubFiles;
        }

        private void Sleep()
        {
            var nextRequest = LastRequest.AddMilliseconds(IntervalRequests.TotalMilliseconds);
            DateTime currentDate = DateTime.Now;
            bool sleep = nextRequest > currentDate;
            TimeSpan timeToSleep = sleep ? (nextRequest - currentDate) : new TimeSpan(0);
            Thread.Sleep(timeToSleep);
        }

        private void Request(string url, ref HttpWebRequest request, ref HttpWebResponse response)
        {
            bool success = false;

            // critical section -- avoiding Too much requests
            while (!success)
            {
                Sleep();

                try
                {
                    //lock (obj)
                    //{
                    
                    request = (HttpWebRequest)WebRequest.Create(url);
                    response = (HttpWebResponse)request.GetResponse();
                    LastRequest = DateTime.Now;
                    success = true;
                    //}
                }
                catch (Exception error)
                {
                    LastRequest = DateTime.Now.AddSeconds(3);
                }
            }
            
            
        }

        private async Task<List<GitHubFile>> ProcessHTML(string url,bool navigateSubFolders=true)
        {
            List<GitHubFile> gitHubFiles = new List<GitHubFile>();
            try
            {
                HttpWebRequest request = null;
                HttpWebResponse response = null;

                Request(url, ref request,ref response);

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
                    var table = doc["[role=\"grid\"]"];
                    gitHubFiles = await this.ProcessTable(table, navigateSubFolders);
                    
                    response.Close();
                    readStream.Close();
                }
                else
                {
                    this.Notify($"Invalid repository {url}");
                }
            }
            catch (Exception error)
            {
                throw error;
            }

            return gitHubFiles;
        }

        private async Task<List<GitHubFile>> ProcessTable(CQ tableFiles, bool navigateSubFolders = true)
        {
            List<GitHubFile> items = new List<GitHubFile>();
            //var tableBody = tableFiles["tbody"].First();
            var rows = tableFiles["[role=\"row\"]"].ToList();

            List<GitHubItem> gitHubItems = new List<GitHubItem>();
            foreach (var row in rows)
            {
                gitHubItems.AddRange(await GetGitHubItems(row, navigateSubFolders));
            }

            var gitHubFiles = gitHubItems.Where(g => g is GitHubFile).ToList();
            var gitHubFolders = gitHubItems.Where(g => g is GitHubFolder);

            IEnumerable<Task> ProcessFolders()
            {
                foreach (var folder in gitHubFolders)
                    yield return Task.Run(async () => gitHubFiles.AddRange(await ProcessHTML($"https://github.com/{folder.UrlAttribute}", navigateSubFolders)));
            }
            await Task.WhenAll(ProcessFolders());

            items = gitHubFiles.Select(t => t as GitHubFile).ToList();
            return items;
        }

        private async Task<List<GitHubItem>> GetGitHubItems(IDomObject row, bool navigateSubFolders = true)
        {
            List<GitHubItem> items = new List<GitHubItem>();
            bool isFile = false;
            var columns = row.ChildElements;
            if (columns != null)
            {
                var iconColumn = columns.FirstOrDefault(c => c.Attributes.Any(at => at.Key == "role" && at.Value=="gridcell"));
                if (iconColumn != null)
                {
                    var svgElement = iconColumn.ChildElements.FirstOrDefault(c => c.NodeName.ToUpper() == "SVG");
                    if (svgElement != null)
                    {
                        isFile = svgElement.Classes.Any(c => c == "octicon-file");

                        //var contentColumn = columns.FirstOrDefault(c => c.Classes.Any(cl => cl == "content"));
                        var contentColumn = columns.FirstOrDefault(c => c.Attributes.Any(at => at.Key == "role" && at.Value == "rowheader")); ;
                        if (contentColumn != null)
                        {
                            if (isFile)
                            {
                                var spanElement = contentColumn.ChildElements.FirstOrDefault(c => c.NodeName == "SPAN");
                                if (spanElement != null)
                                {
                                    var linkElement = spanElement.ChildElements.FirstOrDefault(c => c.NodeName == "A");
                                    if(linkElement != null)
                                    {
                                        string fileName = linkElement.InnerText;
                                        var urlAttribute = linkElement.Attributes["href"];

                                        var extension = GetExtension(fileName);
                                        items.Add(new GitHubFile(fileName, extension, urlAttribute));
                                    }
                                }
                            }
                            else if (navigateSubFolders)
                            {
                                var spanElement = contentColumn.ChildElements.FirstOrDefault(c => c.NodeName == "SPAN");
                                if (spanElement != null)
                                {
                                    var linkElement = spanElement.ChildElements.FirstOrDefault(c => c.NodeName == "A");
                                    if (linkElement != null)
                                    {
                                        string fileName = linkElement.InnerText;
                                        var urlAttribute = linkElement.Attributes["href"];

                                        items.Add(new GitHubFolder(fileName, urlAttribute));
                                    }
                                }
                            }
                        }
                    }
                }
            }

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
                                        items.Add(new GitHubFile(fileName, extension, urlAttribute, fileInformations.Lines, fileInformations.Size));
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
            decimal size = 0;
            try
            {
                var url = $"https://github.com/{urlAttribute}";

                HttpWebRequest request = null;
                HttpWebResponse response = null;

                Request(url, ref request, ref response);

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
                        var element = boxHeader
                                        .Elements
                                        .LastOrDefault()
                                        .ChildElements?
                                        .FirstOrDefault(c =>
                                            c.Classes.Any(c2 => c2.ToUpper() == "TEXT-MONO") &&
                                            c.Classes.Any(c2 => c2.ToUpper() == "F6"));

                        var text = element?.InnerText;
                        text = text?.Replace("\n", "")?.Trim();
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
                            decimal sizeFile = Convert.ToDecimal(split[split.Length - 2].Replace(".",","));
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
            catch (Exception error)
            {
                throw error;
            }

            FileInformations fileInformations = new FileInformations(lines, size);
            return fileInformations;
        }

        private decimal ConvertToBytes(decimal size, string units)
        {
            long multiplier = 1;

            switch(units.ToUpper())
            {
                case "GBYTES":
                case "GBYTE":
                case "GB": { multiplier = 1073741824; break; }//1024*1024*1024
                case "MBYTES":
                case "MBYTE":
                case "MB": { multiplier = 1048576; break; }//1024*1024
                case "KBYTES": 
                case "KBYTE": 
                case "KB": { multiplier = 1024; break; }//1024
                case "BYTES":
                case "BYTE":
                case "B": { multiplier = 1; break; }
                default: multiplier = 0;break;
            }
            var result = size * multiplier;
            return result;
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
