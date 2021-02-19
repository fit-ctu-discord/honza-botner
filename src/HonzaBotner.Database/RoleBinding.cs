namespace HonzaBotner.Database
{
    public class RoleBinding
    {
        public ulong RoleId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }

        public string Emoji { get; set; } = null!;
    }
}
