using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using KeepWords.Core.Configuraiton.Integrations;

namespace KeepWords.Core.Configuraiton
{
    public class KeepWordsConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("TranslationService", IsRequired = true)]
        public TranslationServiceConfigElement TranslationService
        {
            get { return (TranslationServiceConfigElement)this["TranslationService"]; }
            set { this["TranslationService"] = value; }
        }
        
        [ConfigurationProperty("Integrations", IsRequired = true)]
        public IntegrationsConfigElement Integrations
        {
            get { return (IntegrationsConfigElement)this["Integrations"]; }
            set { this["Integrations"] = value; }
        }
    }
}