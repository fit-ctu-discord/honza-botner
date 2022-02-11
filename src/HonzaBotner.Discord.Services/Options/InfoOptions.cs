#nullable disable

namespace HonzaBotner.Discord.Services.Options;

public class InfoOptions
{
    public static string ConfigName => "InfoOptions";

    public string VersionSuffix { get; set; }
    public string RepositoryUrl { get; set; }
    public string IssueTrackerUrl { get; set; }
    public string ChangelogUrl { get; set; }
}
