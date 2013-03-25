using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeepWords.Models.DB;
using KeepWords.Core.Web.Security;
using KeepWords.Core;
using KeepWords.Core.Repositories;

namespace KeepWords.Controllers
{
    [Authorize]
    public class DictionaryController : Controller
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IAuthService _authService;
        private readonly ILanguageInfosRepository _languageInfosRepository;

        public DictionaryController(IDictionaryRepository dictionaryRepository, IAuthService accountRepository, ILanguageInfosRepository languageInfosRepository)
        {
            _dictionaryRepository = dictionaryRepository;
            _authService = accountRepository;
            _languageInfosRepository = languageInfosRepository;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index");
        }

        public ActionResult List()
        {
            var dictionariesThatMathesAccount = _dictionaryRepository.All.Where(d => d.Account.ID == _authService.CurrentAccount.ID).ToList();
            return View(dictionariesThatMathesAccount);
        }

        public ActionResult Add()
        {
            var dictionary = new Dictionary();
            ViewData["languages"] = _languageInfosRepository.All;
            return View("Edit", dictionary);
        }

        public ActionResult Edit(Guid id)
        {
            var dictionary = _dictionaryRepository.Find(id);
            if (dictionary == null || dictionary.Account.ID != _authService.CurrentAccount.ID) return RedirectToAction("List");
            return View(dictionary);
        }

        [HttpPost]
        public ActionResult Save(Guid? id, Dictionary model)
        {
            bool isAdding = id == null || id == Guid.Empty;

            if (ModelState.IsValid == false)
            {
                ModelState.AddModelError(String.Empty, Messages.FieldsInvalid);
                ViewData["languages"] = _languageInfosRepository.All;
                return View("Edit", model);
            }


            var fromLangInfo = _languageInfosRepository.All.Single(l => l.ID == model.From);
            var toLangInfo = _languageInfosRepository.All.Single(l => l.ID == model.To);


            if (isAdding)
            {
                if (_authService.CurrentAccount.Dictionaries.Any(d => fromLangInfo.Lang == d.From && toLangInfo.Lang == d.To))
                {
                    ModelState.AddModelError(String.Empty, String.Format("You already have got the {0} dictionary!", model.FromToDisplayName));
                    ViewData["languages"] = _languageInfosRepository.All;
                    return View("Edit", model);
                }
            }
            
            model.FromToDisplayName = String.Format("{0} - {1}", fromLangInfo.NativeName, toLangInfo.NativeName);
            if (String.IsNullOrEmpty(model.Name))
            {
                model.Name = model.FromToDisplayName;
            }
            model.From = fromLangInfo.Lang;
            model.To = toLangInfo.Lang;
            model.Account = _authService.CurrentAccount;
            _dictionaryRepository.InsertOrUpdate(model);            
            _dictionaryRepository.SaveChanges();
            if (_authService.CurrentAccount.ActiveDictionary == null)
            {
                _authService.CurrentAccount.ActiveDictionary = model;
                _authService.Repository.InsertOrUpdate(_authService.CurrentAccount);
                _authService.Repository.SaveChanges();
            }
            return RedirectToAction("List");
        }

        public ActionResult Delete(Guid id)
        {
            var dictionary = _dictionaryRepository.Find(id);
            if (dictionary == null || dictionary.AccountID != _authService.CurrentAccount.ID) return RedirectToAction("List");
            _dictionaryRepository.Delete(id);
            _dictionaryRepository.SaveChanges();
            var first = _authService.CurrentAccount.Dictionaries.FirstOrDefault();
            if (first != null)
            {
                return RedirectToAction("SetActive", new { id = first.ID });
            }
            return RedirectToAction("List");
        }

        [ChildActionOnly]
        public PartialViewResult DictionaryNavigationItem()
        {
            var currAccount = _authService.CurrentAccount;
            var dictionaries = _dictionaryRepository.All.Where(d => d.AccountID == currAccount.ID).ToArray();
            return PartialView(dictionaries);
        }

        public ActionResult SetActive(Guid id)
        {
            var currAccount = _authService.CurrentAccount;
            var newActiveDictionary = _dictionaryRepository.Find(id);
            currAccount.ActiveDictionaryID = newActiveDictionary.ID;
            currAccount.ActiveDictionary = newActiveDictionary;
            _authService.Repository.InsertOrUpdate(currAccount);
            _authService.Repository.SaveChanges();
            return RedirectToAction("List", "Word", new { dictionaryId = id });
        }
    }
}
