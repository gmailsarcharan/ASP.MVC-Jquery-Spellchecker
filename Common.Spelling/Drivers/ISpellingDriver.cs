using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yo.Net.Spelling.Drivers
{
    public interface ISpellingDriver
    {
        string DriverName { get; }

        string[] GetMisspelledWords(string CheckText, SupportedLanguages Lang);
        string[] GetSuggestions(string Word, SupportedLanguages Lang);
    }
}
