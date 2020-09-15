using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord;
using HonzaBotner.Discord.Command;

namespace HonzaBotner.Commands.Messages
{
    public class EditImage : IChatCommand
    {
        public const string ChatCommand = "editImage";
        // ;editImage <message-link> <new-image-url> <new-message>

        public async Task ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return;
            if (message.Content.Split(" ").Length < 3) return;

            DiscordMessage? oldMessage = await
                DiscordHelper.FindMessageFromLink(message.Channel.Guild,
                    message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1]);

            // TODO: message not found.
            if (oldMessage == null)
            {
                return;
            }

            string imageUrl = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[2];

            // Remove command and channel mention from message.
            // TODO: maybe remove command part to utils?
            const string pattern = @"^.\w+\s+[^\s]+\s+[^\s]+\s+";
            string text = message.Content;
            string editMessageText = Regex.Replace(text, pattern, "");

            try
            {
                await oldMessage.ModifyAsync(editMessageText.Trim() == "" ? oldMessage.Content : editMessageText,
                    embed: new DiscordEmbedBuilder {ImageUrl = imageUrl}.Build());
            }
            catch
            {
                //TODO: some error; do you edit your (bot's) message?
            }
        }
    }
}
