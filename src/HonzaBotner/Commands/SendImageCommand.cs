using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

namespace HonzaBotner.Commands
{
    public class SendImageCommand : IChatCommand
    {
        public const string ChatCommand = "sendImage";
        // ;sendImage #general <image_url> <message>

        public async Task ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return;
            if (message.MentionedChannels.Count.Equals(0)) return;
            if (message.Content.Split(" ").Length < 4) return;

            var channel = message.MentionedChannels[0];
            string channelMention = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];

            // First argument isn't a channel mention.
            if ($"<#{channel.Id}>" != channelMention)
            {
                await client.SendMessageAsync(channel, $"wrong format"); // TODO
                return;
            }

            string imageUrl = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[2];
            Console.WriteLine($"Image url is {imageUrl}");

            // Remove command and channel mention from message.
            // TODO: maybe remove command part to utils?
            string pattern = @"^.\w+\s+<#\w+>\s+[^\s]*\s+";
            string text = message.Content;
            string sendMessage = Regex.Replace(text, pattern, "");
            await client.SendMessageAsync(channel, sendMessage,
                embed: new DiscordEmbedBuilder {ImageUrl = imageUrl}.Build());
        }
    }
}
