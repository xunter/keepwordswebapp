namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ProfileExternalIDAndVerifyHash : DbMigration
    {
        public override void Up()
        {
            AddColumn("Profiles", "ExternalID", c => c.String());
            AddColumn("Profiles", "PictureUri", c => c.String());
            AddColumn("Profiles", "VerifyHash", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Profiles", "VerifyHash");
            DropColumn("Profiles", "PictureUri");
            DropColumn("Profiles", "ExternalID");
        }
    }
}
