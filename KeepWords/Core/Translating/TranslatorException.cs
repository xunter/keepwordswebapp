using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Core.Translating
{
    public class TranslatorException : ApplicationException
    {
        public TranslatorException(string message)
            : base(message)
        { }

        public TranslatorException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}