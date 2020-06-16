using System;

namespace RBL.GitHub.Scrapper.Domain
{
    public class Extension
    {
        public Extension(string description)
        {
            this._description = description;
        }
        public string _description { get; set; }
        public string Description { get { return this._description; } }
    }
}
