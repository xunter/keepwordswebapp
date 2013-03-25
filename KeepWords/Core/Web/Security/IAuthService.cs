using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeepWords.Models.DB;

namespace KeepWords.Core.Web.Security
{
    public interface IAuthService
    {
        IAccountRepository Repository { get; }
        Account CurrentAccount { get; }
        IResult SignIn(string userName, string pwdHash);
        Result<Account> SignUp(string userName, string pwdHash, string displayName);
        IResult SignOut();
        IResult SignInSocial(string token, AccountType type);
    }
}
