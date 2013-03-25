using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeepWords.Core.Web.Security;
using KeepWords.Models.Account;
using KeepWords.Core.Web;
using PavelNazarov.Common;
using KeepWords.Core;
using PavelNazarov.Common.Web.MVC;
using PavelNazarov.Common.Security;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Web.Security;
using NLog;

namespace KeepWords.Controllers
{
    public class AccountController : Controller
    {
        protected readonly Logger _logger = KeepWords.Core.Logging.LoggingService.Instance.Log;
        const int RequiredPasswordLength = 6;
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        public ActionResult SignIn()
        {
            _logger.Trace("The SignIn page was requested.");
            return this.UniversalView(new SignInModel());
        }

        [HttpPost]
        public ActionResult SignIn(SignInModel model, string ReturnUrl)
        {
            _logger.Trace("The post data from the SignIn page were received. ReturnUrl is {0}", String.IsNullOrEmpty(ReturnUrl) ? String.Empty : ReturnUrl);
            if (String.IsNullOrEmpty(model.PasswordHash))
            {
                ModelState.AddModelError("PasswordOriginal", "Required field");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(String.Empty, Messages.FieldsInvalid);                
                return this.UniversalView(model);
            }

            _logger.Trace("Performing the SignIn operation for the user={0}...", model.UserName);
            var signInResult = _authService.SignIn(model.UserName, model.PasswordHash);
            if (signInResult.Code == (int)ResultCodes.Success)
            {
                _logger.Trace("The SignIn operation was successfully completed.");
                var redirectUrl = ReturnUrl != null ? ReturnUrl : Url.Action("Index", "Home");
                return this.UniversalRedirect(redirectUrl);
            }
            else
            {
                _logger.Trace("The SignIn operation was completed with code={0}.", signInResult.Code);
                if (signInResult.Code == (int)ResultCodes.UserError)
                {
                    ModelState.AddModelError(String.Empty, signInResult.Message);
                }
                else
                {
                    ModelState.AddModelError(String.Empty, Messages.FatalError);
                }
                return this.UniversalView(model);
            }
        }

        public ActionResult SignUp()
        {
            var saltBytes = MembershipUtil.CreateSalt();
            var saltString = PavelNazarov.Common.BytesUtil.BytesToString(saltBytes);
            ViewData["salt"] = saltString;
            return this.UniversalView(new SignUpModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpModel model)
        {
            if (String.IsNullOrEmpty(model.PasswordHash)) ModelState.AddModelError("PasswordHash", "Required field");
            if (String.IsNullOrEmpty(model.PasswordHashReply)) ModelState.AddModelError("PasswordHashReply", "Required field");

            if (String.IsNullOrEmpty(model.UserName))
            {
                ModelState.AddModelError("UserName", "The field UserName is required!");
            }
            if (ModelState.IsValid && model.PasswordHash != model.PasswordHashReply)
            {
                ModelState.AddModelError(String.Empty, "Passwords aren't equal!");
            }

            string pwdLenStr = model.PasswordOriginal;
            if (!String.IsNullOrEmpty(pwdLenStr))
            {
                int pwdLen = Convert.ToInt32(pwdLenStr);
                if (pwdLen < RequiredPasswordLength)
                {
                    ModelState.AddModelError("PasswordOriginal", "Password length must be 6 or greater");
                }
            }            

            if (!ModelState.IsValid)
            {

                ModelState.AddModelError(String.Empty, Messages.FieldsInvalid);
                return this.UniversalView(model);
            }
            
            var signInResult = _authService.SignUp(model.UserName, model.PasswordHash, model.DisplayName);
            if (signInResult.Code == (int)ResultCodes.Success)
            {
                _authService.SignIn(model.UserName, model.PasswordHash);
                var redirectUrl = Url.Action("SignUpSuccess", new { id = signInResult.Data.ID });
                return this.UniversalRedirect(redirectUrl);
            }
            else
            {
                if (signInResult.Code == (int)ResultCodes.UserError)
                {
                    ModelState.AddModelError(String.Empty, signInResult.Message);
                }
                else
                {
                    ModelState.AddModelError(String.Empty, Messages.FatalError);
                }
                return this.UniversalView(model);
            }
        }

        public ActionResult SignUpSuccess(Guid id)
        {
            var account = _authService.Repository.Find(id);
            if (account == null) return RedirectToAction("SignUp");
            return View(account);
        }

        public ActionResult FacebookLogin(string token)
        {
            _authService.SignInSocial(token, Models.DB.AccountType.Facebook);
            return RedirectToAction("Index", "Home");

        }

        public ActionResult SignOut()
        {
            var result = _authService.SignOut();
            return this.UniversalRedirect(Url.Action("Index", "Home"));
        }

        public ActionResult ChangePassword()
        {
            var currAccount = _authService.CurrentAccount;
            return View(new ChangePasswordModel { AccountID = currAccount.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var account = _authService.CurrentAccount;
            if (String.IsNullOrEmpty(model.CurrentPasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "The field is required.");
            }
            else
            {
                bool isCurrentPasswordValid = MembershipUtil.ValidatePasswordHashForAccount(model.CurrentPasswordHash, account.Salt, account.Hash);
                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "The entered current password doesn't equal with the actual current password!");
                }
            }

            bool newPasswordsAreFilled = true;
            if (String.IsNullOrEmpty(model.NewPasswordHash))
            {
                newPasswordsAreFilled = false;
                ModelState.AddModelError("NewPassword", "The field is required.");
            }
            if (String.IsNullOrEmpty(model.NewPasswordReplyHash))
            {
                newPasswordsAreFilled = false;
                ModelState.AddModelError("NewPasswordReply", "The field is required.");
            }
            if (newPasswordsAreFilled)
            {
                if (model.NewPasswordHash != model.NewPasswordReplyHash)
                {
                    ModelState.AddModelError("NewPassword", "New password and reply must be equal.");
                }
                else
                {
                    if (model.NewPasswordLength < RequiredPasswordLength)
                    {
                        ModelState.AddModelError("NewPassword", String.Format("The new password length must be equal {0} or more.", RequiredPasswordLength));
                    }
                    else if (model.NewPasswordHash == model.CurrentPasswordHash)
                    {
                        ModelState.AddModelError("NewPassword", "New password must be other than the current password.");
                    }
                }
            }

            if (ModelState.IsValid == false)
            {
                ModelState.AddModelError(String.Empty, Messages.FieldsInvalid);
                return View(model);
            }
            byte[] newPasswordHashAsBytes = MembershipUtil.GetHashAsBytes(model.NewPasswordHash);
            account.Hash = MembershipUtil.CreatePasswordHashWithSalt(newPasswordHashAsBytes, account.Salt);
            _authService.Repository.InsertOrUpdate(account);
            _authService.Repository.SaveChanges();
            ViewData["account"] = account;
            return RedirectToAction("Index", "Home");
        }
    }
}
