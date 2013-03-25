using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeepWords.Models.DB;
using KeepWords.Core.Web.Security;

namespace KeepWords.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IAuthService _authService;
        
        public HomeController(IAuthService authService, IDictionaryRepository dictionaryRepository)
        {
            _authService = authService;
            _dictionaryRepository = dictionaryRepository;
        }

        public ActionResult Index()
        {
            KeepWords.Core.Logging.LoggingService.Instance.Log.Trace("The Index page was requested.");
            var currAccount = _authService.CurrentAccount;
            if (currAccount == null)
            {
                return View();
            }
            else
            {
                if (currAccount.ActiveDictionaryID == null)
                {
                    return View("OfferToCreateDictionary");
                }
                else
                {
                    return RedirectToAction("List", "Word", new { dictionaryId = currAccount.ActiveDictionaryID });
                }

            }
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}
