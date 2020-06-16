using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Domain
{
    public class FileInformations
    {
        public FileInformations(int lines, long size)
        {
            this._lines = lines;
            this._size = size;
        }

        private int _lines { get; set; }
        private long _size { get; set; }

        public int Lines { get { return _lines; } }
        public long Size { get { return _size; } }
    }
}
