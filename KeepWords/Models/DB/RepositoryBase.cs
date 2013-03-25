using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace KeepWords.Models.DB
{
    public class RepositoryBase<TEntity, TID> : IRepository<TEntity, TID>
        where TEntity : class, IIDAvailable<TID>
        where TID : IComparable<TID>
    {
        private readonly DbContext _dbContext;

        public RepositoryBase(IDbContextProvider dbContextProvider)
        {
            _dbContext = dbContextProvider.DbContextInstance;
        }

        public IQueryable<TEntity> All
        {
            get
            {
                return GetSet().AsQueryable();
            }
        }

        private IDbSet<TEntity> GetSet()
        {
            return _dbContext.Set<TEntity>();
        }

        public TEntity Find(TID id)
        {
            var entity = All.SingleOrDefault(e => e.ID.CompareTo(id) == 0);
            return entity;
        }

        public bool InsertOrUpdate(TEntity entity)
        {
            DBRepositoryUtil.InsertOrUpdateEntityForObjectSet(_dbContext, GetSet(), entity, e => e.ID);
            return true;
        }

        public void Delete(TID id)
        {
            var existingEntity = Find(id);
            if (existingEntity != null)
            {
                GetSet().Remove(existingEntity);
            }
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }
    }
}