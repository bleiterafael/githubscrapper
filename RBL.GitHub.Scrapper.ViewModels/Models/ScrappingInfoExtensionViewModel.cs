using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Models
{
    public class ScrappingInfoExtensionViewModel
    {
        public ScrappingInfoExtensionViewModel(string extension,int totalFiles, int totalLines, long totalSize)
        {
            this._extension = extension;
            this._totalFiles = totalFiles;
            this._totalLines = totalLines;
            this._totalSize = totalSize;
        }

        private string _extension { get; set; }
        private int _totalFiles { get; set; }
        private int _totalLines { get; set; }
        private long _totalSize { get; set; }

        public string Extension { get { return this._extension; } }
        public int TotalFiles { get { return this._totalFiles; } }
        public int TotalLines { get { return this._totalLines; } }
        public long TotalSize { get { return this._totalSize; } }
        public string TotalSizeDescription { get { return $"{this._totalSize} bytes"; } }
    }
}
