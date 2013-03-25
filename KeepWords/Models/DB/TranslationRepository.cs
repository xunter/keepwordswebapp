using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models.DB
{
    public interface ITranslationRepository : IRepositoryGuidID<Translation>
    {
    }

    public class TranslationRepositoryImpl : RepositoryBase<Translation, Guid>, ITranslationRepository
    {
        public TranslationRepositoryImpl(IDbContextProvider dbContextProvider) : base(dbContextProvider) { }
    }
}