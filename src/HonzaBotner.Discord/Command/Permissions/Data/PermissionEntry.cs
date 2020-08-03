using System;

namespace HonzaBotner.Discord.Command.Permissions.Data
{
    public sealed class PermissionEntry
    {
        public Guid Id { get; set; }

        public PermissionEntryType Type { get; set; }

        public PermissionEntryTarget Target { get; set; }

        public ulong TargetId { get; set; }

        public string Permission { get; set; } = null!;
    }
}
