using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Business.Validations
{
    public class ValidationResult
    {
        public List<string> Errors { get; set; }
        public ValidationResult()
        {
            Errors = new List<string>();
        }
    }
}
