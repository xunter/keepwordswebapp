using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using KeepWords.Models.DB;

namespace KeepWords.Models.Account
{
    public class SignUpModel
    {
        [Required]
        [RegularExpression(@"^([A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4})$")]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string DisplayName { get; set; }

        public string PasswordOriginal { get; set; }
        public string PasswordOriginalReply { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordHashReply { get; set; }
    }
}