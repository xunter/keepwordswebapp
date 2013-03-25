using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace KeepWords.Core.Configuraiton.Integrations
{
    public class FacebookLoginConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("AppName", IsRequired = true)]
        public string AppName
        {
            get { return (string)this["AppName"]; }
            set { this["AppName"] = value; }
        }

        [ConfigurationProperty("AppID", IsRequired = true)]
        public string AppID
        {
            get { return (string)this["AppID"]; }
            set { this["AppID"] = value; }
        }

        [ConfigurationProperty("AppSecret", IsRequired = true)]
        public string AppSecret
        {
            get { return (string)this["AppSecret"]; }
            set { this["AppSecret"] = value; }
        }

        [ConfigurationProperty("AppDomain", IsRequired = true)]
        public string AppDomain
        {
            get { return (string)this["AppDomain"]; }
            set { this["AppDomain"] = value; }
        }

        [ConfigurationProperty("RedirectUrl", IsRequired = true)]
        public string RedirectUrl
        {
            get { return (string)this["RedirectUrl"]; }
            set { this["RedirectUrl"] = value; }
        }
    }
}