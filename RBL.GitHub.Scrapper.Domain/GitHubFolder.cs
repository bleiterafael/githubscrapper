using System;
using System.Collections.Generic;

namespace RBL.GitHub.Scrapper.Domain
{
    public class GitHubFolder : GitHubItem
    {
        public GitHubFolder(string name,string urlAttribute)
            : base(name,urlAttribute)
        {
        }

        public List<GitHubItem> Items { get; set; }
    }
}
