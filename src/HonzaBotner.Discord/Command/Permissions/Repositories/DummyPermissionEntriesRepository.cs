using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using HonzaBotner.Discord.Command.Permissions.Entities;

namespace HonzaBotner.Discord.Command.Permissions.Repositories
{
    public class DummyPermissionEntriesRepository : IPermissionEntriesRepository
    {
        public Task<List<PermissionEntry>> GetPermissionEntriesByPermissionsAsync(List<string> permissions)
        {
            return Task.FromResult(new List<PermissionEntry>
            {
                new PermissionEntry
                {
                    Id = Guid.NewGuid(),
                    Permission = "tag:brno",
                    Target = PermissionEntryTarget.User,
                    TargetId = 238728915647070209UL,
                    Type = PermissionEntryType.Denial
                },

                new PermissionEntry
                {
                    Id = Guid.NewGuid(),
                    Permission = "something:mods:can:do",
                    Target = PermissionEntryTarget.Role,
                    TargetId = 366970860550225950UL,
                    Type = PermissionEntryType.Grant
                },

                new PermissionEntry
                {
                    Id = Guid.NewGuid(),
                    Permission = "something:authenticated:can:do",
                    Target = PermissionEntryTarget.Role,
                    TargetId = 681559148546359432UL,
                    Type = PermissionEntryType.Grant
                }
            });
        }
    }
}
