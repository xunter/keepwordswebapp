using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeepWords.Models.DB;

namespace KeepWords.Core.Translating
{
    public interface ITranslationService
    {
        String[] TranslateText(string from, string to, string text);
        String DetectLanguage(string text);
        String GetPronounceAudioFileUri(string textToSpeak, string lang, string format, string options);
        File GetPronounceAudioFile(string textToSpeak, string lang, string format, string options);
    }
}
