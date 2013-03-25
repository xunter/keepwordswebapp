using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace KeepWords.Models.DB
{
    public interface IDbContextProvider
    {
        DbContext DbContextInstance { get; }
    }
}
