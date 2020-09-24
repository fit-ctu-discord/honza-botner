using System.Collections.Generic;

namespace HonzaBotner.Services.Contract
{
    public class DiscordRoleConfig
    {
        public const string ConfigName = "DiscordRole";
        public Dictionary<string, ulong> RoleMapping { get; set; }
    }
}
