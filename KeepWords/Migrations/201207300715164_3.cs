namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class _3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Accounts",
                c => new
                    {
                        AccountID = c.Guid(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Hash = c.Binary(nullable: false),
                        Salt = c.Binary(nullable: false),
                        InsertedOn = c.DateTime(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AccountID);
            
            CreateTable(
                "Dictionaries",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        From = c.String(nullable: false, maxLength: 256),
                        To = c.String(nullable: false, maxLength: 256),
                        Account_AccountID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("Accounts", t => t.Account_AccountID, cascadeDelete: true)
                .Index(t => t.Account_AccountID);
            
            CreateTable(
                "Words",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Original = c.String(nullable: false, maxLength: 256),
                        Dictionary_ID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("Dictionaries", t => t.Dictionary_ID, cascadeDelete: true)
                .Index(t => t.Dictionary_ID);
            
            CreateTable(
                "Translations",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Value = c.String(nullable: false, maxLength: 256),
                        Word_ID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("Words", t => t.Word_ID, cascadeDelete: true)
                .Index(t => t.Word_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("Translations", new[] { "Word_ID" });
            DropIndex("Words", new[] { "Dictionary_ID" });
            DropIndex("Dictionaries", new[] { "Account_AccountID" });
            DropForeignKey("Translations", "Word_ID", "Words");
            DropForeignKey("Words", "Dictionary_ID", "Dictionaries");
            DropForeignKey("Dictionaries", "Account_AccountID", "Accounts");
            DropTable("Translations");
            DropTable("Words");
            DropTable("Dictionaries");
            DropTable("Accounts");
        }
    }
}
