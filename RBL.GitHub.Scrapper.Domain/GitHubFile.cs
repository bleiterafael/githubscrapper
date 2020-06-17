using System;

namespace RBL.GitHub.Scrapper.Domain
{
    public class GitHubFile : GitHubItem
    {
        public GitHubFile(string name, Extension extension, string urlAttribute)
            : base(name, urlAttribute)
        {
            this._extension = extension;
        }

        public GitHubFile(string name, Extension extension, string urlAttribute, int lines, decimal size)
            : base(name, urlAttribute)
        {
            this._extension = extension;
            this._lines = lines;
            this._size = size;
        }

        private Extension _extension { get; set; }
        private int _lines { get; set; }
        private decimal _size { get; set; }

        public Extension Extension { get { return this._extension; } }
        public int Lines { get { return this._lines; } }
        public decimal Size { get { return this._size; } }

        public void SetLines(int lines) { this._lines = lines; }
        public void SetSize(decimal size) { this._size = size; }

    }
}
