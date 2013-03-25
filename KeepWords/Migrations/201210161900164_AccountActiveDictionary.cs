namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AccountActiveDictionary : DbMigration
    {
        public override void Up()
        {
            AddColumn("Accounts", "ActiveDictionaryID", c => c.Guid());
            AddForeignKey("Accounts", "ActiveDictionaryID", "Dictionaries", "ID");
            CreateIndex("Accounts", "ActiveDictionaryID");
        }
        
        public override void Down()
        {
            DropIndex("Accounts", new[] { "ActiveDictionaryID" });
            DropForeignKey("Accounts", "ActiveDictionaryID", "Dictionaries");
            DropColumn("Accounts", "ActiveDictionaryID");
        }
    }
}
