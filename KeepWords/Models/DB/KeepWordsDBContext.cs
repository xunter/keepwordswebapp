using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using KeepWords.Migrations;
using System.Globalization;

namespace KeepWords.Models.DB
{
    public interface IIDAvailable<TID>
    {
        TID ID { get; }
    }

    public static class DBConstants
    {
        public const int MaxStringFieldLength = 256;
    }

    public class Account : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [Required]
        [RegularExpression(@"^([A-Za-z0-9._@%-]{3,255})$")]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string DisplayName { get; set; }

        [Required]
        [MaxLength(64)]
        public byte[] Hash { get; set; }

        [Required]
        [MaxLength(64)]
        public byte[] Salt { get; set; }

        public int TypeNum { get; set; }

        [NotMapped]
        public AccountType Type
        {
            get { return (AccountType)TypeNum; }
            set { TypeNum = (int)value; }
        }

        public bool IsBuiltIn
        {
            get { return Type == AccountType.BuiltIn; }
        }

        public DateTime InsertedOn { get; set; }
        public bool IsEnabled { get; set; }

        [InverseProperty("Account")]
        public virtual ICollection<Dictionary> Dictionaries { get; set; }

        [ForeignKey("ActiveDictionary")]
        public Guid? ActiveDictionaryID { get; set; }

        public virtual Dictionary ActiveDictionary { get; set; }

        [ForeignKey("ProfileID")]
        public virtual Profile ProfileInstance { get; set; }

        public Guid? ProfileID { get; set; }
    }

    public class Profile : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        public string ExternalID { get; set; }
        public string PictureUri { get; set; }
        public Image Picture { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string NativeLanguage { get; set; }

        public string FullName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthday { get; set; }

        public string VerifyHash { get; set; }
        [InverseProperty("Profile")]
        public ICollection<ProfileAdditionalField> AdditionalFields { get; set; }
    }

    public class ProfileAdditionalField
    {
        [Key]
        [Column(Order=1)]
        public Guid ProfileID { get; set; }

        [Required]
        [Column(Order = 2)]
        [Key]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        [ForeignKey("ProfileID")]
        public Profile Profile { get; set; } 
    }

    public class Dictionary : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string Name { get; set; }

        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string FromToDisplayName { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string From { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string To { get; set; }

        public virtual ICollection<Word> Words { get; set; }
        
        public virtual Account Account { get; set; }

        [ForeignKey("Account")]
        public Guid AccountID { get; set; }

        public string Code 
        {
            get
            {
                return String.Format("{0} - {1}", From, To);
            }
        }

        public string DisplayName
        {
            get
            {
                if (String.IsNullOrEmpty(Name) || Name == FromToDisplayName)
                {
                    return FromToDisplayName;
                }
                else
                {
                    return String.Format("{0} ({1})", Name, FromToDisplayName);
                }
            }
        }

        public Word[] SearchWords(string searchText, bool isBackTranslation = false)
        {
            if (isBackTranslation)
            {
                var cultureForTo = CultureInfo.GetCultureInfo(this.To);
                return this.Words.SelectMany(w => w.Translations).Where(t => String.Compare(t.Value, searchText, true, cultureForTo) == 0).Select(t => t.Word).ToArray();
            }
            else
            {
                return this.Words.Where(w => w.Original.ToLower() == searchText.ToLower()).ToArray();
            }
        }
    }

    public class Word : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [ForeignKey("DictionaryID")]
        public Dictionary Dictionary { get; set; }

        public Guid DictionaryID { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string Original { get; set; }

        public virtual ICollection<Translation> Translations { get; set; }
                
        public Guid? PronounceAudioFileID { get; set; }

        [ForeignKey("PronounceAudioFileID")]
        public virtual File PronounceAudioFile { get; set; }

        public DateTime InsertedOn { get; set; }

        public string OriginalWithoutExtension
        {
            get { return GetOriginalWithoutArticle(); }
        }

        public string GetOriginalWithoutArticle()
        {
            string original = Original.ToLower();
            if (original.StartsWith("a "))
            {
                return original.Substring(2);
            }
            else if (original.StartsWith("an "))
            {
                return original.Substring(3);
            }
            else if (original.StartsWith("the "))
            {
                return original.Substring(4);
            }
            else if (original.StartsWith("to "))
            {
                return original.Substring(3);
            }
            else
            {
                return original;
            }
        }
    }

    public class Translation : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string Value { get; set; }

        [ForeignKey("WordID")]
        public virtual Word Word { get; set; }

        public Guid WordID { get; set; }

        /*
        public Guid? ImageID { get; set; }

        [ForeignKey("ImageID")]
        public virtual Image AssociatedImage { get; set; }
         */
    }

    public class File : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [Required]
        public long Size { get; set; }
                
        public string MimeType { get; set; }
        
        public string FileName { get; set; }
                
        public string Extension { get; set; }

        public string OriginalUri { get; set; }

        [Required]
        public byte[] BinaryData { get; set; }
    }

    public class Image : IIDAvailable<Guid>
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string Type { get; set; }

        [ForeignKey("FileID")]
        public File FileInstance { get; set; }
        public Guid FileID { get; set; }
    }
    
    public class KeepWordsDBContext : DbContext
    {
        public KeepWordsDBContext()
            : base("KeepWordsDB")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        #region Sets

        public virtual IDbSet<Account> Accounts { get; set; }
        public virtual IDbSet<Dictionary> Dictionaries { get; set; }
        public virtual IDbSet<Word> Words { get; set; }
        public virtual IDbSet<Translation> Translations { get; set; }
        //public virtual IDbSet<Image> Images { get; set; }

        #endregion

        #region Initializer

        internal sealed class KeepWordsDBInstaller : MigrateDatabaseToLatestVersion<KeepWordsDBContext, Configuration>, IDatabaseInitializer<KeepWordsDBContext>
        {
        }

        #endregion
        
    }

    public class KeepWordsDBContextProvider : IDbContextProvider, IDisposable
    {
        private readonly KeepWordsDBContext _dbContext = new KeepWordsDBContext();
        
        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public DbContext DbContextInstance
        {
            get { return _dbContext; }
        }
    }
}