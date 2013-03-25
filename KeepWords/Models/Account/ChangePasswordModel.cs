using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeepWords.Models.Account
{
    public class ChangePasswordModel
    {
        public Guid AccountID { get; set; }

        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordReply { get; set; }

        public string CurrentPasswordHash { get; set; }
        public string NewPasswordHash { get; set; }
        public string NewPasswordReplyHash { get; set; }

        public int NewPasswordLength { get; set; }
    }
}