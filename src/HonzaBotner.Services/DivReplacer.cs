using System.Text.RegularExpressions;
using Html2Markdown.Replacement;

namespace HonzaBotner.Services
{
    internal class DivReplacer : IReplacer
    {
        private readonly Regex _regex = new ("</?div[^>]*>");

        public string Replace(string html) => _regex.Replace(html, string.Empty);
    }
}
