using System;
using System.ComponentModel.DataAnnotations;

namespace HonzaBotner.Discord.Command.Permissions
{
    public class PermissionEntry
    {
        public Guid Id { get; set; }

        [Required]
        public PermissionEntryType Type { get; set; }

        [Required]
        public PermissionEntryTarget Target { get; set; }

        [Required]
        public ulong TargetId { get; set; }

        [Required]
        public string Permission { get; set; }
    }
}
