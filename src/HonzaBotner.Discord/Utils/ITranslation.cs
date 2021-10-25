namespace HonzaBotner.Discord.Utils
{
    public interface ITranslation
    {
        public enum Language
        {
            English,
            Czech
        }

        string GetText(string key, Language? language = null);

        void SetLanguage(Language language);
    }
}
