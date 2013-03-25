using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace KeepWords.Models.DB
{
    public interface IWordRepository : IRepositoryGuidID<Word>
    {
        Word FindByText(string text);
        Word FindByTextForUser(string text, string userName);
    }
    
    public class WordRepositoryImpl : RepositoryBase<Word, Guid>, IWordRepository
    {
        public WordRepositoryImpl(IDbContextProvider dbContextProvider) : base(dbContextProvider) { }

        public Word FindByText(string text)
        {
            return All.SingleOrDefault(w => w.Original == text);
        }

        public Word FindByTextForUser(string text, string userName)
        {
            return All.Where(w => w.Dictionary.Account.UserName == userName).SingleOrDefault(w => w.Original == text); 
        }
    }
}