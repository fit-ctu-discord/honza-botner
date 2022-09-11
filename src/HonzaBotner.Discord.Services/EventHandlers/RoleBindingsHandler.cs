using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class RoleBindingsHandler : IEventHandler<MessageReactionAddEventArgs>,
    IEventHandler<MessageReactionRemoveEventArgs>
{
    private readonly IRoleBindingsService _roleBindingsService;

    private readonly RolesOptions _options;

    private readonly IDictionary<ulong, ICollection<ulong>> _pendingStepechRoleAssings = new Dictionary<ulong, ICollection<ulong>>();

    private readonly DiscordEmoji _plusEmoji = DiscordEmoji.FromUnicode("👍");

    private readonly DiscordEmoji _minusEmoji = DiscordEmoji.FromUnicode("👎");

    public RoleBindingsHandler(IRoleBindingsService roleBindingsService, IOptions<RolesOptions> options)
    {
        _roleBindingsService = roleBindingsService;
        _options = options.Value;
    }

    public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
    {
        if (eventArgs.User.IsBot)
            return EventHandlerResult.Continue;

        if (_pendingStepechRoleAssings.ContainsKey(eventArgs.Message.Id)) 
        {
            var plus = await eventArgs.Message.GetReactionsAsync(_plusEmoji);
            var minus = await eventArgs.Message.GetReactionsAsync(_minusEmoji);
            var diff = plus.Count - minus.Count;

            if (Math.Abs(diff) < _options.RequiredVotes) 
            {
                return EventHandlerResult.Continue;
            } 

            var roles = _pendingStepechRoleAssings[eventArgs.Message.Id];
            var stepech = await eventArgs.Guild.GetMemberAsync(_options.Stepech);

            // Changes were approved by seznamka Chads
            if (diff > 0) 
            {
                Task.WaitAll(
                    roles
                        .Select(role => eventArgs.Guild.GetRole(role))
                        .Select(role => stepech.GrantRoleAsync(role))
                        .ToArray()
                );
            }

            _pendingStepechRoleAssings.Remove(eventArgs.Message.Id);
            await eventArgs.Message.DeleteAsync();
        }

        ICollection<ulong> mappings =
            await _roleBindingsService.FindMappingAsync(eventArgs.Channel.Id, eventArgs.Message.Id,
                eventArgs.Emoji.Name);
        if (!mappings.Any())
            return EventHandlerResult.Continue;

        if (eventArgs.User.Id == _options.Stepech) 
        {
            await CreateSeznamkaPoll(mappings, true, eventArgs.Guild);
            return EventHandlerResult.Stop;
        }

        var member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);

        await Task.Run(async () =>
        {
            foreach (ulong roleId in mappings)
            {
                DiscordRole? role = eventArgs.Guild.GetRole(roleId);
                if (role == null)
                    continue;

                await member.GrantRoleAsync(role, "Add role from binding");
            }
        });

        return EventHandlerResult.Stop;
    }

    public async Task<EventHandlerResult> Handle(MessageReactionRemoveEventArgs eventArgs)
    {
        if (eventArgs.User.IsBot)
            return EventHandlerResult.Continue;

        ICollection<ulong> mappings =
            await _roleBindingsService.FindMappingAsync(eventArgs.Channel.Id, eventArgs.Message.Id,
                eventArgs.Emoji.Name);
        if (!mappings.Any())
            return EventHandlerResult.Continue;

        DiscordMember member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);

        await Task.Run(async () =>
        {
            foreach (ulong roleId in mappings)
            {
                DiscordRole? role = eventArgs.Guild.GetRole(roleId);
                if (role == null)
                    continue;

                await member.RevokeRoleAsync(role, "Remove role because of binding");
            }
        });

        return EventHandlerResult.Stop;
    }

    private async Task CreateSeznamkaPoll(ICollection<ulong> roles, bool adding, DiscordGuild guild)
    {
        var seznamka = guild.GetChannel(_options.Seznamka);
        var action = adding ? "přidal" : "odebral";
        var listing = string.Join('\n', roles.Select(role => " - <@&" + role + ">"));

        var embed = new DiscordEmbedBuilder() 
        {
            Title = "The council will decide your fate",
            ImageUrl = "https://i.imgur.com/kvvQtxZ.png",
            Description = $"<@{_options.Stepech}> by si rád {action} následující role:\n" + listing
        };

        var message =  await seznamka.SendMessageAsync(embed: embed.Build());

        await message.CreateReactionAsync(_plusEmoji);
        await message.CreateReactionAsync(_minusEmoji);

        _pendingStepechRoleAssings[message.Id] = roles;
    }
}
