namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DictionrayFromToLangCodeAndDisplayNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("Dictionaries", "Name", c => c.String(nullable: false, maxLength: 256));
            AddColumn("Dictionaries", "FromToDisplayName", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("Dictionaries", "FromToDisplayName");
            DropColumn("Dictionaries", "Name");
        }
    }
}
