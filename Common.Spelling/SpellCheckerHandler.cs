using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

﻿/*
 * SpellCheckerHandler.ashx - Spell Checker ASP.NET server-side implementation
 * This handler has been modified to support both a local implementation of
 * NHunspell http://www.maierhofer.de/en/open-source/nhunspell-net-spell-checker.aspx spellchecker 
 * as well as the google spell checker.
 * 
 * Copyright (c) 2012, 2013 Richard Willis, Jack Yang, Anthony Terra
 * MIT license  : http://www.opensource.org/licenses/mit-license.php
 * jQuery plugin library written by Richard Willis (willis.rh@gmail.com): https://github.com/badsyntax/jquery-spellchecker
 * Original .NET port done by Jack Yang (jackmyang@gmail.com): https://github.com/jackmyang/jQuery-Spell-Checker-for-ASP.NET
 */

namespace Yo.Net.Spelling
{
    /// <summary>
    /// jQuery spell checker http handler class. Original server-side code was written by Richard Willis in PHP.
    /// This is a version derived from the original design and implemented for ASP.NET platform.
    /// 
    /// It's very easy to use this handler with ASP.NET WebForm or MVC. Simply do the following steps:
    ///     1. Include project jquery.spellchecker assembly in the website as a reference
    ///     2. Include the httphandler node in the system.web node for local dev or IIS 6 or below
    /// <example>
    ///     <![CDATA[
    ///         <system.web>
    ///             <httpHandlers>
    ///                 <add verb="GET,HEAD,POST" type="UI.Common.Handlers.SpellCheckerHandler.ashx" path="SpellCheckerHandler.ashx"/>
    ///             </httpHandlers>
    ///         </system.web>
    ///     ]]>
    /// </example>
    ///     3. If IIS7 is the target web server, also need to include the httphandler node in the system.webServer node
    /// <example>
    ///     <![CDATA[
    ///         <system.webServer>
    ///             <handlers>
    ///                 <add verb="GET,HEAD,POST" name="SpellCheckerHandler" type="UI.Common.Handlers.SpellCheckerHandler.ashx" path="*SpellCheckerHandler.ashx"/>
    ///             </handlers>
    ///         </system.webServer>
    ///     ]]>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Manipulations of XmlNodeList is used for compatibility concern with lower version of .NET framework,
    /// alternatively, they can be simplified using 'LINQ for XML' if .NET 3.5 or higher is available.
    /// </remarks>
    public class SpellCheckerHandler : IHttpHandler
    {
        #region fields
        private const string INCORRECT_WORDS_ACTION = "get_incorrect_words";
        private const string SUGGESTIONS_ACTION = "get_suggestions";
        private const string GOOGLE_FLAG_TEXT_ALREAD_CLIPPED = "textalreadyclipped";
        private const string GOOGLE_FLAG_IGNORE_DUPS = "ignoredups";
        private const string GOOGLE_FLAG_IGNORE_DIGITS = "ignoredigits";
        private const string GOOGLE_FLAG_IGNORE_ALL_CAPS = "ignoreallcaps";
        private const string HUNSPELL_DRIVER = "hunspell";
        private const string GOGGLE_DRIVER = "google";

        /// <summary>
        /// whether the port is 
        /// </summary>
        private enum SpellCheckMode
        {
            IncorrectWords,
            Suggest,

        }

        private SpellCheckMode _currentSpellCheckMode = SpellCheckMode.IncorrectWords;
        #endregion

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

        #region Implementation of IHttpHandler

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. 
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            string engine = context.Request.Form["driver"];
            string lang = context.Request.Form["lang"];
            string text = context.Request.Form["text[]"];
            string suggest = null;
            //if the action is suggest set the suggestion word
            if(context.Request.Form["action"] == SUGGESTIONS_ACTION){
                suggest = context.Request.Form["word"];
                this._currentSpellCheckMode = SpellCheckMode.Suggest;
            }

