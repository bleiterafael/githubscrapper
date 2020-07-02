using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Domain
{
    public class ScrappingInfoExtension
    {
        public ScrappingInfoExtension()
        {

        }
        public ScrappingInfoExtension(string extension, int totalFiles, int totalLines, decimal totalSize)
        {
            this.Extension = extension;
            this.TotalFiles = totalFiles;
            this.TotalLines = totalLines;
            this.TotalSize = totalSize;
        }

        public int ScrappingInfoExtensionId { get; set; }
        public int ScrappingInfoId { get; set; }

        public string Extension { get; set; }
        public int TotalFiles { get; set; }
        public int TotalLines { get; set; }
        public decimal TotalSize { get; set; }
        public string TotalSizeDescription { get { return $"{this.TotalSize} bytes"; } }

        public virtual ScrappingInfo ScrappingInfo { get; set; }
    }
}
