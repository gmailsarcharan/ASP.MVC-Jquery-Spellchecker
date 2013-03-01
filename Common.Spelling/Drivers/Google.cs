using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Yo.Net.Spelling.Drivers
{
    public class Google : ISpellingDriver
    {
        private const string GOOGLE_FLAG_TEXT_ALREAD_CLIPPED = "textalreadyclipped";
        private const string GOOGLE_FLAG_IGNORE_DUPS = "ignoredups";
        private const string GOOGLE_FLAG_IGNORE_DIGITS = "ignoredigits";
        private const string GOOGLE_FLAG_IGNORE_ALL_CAPS = "ignoreallcaps";
        
        #region properties

        /// <summary>
        /// Gets or sets a value indicating whether [ignore duplicated words].
        /// </summary>
        /// <value><c>true</c> if [ignore dups]; otherwise, <c>false</c>.</value>
        private bool IgnoreDups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [ignore digits].
        /// </summary>
        /// <value><c>true</c> if [ignore digits]; otherwise, <c>false</c>.</value>
        private bool IgnoreDigits { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [ignore all capitals].
        /// </summary>
        /// <value><c>true</c> if [ignore all caps]; otherwise, <c>false</c>.</value>
        private bool IgnoreAllCaps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [text alread clipped].
        /// </summary>
        /// <value><c>true</c> if [text alread clipped]; otherwise, <c>false</c>.</value>
        private bool TextAlreadClipped { get; set; }

        #endregion

        public string DriverName
        {
            get
            {
                return "Google";
            }
        }

        public string[] GetMisspelledWords(string CheckText, SupportedLanguages Lang)
        {
            string xml;
            List<string> result;
            SetSwitches(HttpContext.Current);

            xml = GetSpellCheckRequest(CheckText, Enum.GetName(typeof(SupportedLanguages), Lang));
            result = GetListOfMisspelledWords(xml, CheckText);
            
            return result.ToArray();
        }

        public string[] GetSuggestions(string Word, SupportedLanguages Lang)
        {
            string xml;
            List<string> result;
            SetSwitches(HttpContext.Current);

            xml = GetSpellCheckRequest(Word, Enum.GetName(typeof(SupportedLanguages), Lang));
            result = GetListOfSuggestWords(xml);

            return result.ToArray();
        }
        /// <summary>
        /// Requests the spell check and get the result back.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="lang">The language.</param>
        /// <returns></returns>
        private string GetSpellCheckRequest(string text, string lang)
        {
            string requestUrl = ConstructRequestUrl(text, lang);
            string requestContentXml = ConstructSpellRequestContentXml(text);
            byte[] buffer = Encoding.UTF8.GetBytes(requestContentXml);

            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "text/xml");
            try
            {
                byte[] response = webClient.UploadData(requestUrl, "POST", buffer);
                return Encoding.UTF8.GetString(response);
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Constructs the spell request content XML.
        /// </summary>
        /// <param name="text">The text which may contain multiple words.</param>
        /// <returns></returns>
        private string ConstructSpellRequestContentXml(string text)
        {
            XDocument xdoc = new XDocument(
                new XElement("spellrequest",
                    new XAttribute(GOOGLE_FLAG_TEXT_ALREAD_CLIPPED, TextAlreadClipped ? "1" : "0"),
                     new XAttribute(GOOGLE_FLAG_IGNORE_DUPS, IgnoreDups ? "1" : "0"),
                      new XAttribute(GOOGLE_FLAG_IGNORE_DIGITS, IgnoreDigits ? "1" : "0"),
                       new XAttribute(GOOGLE_FLAG_IGNORE_ALL_CAPS, IgnoreAllCaps ? "1" : "0"),
                       new XElement("text", String.Join(" ",text)))
                ) { Declaration = new XDeclaration("1.0", null, null) };
            //use the unencoded string writer as google chokes on others
            var wr = new UnencodedStringWriter();
            xdoc.Save(wr);
            string data = wr.GetStringBuilder().ToString();
            return data;
        }

        /// <summary>
        /// Gets the list of suggest words.
        /// </summary>
        /// <param name="xml">The source XML.</param>
        /// <param name="suggest">The word to be suggested.</param>
        /// <returns></returns>
        private static List<string> GetListOfSuggestWords(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            // 
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xml);
            if (!xdoc.HasChildNodes)
            {
                return null;
            }
            XmlNodeList nodeList = xdoc.SelectNodes("//c");
            if (null == nodeList || 0 >= nodeList.Count)
            {
                return null;
            }
            List<string> list = new List<string>();
            foreach (XmlNode node in nodeList)
            {
                list.AddRange(node.InnerText.Split('\t'));
                return list;
            }
            return list;
        }

        /// <summary>
        /// Gets the list of misspelled words.
        /// </summary>
        /// <param name="xml">The source XML.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static List<string> GetListOfMisspelledWords(string xml, string checkText)
        {
            if (String.IsNullOrEmpty(xml) || String.IsNullOrEmpty(checkText))
            {
                return null;
            }
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xml);
            if (!xdoc.HasChildNodes)
            {
                return null;
            }
            XmlNodeList nodeList = xdoc.SelectNodes("//c");
            if (null == nodeList || 0 >= nodeList.Count)
            {
                return null;
            }
            List<string> list = new List<string>();

            foreach (XmlNode node in nodeList)
            {
                int offset = Convert.ToInt32(node.Attributes["o"].Value);
                int length = Convert.ToInt32(node.Attributes["l"].Value);
                list.Add(checkText.Substring(offset, length));
            }
            return list;
        }
        /// <summary>
        /// Sets the boolean switch.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="queryStringParameter">The query string parameter.</param>
        /// <returns></returns>
        private static bool SetBooleanSwitch(HttpContext context, string queryStringParameter)
        {
            byte tryParseZeroOne;
            string queryStringValue = context.Request.QueryString[queryStringParameter];
            if (!string.IsNullOrEmpty(queryStringValue) && byte.TryParse(queryStringValue, out tryParseZeroOne))
            {
                if (1 < tryParseZeroOne || 0 > tryParseZeroOne)
                {
                    throw new InvalidOperationException(string.Format("Query string parameter '{0}' only supports values of 1 and 0.", queryStringParameter));
                }
                return tryParseZeroOne == 1;
            }
            return false;
        }

        /// <summary>
        /// Constructs the request URL.
        /// </summary>
        /// <param name="text">The text which may contain multiple words.</param>
        /// <param name="lang">The language.</param>
        /// <returns></returns>
        private static string ConstructRequestUrl(string text, string lang)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            lang = string.IsNullOrEmpty(lang) ? "en" : lang;
            return string.Format("{0}?lang={1}&text={2}", Properties.Settings.Default.GoogleSpellUrl, lang, text);
        }

        /// <summary>
        /// Sets the switches.
        /// </summary>
        /// <param name="context">The context.</param>
        private void SetSwitches(HttpContext context)
        {
            IgnoreAllCaps = SetBooleanSwitch(context, GOOGLE_FLAG_IGNORE_ALL_CAPS);
            IgnoreDigits = SetBooleanSwitch(context, GOOGLE_FLAG_IGNORE_DIGITS);
            IgnoreDups = SetBooleanSwitch(context, GOOGLE_FLAG_IGNORE_DUPS);
            TextAlreadClipped = SetBooleanSwitch(context, GOOGLE_FLAG_TEXT_ALREAD_CLIPPED);
        }

    }
    public class UnencodedStringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return null;
            }
        }
    }
}
