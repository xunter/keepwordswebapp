namespace KeepWords.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using KeepWords.Models.DB;
    using System.Security.Cryptography;
    using System.Text;
    using System.Data.SqlClient;
    using PavelNazarov.Common.Security;

    internal sealed class Configuration : DbMigrationsConfiguration<KeepWords.Models.DB.KeepWordsDBContext>
    {
        public Configuration()
        {
            //AutomaticMigrationsEnabled = true;
            //AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(KeepWords.Models.DB.KeepWordsDBContext context)
        {
            /*try
            {
                context.Database.ExecuteSqlCommand("alter table Accounts add constraint UniqueUserName unique (UserName)");
            }
            catch (SqlException)
            { }*/
        }
    }
}
