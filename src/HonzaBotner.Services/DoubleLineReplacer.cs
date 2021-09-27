using System;
using Html2Markdown.Replacement;

namespace HonzaBotner.Services
{
    internal class DoubleLineReplacer : IReplacer
    {
        public string Replace(string html) => html.Replace($"{Environment.NewLine}{Environment.NewLine}", $"{Environment.NewLine}");
    }
}
