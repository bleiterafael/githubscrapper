using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBL.GitHub.Scrapper.Domain
{
    public class ScrappingInfo
    {
        public ScrappingInfo()
        {

        }

        public ScrappingInfo(string gitHubRepository, List<GitHubFile> files,DateTime lastUpdate,TimeSpan processTime)
        {
            this.GitHubRepository = gitHubRepository;
            this.TotalFiles = files.Count;
            this.TotalLines = 0;
            this.TotalSize = 0;
            this.LastUpdate = lastUpdate;
            this.SetProcessTime(processTime);
            this.ScrappingInfoExtensions = new List<ScrappingInfoExtension>();
            var groupItems = files.GroupBy(f => f.Extension.Description).OrderBy(g => g.Key);
            foreach(var g in groupItems)
            {
                string extension = g.Key;
                int totalFiles = g.Count();
                int totalLinesItem = g.Sum(i => i.Lines);
                decimal totalSizeItem = g.Sum(i => i.Size);
                this.ScrappingInfoExtensions.Add(new ScrappingInfoExtension(extension,totalFiles,totalLinesItem,totalSizeItem));

                this.TotalLines += totalLinesItem;
                this.TotalSize += totalSizeItem;
            }

            this.TotalSizeDescription = $"{this.TotalSize} bytes";
        }

        public int ScrappingInfoId { get; set; }
        public string GitHubRepository { get; set; }
        public int TotalFiles { get; set; }
        public int TotalLines { get; set; }
        public decimal TotalSize { get; set; }
        public string TotalSizeDescription { get; set; }
        public DateTime LastUpdate { get; set; }
        public string ProcessTime { get; set; }

        
        public virtual ICollection<ScrappingInfoExtension> ScrappingInfoExtensions { get; set; }

        public void SetProcessTime(TimeSpan processTime)
        {
            this.ProcessTime = $"{processTime.TotalSeconds} s";
        }
    }
}
