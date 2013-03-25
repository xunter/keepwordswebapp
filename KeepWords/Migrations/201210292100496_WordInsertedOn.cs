namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class WordInsertedOn : DbMigration
    {
        public override void Up()
        {
            AddColumn("Words", "InsertedOn", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
        }
        
        public override void Down()
        {
            DropColumn("Words", "InsertedOn");
        }
    }
}
