using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yo.Net.Spelling.Drivers
{
    public class HunSpell : ISpellingDriver
    {
        /// <summary>
        /// 
        /// </summary>
        public string DriverName
        {
            get
            {
                return "HunSpell";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextItems"></param>
        /// <param name="Lang"></param>
        /// <returns></returns>
        public string[] GetMisspelledWords(string CheckText, SupportedLanguages Lang)
        {
            List<string> result = new List<string>();
            SpellingProxy spellingproxy = SpellingProxy.SpellingProxyInstance;
            result.AddRange(spellingproxy.GetMisspelledWords(CheckText, Lang));

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Word"></param>
        /// <param name="Lang"></param>
        /// <returns></returns>
        public string[] GetSuggestions(string Word, SupportedLanguages Lang)
        {
            List<string> result = new List<string>();
            SpellingProxy spellingproxy = SpellingProxy.SpellingProxyInstance;
            result = spellingproxy.GetSuggestion(Word, Lang);
            return result.ToArray();
        }
    }
}
