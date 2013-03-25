using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KeepWords.Models;
using System.Xml.Serialization;
using System.IO;

namespace KeepWords.Core.Repositories
{
    public interface ILanguageInfosRepository
    {
        IEnumerable<LanguageInfo> All { get; }
    }

    public class LanguageInfosRepository : ILanguageInfosRepository
    {
        public IEnumerable<LanguageInfo> All
        {
            get
            {
                var httpContext = HttpContext.Current;
#if DEBUG
                string path = Path.Combine(@"C:\Users\xunter\Documents", "language_infos.xml");
#else
                string path = httpContext.Server.MapPath("~/App_Data/language_infos.xml");
#endif
                var xmlSer = new XmlSerializer(typeof(List<LanguageInfo>), new XmlRootAttribute("LanguageInfos"));
                using (var fileInputStream = File.OpenRead(path))
                {
                    var infos = (List<LanguageInfo>)xmlSer.Deserialize(fileInputStream);
                    return infos;
                }
            }
        }
    }
}