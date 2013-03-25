using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models.DB
{
    public interface IDictionaryRepository : IRepositoryGuidID<Dictionary>
    {
        Dictionary FindByCode(string codeName, string userName);
    }

    public class DictionaryRepositoryImpl : RepositoryBase<Dictionary, Guid>, IDictionaryRepository
    {
        public DictionaryRepositoryImpl(IDbContextProvider dbContextProvider) : base(dbContextProvider) { }

        public Dictionary FindByCode(string codeName, string userName)
        {
            return All.Where(d => d.Account.UserName == userName).SingleOrDefault(d => d.Code == codeName);
        }
    }
}