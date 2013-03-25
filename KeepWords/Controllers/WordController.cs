using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeepWords.Core;
using KeepWords.Core.Repositories;
using KeepWords.Core.Translating;
using KeepWords.Core.Web.Security;
using KeepWords.Models;
using KeepWords.Models.DB;
using KeepWords.Models.Word;
using KeepWords.Models.WordViewModels;
using KeepWords.Core.Logging;

namespace KeepWords.Controllers
{
    [Authorize]
    public class WordController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IWordRepository _wordRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly ITranslationRepository _translationRepository;
        private readonly ILanguageInfosRepository _languageInfosRepository;
        private ITranslationService _translationService = new BingTranslationService();

        
        //
        // GET: /Word/

        public WordController(IAuthService authService, IWordRepository wordRepository, IAccountRepository accountRepository, ILanguageInfosRepository languageInfosRepository, IDictionaryRepository dictionaryRepository, ITranslationRepository translationRepository)
        {
            _authService = authService;
            _wordRepository = wordRepository;
            _accountRepository = accountRepository;
            _dictionaryRepository = dictionaryRepository;
            _translationRepository = translationRepository;
            _languageInfosRepository = languageInfosRepository;
        }

        public ActionResult Index()
        {
            var activeDictID = _authService.CurrentAccount.ActiveDictionaryID;
            return RedirectToAction("List", new { dictionaryId = activeDictID });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(SearchQueryModel model)
        {
            if (model == null || String.IsNullOrEmpty(model.QueryText)) return RedirectToAction("Index", "Home");
            var account = _authService.CurrentAccount;
            var currDictionary = account.ActiveDictionary;
            if (currDictionary == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var searchText = model.QueryText;
            bool backTranslation = false;
            string detectedLangCacheKey = String.Format("detectedLangCacheKey_{0}", searchText);
            string detectedLang = null;
            string cachedDetectedLang = HttpContext.Cache[detectedLangCacheKey] as String;
            if (cachedDetectedLang == null)
            {
                try
                {
                    detectedLang = _translationService.DetectLanguage(searchText);
                    HttpContext.Cache.Add(detectedLangCacheKey, detectedLang, null, DateTime.Now.AddMonths(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                }
                catch (TranslatorException)
                { }
            }
            else
            {
                detectedLang = cachedDetectedLang;
            }
            if (!String.IsNullOrEmpty(detectedLang))
            {
                var languageInfo = _languageInfosRepository.All.Single(l => l.Lang == detectedLang);
                if (currDictionary.From != detectedLang)
                {
                    backTranslation = true;
                }
            }
            var wordsBelongToUser = currDictionary.SearchWords(searchText, backTranslation);
            var resultList = new List<SearchResultModel>();
            bool isStrictCoincidence = wordsBelongToUser.Length > 0;
            if (isStrictCoincidence)
            {
                var foundWord = wordsBelongToUser.Single();
                var searchResultModel = new WordSearchResultModel { WordInstance = foundWord, IsStrictCoincidence = true, IsBackTranslation = backTranslation };
                resultList.Add(searchResultModel);
            }
            else
            {
                var otherDictionaries = _dictionaryRepository.All.Where(d => d.From == currDictionary.From && d.To == currDictionary.To).ToArray();
                var linkListOthersWords = new LinkedList<Word>();
                foreach (var otherDict in otherDictionaries)
                {
                    var foundOthersWords = otherDict.SearchWords(searchText, backTranslation);
                    Array.ForEach(foundOthersWords, w => linkListOthersWords.AddLast(w));
                }
                if (linkListOthersWords.Count > 0)
                {
                    var othersDictsSearchResults = linkListOthersWords.Select(w => new WordSearchResultModel { IsStrictCoincidence = true, WordInstance = w }).ToArray();
                    model.OthersDictionariesResults = othersDictsSearchResults;
                }
                else
                {
                    var cacheKey = String.Format("externalTranslationServiceSearchResultModel_{0}", searchText);
                    var translationResultFromCache = HttpContext.Cache[cacheKey] as ExternalTranslationServiceSearchResultModel;
                    if (translationResultFromCache == null)
                    {
                        //string fromCode = _languageInfosRepository.All.Single(l => l.Lang == currDictionary.From).Lang;
                        //string toCode = _languageInfosRepository.All.Single(l => l.Lang == currDictionary.To).Lang;
                        string from = backTranslation ? currDictionary.To : currDictionary.From;
                        string to = backTranslation ? currDictionary.From : currDictionary.To;
                        try
                        {
                            var translations = _translationService.TranslateText(from, to, searchText);
                            if (translations.Length > 0)
                            {
                                translationResultFromCache = new ExternalTranslationServiceSearchResultModel { OriginalText = searchText, Translations = translations, IsBackTranslation = backTranslation };
                                HttpContext.Cache.Add(cacheKey, translationResultFromCache, null, DateTime.Now.AddMonths(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Low, null);
                            }
                        }
                        catch (TranslatorException)
                        { 
                        }
                    }
                    if (translationResultFromCache != null)
                    {
                        model.TranslationServiceResult = translationResultFromCache;
                    }
                    else
                    {
                        var wordsWhichContainsTheSearchText = currDictionary.Words.Where(w => w.Original.ToLower().Contains(searchText.ToLower())).Except(wordsBelongToUser).ToArray();
                        var wordsBelongToUserResult = wordsBelongToUser.Select(w => new WordSearchResultModel { WordInstance = w }).ToArray();
                        resultList.AddRange(wordsBelongToUserResult);
                    }
                }
                //model.TranslationText = translator.Translate(searchText, currDictionary.From, currDictionary.To);
            }
            ModelState.Remove("Results");
            model.Results = resultList;
            return View(model);
        }

        public class AddWordModel
        {
            public string Word { get; set; }
            public string Translation { get; set; }
            public string DictionaryCode { get; set; }
        }

        public ActionResult List(Guid? dictionaryId)
        {
            var specifiedDictionary = dictionaryId.HasValue ? _dictionaryRepository.Find(dictionaryId.Value) : null;
            IEnumerable<Dictionary> dictionariesToView = null;
            if (specifiedDictionary != null) dictionariesToView = new[] { specifiedDictionary };
            else dictionariesToView = _dictionaryRepository.All.Where(d => d.AccountID == _authService.CurrentAccount.ID).ToList();
            ViewData["account"] = _authService.CurrentAccount;
            return View(dictionariesToView);
        }

        public ActionResult Remove(Guid id, Guid? dictionaryId)
        {
            _wordRepository.Delete(id);
            _wordRepository.SaveChanges();
            return RedirectToAction("List", new { dictionaryId = dictionaryId });
        }

        public ActionResult Add(Guid dictionaryId, string wordText)
        {
            var word = new Word() { DictionaryID = dictionaryId, InsertedOn = DateTime.Now };
            if (!String.IsNullOrEmpty(wordText)) word.Original = wordText;
            return View("Edit", word);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Guid dictionaryId, Word word)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(String.Empty, Messages.FieldsInvalid);
                return View("Edit", word);
            }
            word.Dictionary = _dictionaryRepository.Find(dictionaryId);
            try
            {
                var pronounceFile = _translationService.GetPronounceAudioFile(word.Original, word.Dictionary.From, "mp3", String.Empty);
                word.PronounceAudioFile = pronounceFile;
            }
            catch (TranslatorException)
            { }
            if (word.InsertedOn == default(DateTime)) word.InsertedOn = DateTime.Now;
            _wordRepository.InsertOrUpdate(word);
            _wordRepository.SaveChanges();
            return RedirectToAction("Translations", new { id = word.ID, afterSave = true });
        }

        public ActionResult Translations(Guid id, bool? afterSave)
        {
            var word = _wordRepository.Find(id);
            var activeDict = _authService.CurrentAccount.ActiveDictionary;
            var model = new TranslationsModel { Word = word };
            var currentWordTranslations = word.Translations.ToList();
            model.Translations = currentWordTranslations;
            if (afterSave.HasValue && afterSave.Value && currentWordTranslations.Count == 0)
            {
                try
                {
                    var translationsFromTranslator = _translationService.TranslateText(activeDict.From, activeDict.To, word.Original);
                    if (translationsFromTranslator.Length > 0)
                    {
                        foreach (var translationFromTranslator in translationsFromTranslator)
                        {
                            var newTranslation = new Translation { Word = word, Value = translationFromTranslator };
                            currentWordTranslations.Add(newTranslation);
                        }
                    }
                    else
                    {
                        var newTranslation = new Translation { Word = word };
                        currentWordTranslations.Add(newTranslation);
                    }

                }
                catch (TranslatorException)
                {
                    var newTranslation = new Translation { Word = word };
                    currentWordTranslations.Add(newTranslation); 
                }
            }
            return View(model);
        }

        public ActionResult RemoveTranslation(Guid id, Guid translationId)
        {
            _translationRepository.Delete(translationId);
            _translationRepository.SaveChanges();
            return RedirectToAction("Translations", new { id = id });
        }

        [HttpPost]
        public ActionResult SaveAndAddTranslation(Guid id, List<Translation> translations, string isSave)
        {
            var word = _wordRepository.Find(id);
            ModelState.Clear();
            var model = new TranslationsModel { Word = word, Translations = translations };
            if (translations == null)
                model.Translations = word.Translations.ToList();
            if (translations != null && translations.Any(t => t.ID == Guid.Empty) && word.Translations.Any(t => translations.Single(tt => tt.ID == Guid.Empty).Value.ToLower() == t.Value.ToLower()))
            {
                model.Translations = word.Translations.ToList();
                var newTranslation = new Translation { Word = word };
                model.Translations.Add(newTranslation);
                ModelState.AddModelError(String.Empty, "This translation already exists.");
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(String.Empty, Messages.FieldsInvalid);
                return View("Translations", model);
            }
            if (translations != null)
            {
                var translationsWithValues = translations.Where(t => t.Value != "__IGNORE").ToArray();
                bool isTranslationsAdded = false;
                foreach (var translation in translationsWithValues)
                {
                    var doesTranslationNotExist = !word.Translations.Any(t => t.ID == translation.ID);
                    if (doesTranslationNotExist)
                    {
                        word.Translations.Add(translation);
                        isTranslationsAdded = true;
                    }
                }
                if (isTranslationsAdded)
                {
                    _wordRepository.SaveChanges();
                }
                model.Translations = word.Translations.ToList();
            }
            if (isSave == "1")
            {
                return RedirectToAction("List", new { dictionaryId = word.DictionaryID });
            }
            else
            {
                var newTranslation = new Translation { Word = word };
                model.Translations.Add(newTranslation);
                return View("Translations", model); 
            }
        }

        [HttpPost]
        public JsonResult SearchWordsJson(Guid dictionaryId, string wordToMatch)
        {
            var dictionary = _dictionaryRepository.Find(dictionaryId);
            var found = dictionary.Words.Where(w => String.Compare(w.Original, wordToMatch, true) == 0).ToArray().Select(w => WordViewModel.CreateFromEntity(w));
            return Json(found);
        }

        [HttpPost]
        public PartialViewResult FoundWordsPartial(WordViewModel[] words, Guid dictionaryId)
        {
            var wordsFromDB = _wordRepository.All.ToArray().Where(w => words.Any(wvm => wvm.ID == w.ID));
            return PartialView(wordsFromDB);
        }

        public ActionResult AddOther(Guid id)
        {
            var word = _wordRepository.Find(id);
            if (word == null) return RedirectToAction("Index");
            var activeDict = _authService.CurrentAccount.ActiveDictionary;
            var wordTranslations = word.Translations.ToArray();
            var newWord = new Word { Original = word.Original, InsertedOn = DateTime.Now };
            try
            {
                var pronounceFile = _translationService.GetPronounceAudioFile(word.Original, activeDict.From, "mp3", String.Empty);
                newWord.PronounceAudioFile = pronounceFile;

            }
            catch (TranslatorException)
            {
            }
            activeDict.Words.Add(newWord);
            newWord.Translations = wordTranslations;
            _wordRepository.SaveChanges();
            return RedirectToAction("List", new { dictionaryId = activeDict.ID });
        }

        public ActionResult AddFromTranslator(string wordText, string translationsConcated)
        {
            try
            {
                var activeDict = _authService.CurrentAccount.ActiveDictionary;
                var translations = translationsConcated.Split(';');
                string firstTranslation = translations.First();
                var newWord = new Word { Original = wordText, InsertedOn = DateTime.Now };
                try
                {
                    var pronounceFile = _translationService.GetPronounceAudioFile(wordText, activeDict.From, "mp3", String.Empty);
                    newWord.PronounceAudioFile = pronounceFile;
                }
                catch (TranslatorException)
                {
                }
                activeDict.Words.Add(newWord);
                newWord.Translations = translations.Select(t => new Translation { Value = t }).ToArray();
                _wordRepository.SaveChanges();
                return RedirectToAction("List", new { dictionaryId = activeDict.ID });
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log.Fatal(String.Format("A fatal error has occurred while adding a new word using an external translator service. The new word is {0}. The error is {1}", wordText, ex));
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult PronounceAudioFile(Guid id)
        {
            var word = _wordRepository.Find(id);
            if (word == null) return new HttpStatusCodeResult(404);
            if (word.PronounceAudioFileID.HasValue)
            {
                var pronounceFile = word.PronounceAudioFile;
                return File(pronounceFile.BinaryData, pronounceFile.MimeType, pronounceFile.FileName);
            }
            else
            {
                return new HttpStatusCodeResult(404);
            }
        }
    }
}
