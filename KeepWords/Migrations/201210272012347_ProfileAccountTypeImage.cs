namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ProfileAccountTypeImage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Profiles",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Email = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        MiddleName = c.String(),
                        NativeLanguage = c.String(),
                        FullName = c.String(),
                        Country = c.String(),
                        City = c.String(),
                        Gender = c.String(),
                        Birthday = c.DateTime(),
                        Picture_ID = c.Guid(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("Images", t => t.Picture_ID)
                .Index(t => t.Picture_ID);
            
            CreateTable(
                "Images",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Width = c.Int(),
                        Height = c.Int(),
                        FileName = c.String(nullable: false, maxLength: 256),
                        Type = c.String(nullable: false, maxLength: 256),
                        FileID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("Files", t => t.FileID, cascadeDelete: true)
                .Index(t => t.FileID);
            
            CreateTable(
                "ProfileAdditionalFields",
                c => new
                    {
                        ProfileID = c.Guid(nullable: false),
                        Key = c.String(nullable: false, maxLength: 128),
                        Value = c.String(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProfileID, t.Key })
                .ForeignKey("Profiles", t => t.ProfileID, cascadeDelete: true)
                .Index(t => t.ProfileID);
            
            AddColumn("Accounts", "ProfileID", c => c.Guid());
            AddForeignKey("Accounts", "ProfileID", "Profiles", "ID");
            CreateIndex("Accounts", "ProfileID");
        }
        
        public override void Down()
        {
            DropIndex("ProfileAdditionalFields", new[] { "ProfileID" });
            DropIndex("Images", new[] { "FileID" });
            DropIndex("Profiles", new[] { "Picture_ID" });
            DropIndex("Accounts", new[] { "ProfileID" });
            DropForeignKey("ProfileAdditionalFields", "ProfileID", "Profiles");
            DropForeignKey("Images", "FileID", "Files");
            DropForeignKey("Profiles", "Picture_ID", "Images");
            DropForeignKey("Accounts", "ProfileID", "Profiles");
            DropColumn("Accounts", "ProfileID");
            DropTable("ProfileAdditionalFields");
            DropTable("Images");
            DropTable("Profiles");
        }
    }
}
