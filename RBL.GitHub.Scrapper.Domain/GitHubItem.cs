using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Domain
{
    public class GitHubItem
    {
        public GitHubItem(string name,string urlAttribute)
        {
            this._name = name;
            this._urlAttribute = urlAttribute;
        }
        private string _name { get; set; }
        private string _urlAttribute { get; set; }
        private string Name { get { return this._name; } }
        public string UrlAttribute { get { return this._urlAttribute; } }
    }
}
