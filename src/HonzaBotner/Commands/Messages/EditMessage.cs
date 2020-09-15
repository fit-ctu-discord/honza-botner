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
    public class EditMessage : IChatCommand
    {
        public const string ChatCommand = "edit";
        // ;edit <message-link> <new-message>

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

            const string pattern = @"^.\w+\s+[^\s]*\s+";
            string text = message.Content;
            string editMessageText = Regex.Replace(text, pattern, "");

            try
            {
                await oldMessage.ModifyAsync(editMessageText);
            }
            catch
            {
                //TODO: some error; do you edit your (bot's) message?
            }
        }
    }
}
