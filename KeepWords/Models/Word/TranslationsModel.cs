using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models.Word
{
    public class TranslationsModel
    {
        public DB.Word Word { get; set; }
        public List<DB.Translation> Translations { get; set; }
    }
}