using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord;
using HonzaBotner.Discord.Command;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Commands.Messages
{
    public class SendMessage : IChatCommand
    {
        public const string ChatCommand = "send";
        // ;send #general <message>

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return ChatCommendExecutedResult.CannotBeUsedByBot;
            if (message.MentionedChannels.Count.Equals(0)) return ChatCommendExecutedResult.WrongSyntax;
            if (message.Content.Split(" ").Length < 3) return ChatCommendExecutedResult.WrongSyntax;

            var channel = message.MentionedChannels[0];
            string channelMention = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];

            // First argument isn't a channel mention.
            if ($"<#{channel.Id}>" != channelMention)
            {
                return ChatCommendExecutedResult.WrongSyntax;
            }

            // Remove command and channel mention from message.
            // TODO: maybe remove command part to utils?
            string pattern = @"^.\w+\s+<#\w+>\s+";
            string text = message.Content;
            string sendMessage = Regex.Replace(text, pattern, "");
            await client.SendMessageAsync(channel, sendMessage);

            return ChatCommendExecutedResult.Ok;
        }
    }
}
