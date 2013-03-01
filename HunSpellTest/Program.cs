using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yo.Net.Spelling;
namespace HunSpellTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SpellingProxy proxy = SpellingProxy.SpellingProxyInstance;

            Console.WriteLine("Write a sentence to be checked.");

            string sentence = Console.ReadLine();
            List<string> misspelledWords = proxy.GetMisspelledWords(sentence, SupportedLanguages.en);

            if (misspelledWords.Count > 0)
            {
                Console.WriteLine(String.Join(",", misspelledWords));
                
                foreach (string word in misspelledWords)
                {
                    List<string> suggestionList = proxy.GetSuggestion(word, SupportedLanguages.en);
                    string suggestions = String.Join(",", suggestionList);
                    Console.WriteLine(word + " suggestions: " + suggestions);
                }
            }
            else
            {
                Console.WriteLine("There are no misspelled words.");
            }
            
            Console.ReadLine();
        }
    }
}
