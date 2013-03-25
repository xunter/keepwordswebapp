namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddFileEntityAndPronounceAudioFileToTheWordEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Files",
                c => new
                    {
                        ID = c.Guid(nullable: false, identity: true),
                        Size = c.Long(nullable: false),
                        MimeType = c.String(),
                        FileName = c.String(),
                        Extension = c.String(),
                        OriginalUri = c.String(),
                        BinaryData = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("Words", "PronounceAudioFileID", c => c.Guid());
            AddForeignKey("Words", "PronounceAudioFileID", "Files", "ID");
            CreateIndex("Words", "PronounceAudioFileID");
        }
        
        public override void Down()
        {
            DropIndex("Words", new[] { "PronounceAudioFileID" });
            DropForeignKey("Words", "PronounceAudioFileID", "Files");
            DropColumn("Words", "PronounceAudioFileID");
            DropTable("Files");
        }
    }
}