            SetSwitches(context);
            string result = SpellCheck(text, lang, engine, suggest);
            context.Response.ContentType = "application/js";
            context.Response.Write(result);
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get { return false; }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Spells the check.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="lang">The lang.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="suggest">The suggest.</param>
        /// <returns></returns>
        private string SpellCheck(string text, string lang, string engine, string suggest)
        {
            if (0 == string.Compare(suggest, "undefined", StringComparison.OrdinalIgnoreCase))
            {
                suggest = string.Empty;
            }
            //supports google and hunspell only
            if (0 == string.Compare(engine, GOGGLE_DRIVER, true))
            {
                string xml;
                List<string> result;
                if (string.IsNullOrEmpty(suggest))
                {
                    xml = GetSpellCheckRequest(text, lang);
                    result = GetListOfMisspelledWords(xml, text);
                }
                else
                {
                    xml = GetSpellCheckRequest(suggest, lang);
                    result = GetListOfSuggestWords(xml, suggest);
                }
                return ConvertStringListToJavascriptArrayString(result);
            }
            if (0 == string.Compare(engine, HUNSPELL_DRIVER, true))
            {
                List<string> result;
                SpellingProxy spellingproxy = SpellingProxy.SpellingProxyInstance;
                //todo: Add support for more than english
                if (string.IsNullOrEmpty(suggest))
                {
                    result = spellingproxy.GetMisspelledWords(text, SupportedLanguages.EnglishUs);
                }
                else
                {
                    result = spellingproxy.GetSuggestion(suggest, SupportedLanguages.EnglishUs); ;
                }
                return ConvertStringListToJavascriptArrayString(result);
            }
            else
            {
                throw new NotImplementedException("Only google and hanspell check engine is support at this moment.");
            }

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
        /// Gets the list of suggest words.
        /// </summary>
        /// <param name="xml">The source XML.</param>
        /// <param name="suggest">The word to be suggested.</param>
        /// <returns></returns>
        private static List<string> GetListOfSuggestWords(string xml, string suggest)
        {
            if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(suggest))
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
        private static List<string> GetListOfMisspelledWords(string xml, string text)
        {
            if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(text))
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
                list.Add(text.Substring(offset, length));
            }
            return list;
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
            return string.Format("{0}lang={1}&text={2}", Properties.Settings.Default.GoogleSpellUrl, lang, text);
        }

        /// <summary>
        /// Converts the C# string list to Javascript array string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private string ConvertStringListToJavascriptArrayString(ICollection<string> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            //string result = list.Count > 0 ? "err" : "success";
            switch (this._currentSpellCheckMode)
            {
                case SpellCheckMode.IncorrectWords:
                    stringBuilder.Append("{\"outcome\":\"success\",\"data\":[[");
                    BuildArray(list, stringBuilder);
                    stringBuilder.Append("]]}");
                    break;
                case SpellCheckMode.Suggest:
                    stringBuilder.Append("[");
                    BuildArray(list, stringBuilder);
                    stringBuilder.Append("]");
                    break;
            }
            
            return stringBuilder.ToString();
        }
  
        /// <summary>
        /// Builds a generic comma delimited array
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="stringBuilder">The Current String Builder</param>
        private void BuildArray(ICollection<string> list, StringBuilder stringBuilder)
        {
            if (null != list && 0 < list.Count)
            {
                bool showSeperator = false;
                foreach (string word in list)
                {
                    if (showSeperator)
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.AppendFormat("\"{0}\"", word);
                    showSeperator = true;
                }
            }
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

        /// <summary>
        /// Requests the spell check and get the result back.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="lang">The language.</param>
        /// <returns></returns>
        private string GetSpellCheckRequest(string text, string lang)
        {
            string requestUrl = Properties.Settings.Default.GoogleSpellUrl;//ConstructRequestUrl("", lang);
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
                       new XElement("text", text))
                ) { Declaration = new XDeclaration("1.0", null, null) };
            //use the unencoded string writer as google chokes on others
            var wr = new UnencodedStringWriter();
            xdoc.Save(wr);
            string data = wr.GetStringBuilder().ToString();
            return data;
        }

        #endregion
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