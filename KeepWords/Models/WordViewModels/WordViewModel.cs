using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KeepWords.Models.DB;

namespace KeepWords.Models.WordViewModels
{
    [Serializable]
    public class WordViewModel
    {
        public Guid ID { get; set; }
        public string Original { get; set; }
        public Guid DictionaryID { get; set; }

        public static WordViewModel CreateFromEntity(DB.Word word)
        {
            return new WordViewModel { ID = word.ID, Original = word.Original, DictionaryID = word.DictionaryID };
        }
    }
}