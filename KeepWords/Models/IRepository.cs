using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models
{
    public interface IRepository<TEntity, TID> 
        where TEntity : class 
        where TID : IComparable<TID>
    {
        IQueryable<TEntity> All { get; }
        TEntity Find(TID id);
        bool InsertOrUpdate(TEntity entity);
        void Delete(TID id);
        void SaveChanges();
    }

    public interface IRepositoryLongID<T> : IRepository<T, long> where T : class
    {
 
    }

    public interface IRepositoryGuidID<T> : IRepository<T, Guid> where T : class
    {

    }
}