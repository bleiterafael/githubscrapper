using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Domain
{
    public class GitHubItem
    {
        public GitHubItem(string name)
        {
            this._name = name;
        }
        private string _name { get; set; }
        private string Name { get { return this._name; } }
    }
}
