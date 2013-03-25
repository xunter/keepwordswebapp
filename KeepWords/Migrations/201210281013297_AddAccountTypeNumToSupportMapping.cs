namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddAccountTypeNumToSupportMapping : DbMigration
    {
        public override void Up()
        {
            AddColumn("Accounts", "TypeNum", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Accounts", "TypeNum");
        }
    }
}
