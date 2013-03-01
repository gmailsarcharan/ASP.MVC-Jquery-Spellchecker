using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yo.Net.Spelling
{
    /// <summary>
    /// Langauges that are currently part of the factory
    /// </summary>
    /// <remarks>
    /// This could be further expanded to allow for multiple languages, or custom
    /// dictionaries (scientific, chemical, legal, etc...)
    /// </remarks>
    public enum SupportedLanguages
    {
        /// <summary>
        /// The Apache Open Office dictionaries
        /// </summary>
        en,
    }
    /// <summary>
    /// That drivers are currently supported
    /// </summary>
    public enum SupportedDrivers
    {
        /// <summary>
        /// Google Spell API web Service
        /// </summary>
        Google,
        /// <summary>
        /// The server side implementation of Hunspell
        /// </summary>
        Hunspell,
    }
    internal class Const
    {
    }
}
