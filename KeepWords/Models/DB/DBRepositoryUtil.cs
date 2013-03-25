using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;

namespace KeepWords.Models.DB
{
    public static class DBRepositoryUtil
    {
        public static void InsertOrUpdateEntityForObjectSet<T, TID>(DbContext dbContext, IDbSet<T> objectSet, T entity, Expression<Func<T, TID>> idAccessor)
            where T : class
            where TID : IComparable<TID>
        {
            var memberAccess = (MemberExpression)idAccessor.Body;
            var property = (PropertyInfo)memberAccess.Member;
            var idValue = (TID)property.GetValue(entity, null);
            if (idValue.CompareTo(default(TID)) == 0)
            {
                objectSet.Add(entity);
            }
            else
            {
                objectSet.Attach(entity);

                if (dbContext.Entry(entity).State == System.Data.EntityState.Detached)
                    objectSet.Attach(entity);
                ((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Modified);
            }
        }
    }
}