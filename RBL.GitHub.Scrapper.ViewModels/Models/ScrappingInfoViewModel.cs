using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Models
{
    public class ScrappingInfoViewModel
    {
        public ScrappingInfoViewModel(string gitHubRepository, List<GitHubFileViewModel> fileInfos,TimeSpan _processTime)
        {
            this._gitHubRepository = gitHubRepository;
            this._totalLines = 0;
            this._totalSize = 0;
            this._processTime = _processTime;
            this._scrappingItems = new List<ScrappingInfoExtensionViewModel>();
            var groupItems = fileInfos.GroupBy(f => f.Extension.Description);
            foreach(var g in groupItems)
            {
                string extension = g.Key;
                int totalLinesItem = g.Sum(i => i.Lines);
                long totalSizeItem = g.Sum(i => i.Size);
                this._scrappingItems.Add(new ScrappingInfoExtensionViewModel(extension,totalLinesItem,totalSizeItem));

                this._totalLines += totalLinesItem;
                this._totalSize += totalSizeItem;
            }
        }

        private string _gitHubRepository { get; set; }
        private int _totalLines { get; set; }
        private long _totalSize { get; set; }
        private TimeSpan _processTime { get; set; }
        private List<ScrappingInfoExtensionViewModel> _scrappingItems { get; set; }

        public string GitHubRepository { get { return this._gitHubRepository; } }
        public int TotalLines { get { return this._totalLines; } }
        public long TotalSize { get { return this._totalSize; } }
        public string TotalSizeDescription { get { return $"{this._totalSize} bytes"; } }
        public string ProcessTime { get { return $"{Math.Round(this._processTime.TotalSeconds,2)} s"; } }
        public List<ScrappingInfoExtensionViewModel> ScrappingItems { get { return this._scrappingItems; } }
    }
}
