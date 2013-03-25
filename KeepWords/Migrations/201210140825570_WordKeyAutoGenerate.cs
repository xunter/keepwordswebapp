namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class WordKeyAutoGenerate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("Words", "ID", c => c.Guid(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("Words", "ID", c => c.Guid(nullable: false));
        }
    }
}
