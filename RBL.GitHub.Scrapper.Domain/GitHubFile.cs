using System;

namespace RBL.GitHub.Scrapper.Domain
{
    public class GitHubFile : GitHubItem
    {
        public GitHubFile(string name, Extension extension, int lines, long size)
            : base(name)
        {
            this._extension = extension;
            this._lines = lines;
            this._size = size;
        }

        private Extension _extension { get; set; }
        private int _lines { get; set; }
        private long _size { get; set; }

        public Extension Extension { get { return this._extension; } }
        public int Lines { get { return this._lines; } }
        public long Size { get { return this._size; } }

    }
}
