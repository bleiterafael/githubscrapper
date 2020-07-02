using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Models
{
    public class ScrappingInfoViewModel
    {

        public int ScrappingInfoId { get; set; }
        public string GitHubRepository { get; set; }
        public int TotalFiles { get; set; }
        public int TotalLines { get; set; }
        public decimal TotalSize { get; set; }
        public string TotalSizeDescription { get; set; }
        public DateTime LastUpdate { get; set; }
        public string ProcessTime { get; set; }

        public virtual ICollection<ScrappingInfoExtensionViewModel> ScrappingInfoExtensions { get; set; }

    }
}
