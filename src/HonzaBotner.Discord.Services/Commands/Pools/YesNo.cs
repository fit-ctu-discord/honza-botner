using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands.Pools
{
    public class YesNo : BaseCommand
    {
        public const string ChatCommand = "yesno";
        // ;yesno <question>

        public YesNo(IPermissionHandler permissionHandler, ILogger<YesNo> logger)
            : base(permissionHandler, logger)
        {
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken = default)
        {
            if (message.Author.IsBot) return ChatCommendExecutedResult.CannotBeUsedByBot;
            if (message.Content.Split(" ").Length < 2) return ChatCommendExecutedResult.WrongSyntax;

            const string pattern = @"^.\w+\s+";
            string text = message.Content;
            string poolMessageContent = Regex.Replace(text, pattern, "");

            string authorNickName = message.Channel.Guild.GetMemberAsync(message.Author.Id).Result.Nickname;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = authorNickName, IconUrl = message.Author.AvatarUrl
                },
                Title = poolMessageContent,
            };

            var poolMessage = await client.SendMessageAsync(message.Channel, embed: embed.Build());
            await message.DeleteAsync();
            await poolMessage.CreateReactionAsync(DiscordEmoji.FromName(client, ":thumbsup:"));
            await poolMessage.CreateReactionAsync(DiscordEmoji.FromName(client, ":thumbsdown:"));

            return ChatCommendExecutedResult.Ok;
        }
    }
}
