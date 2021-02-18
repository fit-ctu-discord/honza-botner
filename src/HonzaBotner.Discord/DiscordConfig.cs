namespace HonzaBotner.Discord
{
    public class DiscordConfig
    {
        public static string ConfigName => "Discord";

        public string? Token { get; set; }
        public ulong? GuildId { get; set; }
        public string[] Prefixes { get; set; } = null!;
    }
}
