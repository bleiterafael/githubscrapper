using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.ViewModels.Models
{
    public class GitHubFileViewModel : GitHubItemViewModel
    {
        public ExtensionViewModel Extension { get; set; }
        public int Lines { get; set; }
        public decimal Size { get; set; }
    }
}
