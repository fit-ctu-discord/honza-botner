using System.Collections.Generic;

#nullable disable
namespace HonzaBotner.Services.Contract
{
    public class DiscordRoleConfig
    {
        public const string ConfigName = "DiscordRoles";
        public Dictionary<string, ulong> RoleMapping { get; set; }
    }
}
