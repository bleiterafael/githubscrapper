using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Models
{
    public class ScrappingInfoExtensionViewModel
    {
        public string Extension { get; set; }
        public int TotalFiles { get; set; }
        public int TotalLines { get; set; }
        public decimal TotalSize { get; set; }
        public string TotalSizeDescription { get; set; }

        public int ScrappingInfoId { get; set; }
        public int ScrappingInfoExtensionId { get; set; }
    }
}
