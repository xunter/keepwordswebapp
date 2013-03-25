using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models.DB
{
    public interface IAccountRepository : IRepositoryGuidID<Account>
    {
        Account FindByUserName(string userName);


    }
    
    public class AccountRepositoryImpl : RepositoryBase<Account, Guid>, IAccountRepository
    {
        public AccountRepositoryImpl(IDbContextProvider dbContextProvider) : base(dbContextProvider) { }

        public Account FindByUserName(string userName)
        {
            var account = All.Where(a => a.UserName == userName).SingleOrDefault();
            return account;
        }
    }
}