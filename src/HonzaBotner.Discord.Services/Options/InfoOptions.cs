#nullable disable
using System.Collections.Generic;

namespace HonzaBotner.Discord.Services.Options
{
    public class InfoOptions
    {
        public static string ConfigName => "InfoOptions";

        public string RepositoryUrl { get; set; }
        public string IssueTrackerUrl { get; set; }
        public string AvatarUrl { get; set; }
    }
}
