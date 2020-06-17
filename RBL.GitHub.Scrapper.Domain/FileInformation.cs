using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Domain
{
    public class FileInformations
    {
        public FileInformations(int lines, decimal size)
        {
            this._lines = lines;
            this._size = size;
        }

        private int _lines { get; set; }
        private decimal _size { get; set; }

        public int Lines { get { return _lines; } }
        public decimal Size { get { return _size; } }
    }
}
