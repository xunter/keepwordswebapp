using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Channels;
using KeepWords.Core.Configuraiton;
using System.Web.Configuration;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using KeepWords.Models.DB;
using KeepWords.Models;

namespace KeepWords.Core.Translating
{
    public class BingTranslationService : ITranslationService
    {
        private TranslationServiceConfigElement _config;
        
        protected HttpContext HttpContextInstance
        {
            get { return System.Web.HttpContext.Current; }
        }

        public AdmAccessToken AccessTokenInfo
        {
            get { return HttpContextInstance.Cache["bingTranslationAccessToken"] as AdmAccessToken; }
            protected set { HttpContextInstance.Cache.Add("bingTranslationAccessToken", value, null, DateTime.Now.AddSeconds(value.ExpiresIn), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null); }
        }

        public bool IsAccessTokenValid
        {
            get { return AccessTokenInfo != null; }
        }

        [DataContract]
        public class AdmAccessToken
        {
            [DataMember(Name = "access_token")]
            public string AccessToken { get; set; }

            [DataMember(Name = "token_type")]
            public string TokenType { get; set; }

            [DataMember(Name = "expires_in")]
            public int ExpiresIn { get; set; }

            [DataMember(Name = "scope")]
            public string Scope { get; set; }
        }

        [DataContract]
        public class AdmAuthentication
        {
            [DataMember(Name = "client_id")]
            public string ClientID { get; set; }

            [DataMember(Name = "client_secret")]
            public string ClientSecret { get; set; }

            [DataMember(Name = "scope")]
            public string Scope { get; set; }

            [DataMember(Name = "grant_type")]
            public string GrantType { get; set; }
        }

        protected virtual AdmAccessToken GetAccessToken(AdmAuthentication authData)
        {            //Prepare OAuth request 
            string requestDetails = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(authData.ClientID), HttpUtility.UrlEncode(authData.ClientSecret));

            WebRequest webRequest = WebRequest.Create(_config.AccessUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (var outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }

        }

        public void ObtainAccessToken()
        {
            AdmAccessToken admToken;
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication { ClientID = _config.ClientID, ClientSecret = _config.ClientSecret };
            try
            {
                admToken = GetAccessToken(admAuth);
                AccessTokenInfo = admToken;
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.AccessToken;
            }
            catch (WebException e)
            {
            }
            catch (Exception ex)
            {

            }

        }

        public BingTranslationService()
        {
            var keepWordsConfig = (KeepWordsConfigSection)WebConfigurationManager.GetSection("KeepWords");
            _config = keepWordsConfig.TranslationService;
        }



        public string[] TranslateText(string from, string to, string text)
        {
            try
            {
                var cacheKey = String.Format("bingTranslation_{0}", text);
                var translations = HttpContextInstance.Cache[cacheKey] as string[];
                if (translations == null)
                {
                    UseClient(client =>
                    {
                        var options = new KeepWords.BingTranslatorService.TranslateOptions { };
                        options.Category = "general";
                        options.ContentType = "text/plain";
                        options.User = "TestUserId";
                        options.Uri = "";
                        //Keep appId parameter 
                        var translationsResult = client.GetTranslations(String.Empty, text, from, to, _config.MaxTranslations, options);
                        translations = translationsResult.Translations.OrderBy(t => t.Rating).Select(t => t.TranslatedText).ToArray();
                    });
                    if (translations.Length > 0)
                    {
                        HttpContextInstance.Cache.Add(cacheKey, translations, null, DateTime.Now.AddMonths(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Low, null);
                    }
                }
                return translations;
            }
            catch (Exception ex)
            {
                // log an error
                throw new TranslatorException("A fatal error has occurred while translate a text!", ex);
            }
        }

        protected void UseClient(Action<BingTranslatorService.LanguageService> action)
        {
            if (!IsAccessTokenValid)
            {
                ObtainAccessToken();
            }
            // Add TranslatorService as a service reference, Address:http://api.microsofttranslator.com/V2/Soap.svc
            BingTranslatorService.LanguageServiceClient client = new BingTranslatorService.LanguageServiceClient();
            //Set Authorization header before sending the request
            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Method = "POST";
            httpRequestProperty.Headers.Add("Authorization", String.Format("Bearer {0}", AccessTokenInfo.AccessToken));
            // Creates a block within which an OperationContext object is in scope.
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                action(client);
            } 
        }

        public string DetectLanguage(string text)
        {
            try
            {
                string lang = null;
                UseClient(client =>
                {
                    lang = client.Detect(String.Empty, text);
                });
                return lang;
            }
            catch (Exception ex)
            {
                throw new TranslatorException("An error has occurred while detect a language!", ex);
            }
        }
        
        public string GetPronounceAudioFileUri(string textToSpeak, string lang, string format, string options)
        {
            try
            {
                string pronounceUri = null;
                UseClient(client =>
                {
                    pronounceUri = client.Speak(String.Empty, textToSpeak, lang, format, options);
                });
                return pronounceUri;
            }
            catch (Exception ex)
            {
                throw new TranslatorException("An error has occurred while detect a language!", ex);
            }
        }
        
        public Models.DB.File GetPronounceAudioFile(string textToSpeak, string lang, string format, string options)
        {
            try
            {
                string fileUri = GetPronounceAudioFileUri(textToSpeak, lang, format, options);
                var webRequest = WebRequest.Create(fileUri);
                using (var response = webRequest.GetResponse())
                {
                    var file = new File() { OriginalUri = fileUri, Size = response.ContentLength, MimeType = response.ContentType };
                    //var fileName = System.IO.Path.GetFileName(fileUri);
                    //var extension = System.IO.Path.GetExtension(fileUri);
                    //file.FileName = fileName;
                    //file.Extension = extension;
                    var responseStream = response.GetResponseStream();
                    var buffer = new byte[response.ContentLength];
                    responseStream.Read(buffer, 0, buffer.Length);
                    file.BinaryData = buffer;
                    return file;
                }
            }
            catch (TranslatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TranslatorException("An error has occurred while detect a language!", ex);
            }
        }
    }
}