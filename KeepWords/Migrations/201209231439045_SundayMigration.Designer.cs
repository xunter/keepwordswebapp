// <auto-generated />
namespace KeepWords.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    
    public sealed partial class SundayMigration : IMigrationMetadata
    {
        string IMigrationMetadata.Id
        {
            get { return "201209231439045_SundayMigration"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return "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"; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return "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"; }
        }
    }
}