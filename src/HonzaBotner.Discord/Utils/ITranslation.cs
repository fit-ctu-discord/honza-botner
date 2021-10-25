using System;

namespace HonzaBotner.Discord.Utils
{
    public interface ITranslation
    {
        /// <summary>
        /// Language options to choose from.
        /// </summary>
        public enum Language
        {
            English,
            Czech
        }

        /// <summary>
        /// Get translation by it's key.
        /// When a language is not set, a default language is used.
        /// </summary>
        ///
        /// <param name="key">Translation name.</param>
        /// <param name="language">Specified translation.</param>
        ///
        /// <returns>Translated string.</returns>
        /// <exception cref="ArgumentException">When the key is not found.</exception>
        /// <exception cref="ArgumentException">When no translation is found for specified/default language.</exception>
        string GetText(string key, Language? language = null);

        /// <inheritdoc cref="GetText"/>
        string this[string text, Language? language = null]
        {
            get { return GetText(text, language); }
        }

        /// <summary>
        /// Sets a language, used as default options to further calls.
        /// </summary>
        /// <param name="language">Language to be set.</param>
        void SetLanguage(Language language);
    }
}
