using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KeepWords.Models.DB;
using System.Web.Security;
using PavelNazarov.Common.Security;
using System.Net;
using Newtonsoft.Json.Linq;
using KeepWords.Core.Logging;
using System.Security.Cryptography;
using System.Globalization;
using NLog;

namespace KeepWords.Core.Web.Security
{
    public class AuthServiceImpl : IAuthService
    {
        protected readonly Logger log = KeepWords.Core.Logging.LoggingService.Instance.Log;
        private readonly IAccountRepository _accountRepository;

        public IDbContextProvider DBContextProvider { get; set; }

        public AuthServiceImpl(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        protected HttpContextBase HttpContext
        {
            get { return new HttpContextWrapper(System.Web.HttpContext.Current); }
        }

        public IResult SignIn(string userName, string pwdHash)
        {
            var failedSignInResult = new Result { Code = 0, Message = Messages.UserCredentialIsInvalid };
            try
            {
                if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(pwdHash))
                {
                    return failedSignInResult;
                }

                log.Trace("Fetching an existing account from DB matched the specified userName={0}...", userName);
                var existingUser = _accountRepository.FindByUserName(userName);

                if (existingUser == null)
                {
                    log.Trace("An existing account was not found.");
                    return failedSignInResult;
                }
                else
                {
                    log.Trace("An existing account was found.");
                    var salt = existingUser.Salt;
                    var hash = existingUser.Hash;
                    if (MembershipUtil.ValidatePasswordHashForAccount(pwdHash, salt, hash))
                    {
                        FormsAuthentication.SetAuthCookie(userName, true);
                        return Result.Success;
                    }
                    else
                    {
                        return failedSignInResult;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("A fatal error has occurred! Error is {0}", ex);
                log.Fatal(msg);
                return new Result { Code = -1001, Message = msg };
            }
        }

        public Result<Account> SignUp(string userName, string pwdHash, string displayName)
        {
            var failedSignUpResult = new Result<Account> { Code = 0, Message = Messages.UserAlreadyExists };
            var emptySignUpResult = new Result<Account> { Code = 0, Message = Messages.UserCredentialIsEmpty };
            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(pwdHash))
            {
                return emptySignUpResult;
            }

            var existingUser = _accountRepository.FindByUserName(userName);
            if (existingUser == null)
            {
                var salt = MembershipUtil.CreateSalt();
                var hashInBytes = MembershipUtil.GetHashAsBytes(pwdHash);
                var hashWithSalt = MembershipUtil.CreatePasswordHashWithSalt(hashInBytes, salt);
                var newAccount = new Account { UserName = userName, DisplayName = displayName, Hash = hashWithSalt, Salt = salt, InsertedOn = DateTime.Now, IsEnabled = true  };
                _accountRepository.InsertOrUpdate(newAccount);
                _accountRepository.SaveChanges();
                return Result.Generic(Result.Success, newAccount);
            }
            else
            {
                return failedSignUpResult;
            }
        }

        public IResult SignOut()
        {
            FormsAuthentication.SignOut();
            return Result.Success;
        }

        public IAccountRepository Repository
        {
            get { return _accountRepository; }
        }
        
        public Account CurrentAccount
        {
            get 
            {                
                if (HttpContext.Items["IsCurrentAccountNotNull"] != null && (bool)HttpContext.Items["IsCurrentAccountNotNull"] == true)
                {
                    return (Account)HttpContext.Items["CurrentAccount"];
                }
                else
                {
                    var account = GetCurrentAccount();
                    if (account != null)
                    {
                        HttpContext.Items["CurrentAccount"] = account;
                        HttpContext.Items["IsCurrentAccountNotNull"] = true;
                    }
                    return account;
                }
            }
        }

        private Account GetCurrentAccount()
        {
            var userName = HttpContext.User.Identity.Name;
            if (String.IsNullOrEmpty(userName)) return null;

            var foundAccount = _accountRepository.FindByUserName(userName);
            return foundAccount;
        }


        public IResult SignInSocial(string token, AccountType type)
        {
            try
            {
                string userName = null;
                switch (type)
                {
                    case AccountType.Facebook: userName = SignInFacebook(token);
                        break;
                    default: return new Result { Code = -1, Message = "An unrecognized type of the social login!" };
                }
                if (String.IsNullOrEmpty(userName))
                {
                    throw new NullReferenceException("A userName is null!");
                }
                FormsAuthentication.SetAuthCookie(userName, true);
                return Result.Success;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log.ErrorException("An error has occurred while sign in via Facebook.", ex);
                return new Result { Code = -1001, Message = "A fatal error has occurred!" };
            }
        }

        private string SignInFacebook(string token)
        {
            WebClient client = new WebClient();
            string JsonResult = client.DownloadString(string.Format(
                   "https://graph.facebook.com/me?access_token={0}&scope=user_birthday,user_hometown,email", token));
            // Json.Net is really helpful if you have to deal
            // with Json from .Net http://json.codeplex.com/
            JObject jsonUserInfo = JObject.Parse(JsonResult);
            // you can get more user's info here. Please refer to:
            //     http://developers.facebook.com/docs/reference/api/user/

            string facebook_userID = jsonUserInfo.Value<string>("id");
            string username = jsonUserInfo.Value<string>("username");
            var accountUserName = String.Format("__FB.{0}", username);
            var verifyHash = HashFactory<SHA1>.CreateText(JsonResult);
            var existingAccount = _accountRepository.FindByUserName(accountUserName);
            if (existingAccount != null && existingAccount.IsBuiltIn) throw new InvalidOperationException("The account for this username is a built in account!");
            if (existingAccount == null)
            {
                var profile = new Profile();
                profile.ExternalID = facebook_userID;
                string email = jsonUserInfo.Value<string>("email");
                string locale = jsonUserInfo.Value<string>("locale");
                profile.City = jsonUserInfo.Value<string>("hometown");
                profile.FullName = jsonUserInfo.Value<string>("name");
                profile.FirstName = jsonUserInfo.Value<string>("first_name");
                profile.LastName = jsonUserInfo.Value<string>("last_name");
                profile.MiddleName = jsonUserInfo.Value<string>("middle_name");
                profile.Gender = jsonUserInfo.Value<string>("gender");
                string birthdaytStr = jsonUserInfo.Value<string>("birthday");
                if (!String.IsNullOrEmpty(birthdaytStr))
                {
                    DateTime birthdayDate = DateTime.ParseExact(birthdaytStr, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    profile.Birthday = birthdayDate;
                }
                profile.Email = email;
                profile.VerifyHash = verifyHash;
                /*
                string pictureUri = jsonUserInfo.Value<string>("picture");
                var pictureRequest = WebRequest.Create(pictureUri);
                using (var response = pictureRequest.GetResponse())
                {
                    byte[] buffer = new byte[response.ContentLength];
                    response.GetResponseStream().Read(buffer, 0, buffer.Length);
                    var file = new File { OriginalUri = pictureUri, Size = response.ContentLength, MimeType = response.ContentType, BinaryData = buffer };
                    var image = new Image { FileInstance = file };
                    profile.Picture = image;
                }
                */
                var fakeSalt = MembershipUtil.CreateSalt();
                var fakePwd = MembershipUtil.CreateRNDPasswordAsBase64();
                var fakePwdHashAsBytes = Convert.FromBase64String(fakePwd);
                var fakePwdHashWithSalt = MembershipUtil.CreatePasswordHashWithSaltAsString(fakePwdHashAsBytes, fakeSalt);
                var fakePwdHashWithSaltAsBytes = MembershipUtil.GetHashAsBytes(fakePwdHashWithSalt);
                var account = new Account { UserName = accountUserName, Hash = fakePwdHashWithSaltAsBytes, Salt = fakeSalt, Type = AccountType.Facebook, DisplayName = profile.FullName, ProfileInstance = profile, InsertedOn = DateTime.Now, IsEnabled = true };
                _accountRepository.InsertOrUpdate(account);
                _accountRepository.SaveChanges();
            }
            else if (existingAccount.ProfileInstance.VerifyHash != verifyHash)
            {
                var profile = existingAccount.ProfileInstance;
                profile.ExternalID = facebook_userID;
                string email = jsonUserInfo.Value<string>("email");
                string locale = jsonUserInfo.Value<string>("locale");
                profile.City = jsonUserInfo.Value<string>("hometown");
                profile.FullName = jsonUserInfo.Value<string>("name");
                profile.FirstName = jsonUserInfo.Value<string>("first_name");
                profile.LastName = jsonUserInfo.Value<string>("last_name");
                profile.MiddleName = jsonUserInfo.Value<string>("middle_name");
                profile.Gender = jsonUserInfo.Value<string>("gender");
                string birthdaytStr = jsonUserInfo.Value<string>("birthday");
                if (!String.IsNullOrEmpty(birthdaytStr))
                {
                    DateTime birthdayDate = DateTime.ParseExact(birthdaytStr, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    profile.Birthday = birthdayDate;
                }
                profile.Email = email;
                profile.VerifyHash = verifyHash;
                /*
                string pictureUri = jsonUserInfo.Value<string>("picture");
                var pictureRequest = WebRequest.Create(pictureUri);
                using (var response = pictureRequest.GetResponse())
                {
                    byte[] buffer = new byte[response.ContentLength];
                    response.GetResponseStream().Read(buffer, 0, buffer.Length);
                    var file = new File { OriginalUri = pictureUri, Size = response.ContentLength, MimeType = response.ContentType, BinaryData = buffer };
                    var image = new Image { FileInstance = file };
                    profile.Picture = image;
                }
                */

                existingAccount.DisplayName = profile.FullName;
                _accountRepository.InsertOrUpdate(existingAccount);
                _accountRepository.SaveChanges();  
            }

            return accountUserName;
        }
    }
}