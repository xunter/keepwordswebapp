namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddedWordIDPropertyToTranslationEntity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Translations", "Word_ID", "Words");
            DropIndex("Translations", new[] { "Word_ID" });
            RenameColumn(table: "Translations", name: "Word_ID", newName: "WordID");
            AddForeignKey("Translations", "WordID", "Words", "ID", cascadeDelete: true);
            CreateIndex("Translations", "WordID");
        }
        
        public override void Down()
        {
            DropIndex("Translations", new[] { "WordID" });
            DropForeignKey("Translations", "WordID", "Words");
            RenameColumn(table: "Translations", name: "WordID", newName: "Word_ID");
            CreateIndex("Translations", "Word_ID");
            AddForeignKey("Translations", "Word_ID", "Words", "ID");
        }
    }
}
