using System;

namespace HonzaBotner.Services.Contract
{
    public class DiscordRole
    {
        public DiscordRole(ulong roleId)
        {
            RoleId = roleId;
        }

        public ulong RoleId { get; }

        public override bool Equals(object? obj) => obj is DiscordRole role && RoleId == role.RoleId;
        public override int GetHashCode() => HashCode.Combine(RoleId);
    }
}
