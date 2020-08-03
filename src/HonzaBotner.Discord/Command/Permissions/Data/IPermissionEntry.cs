using System;

namespace HonzaBotner.Discord.Command.Permissions.Data
{
    public interface IPermissionEntry
    {
        public Guid Id { get; }
        public PermissionEntryType Type { get; }
        public PermissionEntryTarget Target { get; }
        public ulong TargetId { get; }
        public string Permission { get; }
    }
}
