using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Models
{
    public class ScrappingInfoViewModel
    {
        public ScrappingInfoViewModel()
        {

        }

        public ScrappingInfoViewModel(string gitHubRepository, List<GitHubFileViewModel> files,DateTime lastUpdate,TimeSpan processTime)
        {
            this.GitHubRepository = gitHubRepository;
            this.TotalFiles = files.Count;
            this.TotalLines = 0;
            this.TotalSize = 0;
            this.LastUpdate = lastUpdate;
            this.SetProcessTime(processTime);
            this.ScrappingItems = new List<ScrappingInfoExtensionViewModel>();
            var groupItems = files.GroupBy(f => f.Extension.Description).OrderBy(g => g.Key);
            foreach(var g in groupItems)
            {
                string extension = g.Key;
                int totalFiles = g.Count();
                int totalLinesItem = g.Sum(i => i.Lines);
                long totalSizeItem = g.Sum(i => i.Size);
                this.ScrappingItems.Add(new ScrappingInfoExtensionViewModel(extension,totalFiles,totalLinesItem,totalSizeItem));

                this.TotalLines += totalLinesItem;
                this.TotalSize += totalSizeItem;
            }

            this.TotalSizeDescription = $"{this.TotalSize} bytes";
        }

        public string GitHubRepository { get; set; }
        public int TotalFiles { get; set; }
        public int TotalLines { get; set; }
        public long TotalSize { get; set; }
        public string TotalSizeDescription { get; set; }
        public DateTime LastUpdate { get; set; }
        public string ProcessTime { get; set; }
        public List<ScrappingInfoExtensionViewModel> ScrappingItems { get; set; }

        public void SetProcessTime(TimeSpan processTime)
        {
            this.ProcessTime = $"{processTime.TotalSeconds} s";
        }
    }
}
