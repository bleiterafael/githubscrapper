﻿using AutoMapper;
using CsQuery;
using CsQuery.ExtensionMethods;
using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Data.EF.IRepositories;
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
        private static DateTime LastRequest = DateTime.Now;
        private static TimeSpan IntervalRequests = new TimeSpan(0, 0, 0, 0, 50);

        private readonly IMapper _mapper;
        private readonly IScrappingInfoRepository _scrappingInfoRepository;
        public ScrapperService(INotifier notifier, IMapper mapper,
            IScrappingInfoRepository scrappingInfoRepository)
            : base(notifier)
        {
            _mapper = mapper;
            _scrappingInfoRepository = scrappingInfoRepository;
        }

        
        public async Task<ScrappingInfoViewModel> ScrapeGitHub(string gitHubRepository, bool navigateSubFolders=true)
        {
            if(this.Validate(gitHubRepository))
            {
                try
                {
                    ScrappingInfoViewModel scrappingInfoViewModel = null;
                    ScrappingInfo scrappingInfo = null;

                    DateTime startTime = DateTime.Now;
                    DateTime endTime;
                    TimeSpan processingTime;

                    scrappingInfo = (await _scrappingInfoRepository.GetAsync(s => s.GitHubRepository == gitHubRepository)).FirstOrDefault();
                        
                    DateTime lastUpdate = await GetLastUpdate(gitHubRepository);
                    bool needDeleteCurrentScrappingInfo = false;
                    bool needLoadData = true;
                    if (scrappingInfo != null)
                    {
                        needLoadData = lastUpdate > scrappingInfo.LastUpdate;

                        if (!needLoadData)
                        {
                            endTime = DateTime.Now;
                            processingTime = endTime - startTime;
                            scrappingInfo.SetProcessTime(processingTime);
                            scrappingInfoViewModel = _mapper.Map<ScrappingInfoViewModel>(scrappingInfo);
                            return scrappingInfoViewModel;
                        }
                        needDeleteCurrentScrappingInfo = true;
                    }

                    List<GitHubFile> gitHubFiles = await GetGitHubFiles(gitHubRepository, navigateSubFolders);
                    endTime = DateTime.Now;
                    processingTime = endTime - startTime;

                    if (needDeleteCurrentScrappingInfo)
                        await _scrappingInfoRepository.DeleteAsync(scrappingInfo);

                    scrappingInfo = new ScrappingInfo(gitHubRepository, gitHubFiles, lastUpdate, processingTime);

                    await _scrappingInfoRepository.AddAsync(scrappingInfo);
                    scrappingInfoViewModel = _mapper.Map<ScrappingInfoViewModel>(scrappingInfo);
                    

                    return scrappingInfoViewModel;
                }
                catch (WebException error)
                {
                    var statusCode = (error.Response as HttpWebResponse)?.StatusCode;
                    if (statusCode == HttpStatusCode.NotFound)
                        base.Notify($"Repository '{gitHubRepository}' not found");
                    else
                        base.Notify(error.Message);
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
            gitHubRepository = $"{gitHubRepository}/file-list/master";
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

                    var element = doc["[datetime]"];
                    
                    if(!element.Elements.Any())
                    {
                        element = doc["relative-time"];
                    }
                    if (!element.Elements.Any())
                    {
                        element = doc["time-ago"];
                    }

                    var times = element.Elements.Where(c => c.Attributes.Any(at => at.Key == "datetime")).ToList();
                    var dates = new List<DateTime>();
                    times.ForEach(t =>
                    {
                        var dateStr = t.Attributes["datetime"];
                        var date = Convert.ToDateTime(dateStr);
                        dates.Add(date);
                    });
                    if (dates.Any())
                    {
                        lastUpdate = dates.Max();
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
            else if (!gitHubRepository.StartsWith("https://github.com/"))
            {
                retorno = false;
                this.Notify($"{gitHubRepository} does not belong a https://github.com/{{repository}} domain");
            }

            return retorno;
        }

        private async Task<List<GitHubFile>> GetGitHubFiles(string gitHubRepository,bool navigateSubFolders=true)
        {
            List<GitHubFile> gitHubFiles = new List<GitHubFile>();

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

            gitHubFiles = files;

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
                    request = (HttpWebRequest)WebRequest.Create(url);
                    response = (HttpWebResponse)request.GetResponse();
                    LastRequest = DateTime.Now;
                    success = true;
                }
                catch (WebException ex)
                {
                    var statusCode = (ex.Response as HttpWebResponse)?.StatusCode;
                    if (statusCode == HttpStatusCode.NotFound)
                        throw ex;
                    else if (statusCode == HttpStatusCode.TooManyRequests)
                        LastRequest = DateTime.Now.AddSeconds(3);
                    else
                        throw ex;
                }
                catch (Exception error)
                {
                    throw error;
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
