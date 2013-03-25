using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace KeepWords.Models
{
    [Serializable]
    [XmlRoot]
    public class LanguageInfo
    {
        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("family")]
        public string Family { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("native_name")]
        public string NativeName { get; set; }

        [XmlAttribute("lang")]
        public string Lang { get; set; }
    }
}