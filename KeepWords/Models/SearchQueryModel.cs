using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models
{


    public class SearchQueryModel
    {
        public string QueryText { get; set; }
        public IEnumerable<SearchResultModel> Results { get; set; }
        public ExternalTranslationServiceSearchResultModel TranslationServiceResult { get; set; }
        public SearchResultModel[] OthersDictionariesResults { get; set; }
        public bool IsEmpty 
        {
            get { return (Results == null || Results.Count() == 0) && TranslationServiceResult == null && OthersDictionariesResults == null; }
        }
    }

    public abstract class SearchResultModel
    {
        public bool IsBackTranslation { get; set; }
    }

    public class WordSearchResultModel : SearchResultModel
    {
        public DB.Word WordInstance { get; set; }
        public bool IsStrictCoincidence { get; set; }       
    }

    public class ExternalTranslationServiceSearchResultModel : SearchResultModel
    {
        public string OriginalText { get; set; }
        public string[] Translations { get; set; }
    }
}