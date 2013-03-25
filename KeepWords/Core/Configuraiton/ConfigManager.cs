using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace KeepWords.Core.Configuraiton
{
    public class ConfigManager
    {
        public static KeepWordsConfigSection Current
        {
            get
            {
                return ConfigurationManager.GetSection("KeepWords") as KeepWordsConfigSection;
            }
        }
    }
}