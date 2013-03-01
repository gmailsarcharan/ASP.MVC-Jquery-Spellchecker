using NHunspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

﻿/*
 * SpellingProxy a simple spellcheck proxy that can be used in conjunction 
 * with a multitude of applications.  This proxy uses a local version of 
 * NHunspell http://www.maierhofer.de/en/open-source/nhunspell-net-spell-checker.aspx spellchecker 
 * as the Open Office english dictionary files found here http://extensions.services.openoffice.org/dictionary
 * 
 * Copyright (c) 2012, 2013 Anthony Terra
 * MIT license  : http://www.opensource.org/licenses/mit-license.php
 */
namespace Yo.Net.Spelling
{
   /// <summary>
   /// Simple implementation of th NHunspell Libraries
   /// </summary>
    public class SpellingProxy
    {
        #region constants
        //list of marks that will be trimmed from each checked word
        private const string IGNORED_MARKS = "~`!@#$%^&*()_+|}{[]\\:\";'<>?,./";
        #endregion

        #region variables
        //create a lazy loaded static spell check factor
        private static readonly Lazy<SpellingProxy> _factory = new Lazy<SpellingProxy>(() =>new SpellingProxy());
        //create all appropriate dictionaries
        private static readonly Lazy<Hunspell> _englishDictionary = new Lazy<Hunspell>(
            () => new Hunspell(Common_Spell_Resources.en_USaff, Common_Spell_Resources.en_USDic), true);

        private static readonly object _lockObject = new object();
        #endregion

        #region constructor
        private SpellingProxy()
        {
            
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the singleton instance of the current spelling factory
        /// </summary>
        public static SpellingProxy SpellingProxyInstance
        {
            get 
            {
                //ensure thread safety
                lock (_lockObject)
                {
                    return _factory.Value;
                }
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a list of misspelled words based on the passed input
        /// </summary>
        /// <param name="Text">
        /// The Text to be checked for misspelled words
        /// </param>
        /// <param name="Language">
        /// The language in which the check will be performed
        /// </param>
        /// <returns>
        /// A list of misspelled words
        /// </returns>
        public List<string> GetMisspelledWords(string Text, SupportedLanguages Language)
        {
            string[] words = Text.Split(' ');
            Hunspell spellEngine = this.getDictionary(Language);
            List<string> misspelledWords = new List<string>();
            foreach (string word in words)
            {
                string trimmedword = word.Trim(IGNORED_MARKS.ToCharArray());
                if (!spellEngine.Spell(trimmedword)) misspelledWords.Add(trimmedword);

            }
            return misspelledWords;

        }

        /// <summary>
        /// Returns a list of suggestions for a given misspelled word
        /// </summary>
        /// <param name="Word">
        /// The misspelled word
        /// </param>
        /// <param name="Language">
        /// The Language to retrieve the suggestions in
        /// </param>
        /// <returns></returns>
        public List<string> GetSuggestion(string Word, SupportedLanguages Language)
        {
            Hunspell spellEngine = this.getDictionary(Language);
            return spellEngine.Suggest(Word);
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// Returns a dictionary from the factory
        /// </summary>
        /// <param name="Language">
        /// One of the supported Langauges.  Currently only english is supported, but this can be expanded 
        /// to support and of the Open Office Languages
        /// </param>
        /// <returns>
        /// This singleton instance of the dictionary
        /// </returns>
        private Hunspell getDictionary(SupportedLanguages Language)
        {
           //ensure thread safety
            lock (_lockObject)
            {
                switch (Language)
                {
                    case SupportedLanguages.en:
                        return _englishDictionary.Value;
                    default:
                        throw new NotSupportedException("This Language is not supported.");
                }
            }
        }
        #endregion

    }
    
    
}
