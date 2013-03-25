using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace KeepWords.Core.Configuraiton
{
    public class TranslationServiceConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("ClientID", IsRequired = true)]
        public string ClientID 
        {
            get { return (string)this["ClientID"]; }
            set { this["ClientID"] = value; }
        }

        [ConfigurationProperty("ClientSecret", IsRequired = true)]
        public string ClientSecret
        {
            get { return (string)this["ClientSecret"]; }
            set { this["ClientSecret"] = value; }
        }
        
        [ConfigurationProperty("RedirectUri", IsRequired = true)]
        public string RedirectUri
        {
            get { return (string)this["RedirectUri"]; }
            set { this["RedirectUri"] = value; }
        }

        [ConfigurationProperty("AccessUrl", IsRequired = true)]
        public string AccessUrl
        {
            get { return (string)this["AccessUrl"]; }
            set { this["AccessUrl"] = value; }
        }

        [ConfigurationProperty("MaxTranslations", IsRequired = true)]
        public int MaxTranslations
        {
            get { return (int)this["MaxTranslations"]; }
            set { this["MaxTranslations"] = value; }
        }
    }
}