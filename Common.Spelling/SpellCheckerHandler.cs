using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using Yo.Net.Spelling.Drivers;

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
            //PHP defines an array passed back in the form with "[]"
            string[] text = context.Request.Form.GetValues("text[]");
            string suggest = null;
            //if the action is suggest set the suggestion word
            if(context.Request.Form["action"] == SUGGESTIONS_ACTION){
                suggest = context.Request.Form["word"];
                this._currentSpellCheckMode = SpellCheckMode.Suggest;
            }

            
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
        private string SpellCheck(string[] textItems, string lang, string engine, string suggest)
        {
            string[] result = null;
            StringBuilder stringBuilder = new StringBuilder();

            if (string.Equals(suggest, "undefined", StringComparison.OrdinalIgnoreCase))
            {
                suggest = string.Empty;
            }

            ISpellingDriver driver = DriverFactory.GetDriver(engine);
            SupportedLanguages currentLang = (SupportedLanguages)Enum.Parse(typeof(SupportedLanguages), lang);

            if (!string.IsNullOrWhiteSpace(suggest))
            {
                result = driver.GetSuggestions(suggest, currentLang);
                ConvertToStringArray(result, stringBuilder);
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            else
            {
                result = driver.GetMisspelledWords(String.Join(" ",textItems), currentLang);
                stringBuilder.Append("{\"outcome\":\"success\",\"data\":[");

                if (textItems.Length > 0)
                {
                    foreach (string item in textItems)
                    {
                        List<string> currentList = new List<string>();
                        foreach (string data in result)
                        {
                            if (item.Contains(data)) currentList.Add(data);
                        }
                        ConvertToStringArray(currentList, stringBuilder);
                    }
                }
                else
                {
                    ConvertToStringArray(textItems, stringBuilder);
                }
                stringBuilder.Remove(stringBuilder.Length-1, 1);
                stringBuilder.Append("]}");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts the C# string list to Javascript array string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private void ConvertToStringArray(ICollection<string> list, StringBuilder stringBuilder)
        {
            
            stringBuilder.Append("[");
            BuildArray(list, stringBuilder);
            stringBuilder.Append("],");
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

        
        

        #endregion
    }
   
}