using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using KeepWords.Models.DB;

namespace KeepWords.Models.Account
{
    [Serializable]
    public class SignInModel
    {
        [Required]
        [RegularExpression(@"^([A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4})$")]
        [MaxLength(DBConstants.MaxStringFieldLength)]
        public string UserName { get; set; }
        public string PasswordOriginal { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }
}