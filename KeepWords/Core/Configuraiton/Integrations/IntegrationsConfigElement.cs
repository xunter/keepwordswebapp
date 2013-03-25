using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace KeepWords.Core.Configuraiton.Integrations
{
    public class IntegrationsConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("FacebookLogin", IsRequired = true)]
        public FacebookLoginConfigElement FacebookLogin
        {
            get { return (FacebookLoginConfigElement)this["FacebookLogin"]; }
            set { this["FacebookLogin"] = value; }
        }
    }
}