using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;

namespace HonzaBotner.Discord.Services.Commands.Messages
{
    public class SendImage : IChatCommand
    {
        public const string ChatCommand = "sendImage";
        // ;sendImage <channel> <image-url> <message>

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return ChatCommendExecutedResult.CannotBeUsedByBot;
            if (message.MentionedChannels.Count.Equals(0)) return ChatCommendExecutedResult.WrongSyntax;
            if (message.Content.Split(" ").Length < 4) return ChatCommendExecutedResult.WrongSyntax;

            var channel = message.MentionedChannels[0];
            string channelMention = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];

            // First argument isn't a channel mention.
            if ($"<#{channel.Id}>" != channelMention)
            {
                return ChatCommendExecutedResult.WrongSyntax;
            }

            string imageUrl = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[2];

            // Remove command and channel mention from message.
            // TODO: maybe remove command part to utils?
            const string pattern = @"^.\w+\s+<#\w+>\s+[^\s]+\s+";
            string text = message.Content;
            string sendMessage = Regex.Replace(text, pattern, "");
            await client.SendMessageAsync(channel, sendMessage,
                embed: new DiscordEmbedBuilder {ImageUrl = imageUrl}.Build());

            return ChatCommendExecutedResult.Ok;
        }
    }
}
