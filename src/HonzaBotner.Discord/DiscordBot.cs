using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord
{
    internal class DiscordBot : IDiscordBot
    {
        private readonly DiscordWrapper _discordWrapper;
        private readonly ReactionHandler _reactionHandler;
        private readonly CommandConfigurator _configurator;
        private readonly IVoiceManager _voiceManager;

        private DiscordClient Client => _discordWrapper.Client;
        private CommandsNextExtension Commands => _discordWrapper.Commands;

        public DiscordBot(DiscordWrapper discordWrapper, ReactionHandler reactionHandler,
            CommandConfigurator configurator, IVoiceManager voiceManager)
        {
            _discordWrapper = discordWrapper;
            _reactionHandler = reactionHandler;
            _configurator = configurator;
            _voiceManager = voiceManager;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;
            Client.MessageReactionAdded += Client_MessageReactionAdded;
            Client.MessageReactionRemoved += Client_MessageReactionRemoved;
            Client.GuildDownloadCompleted += Client_GuildDownloadCompleted;

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            _configurator.Config(Commands);

            await Client.ConnectAsync();
            await Task.Delay(-1, cancellationToken);
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation("Client is ready to process events");
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation("Guild available: {0}", e.Guild.Name);
            return Task.CompletedTask;
        }

        private async Task Client_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            sender.Logger.LogInformation("Guild download completed");

            // Run managers.
            await _voiceManager.Init();
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(e.Exception, "Exception occured");
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(
                "{0} successfully executed '{1}'", e.Context.User.Username, e.Command.QualifiedName);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(e.Exception,
                $"{0} tried executing '{1}' but it errored: {2}: {3}", e.Context.User.Username,
                e.Command?.QualifiedName ?? "<unknown command>", e.Exception.GetType(),
                e.Exception.Message);

            switch (e.Exception)
            {
                case CommandNotFoundException:
                {
                    await e.Context.RespondAsync("Tento příkaz neznám.");
                    CommandContext? fakeContext = Commands.CreateFakeContext(e.Context.Member, e.Context.Channel,
                        $"help", e.Context.Prefix,
                        Commands.FindCommand($"help", out string args), args
                    );
                    await Commands.ExecuteCommandAsync(fakeContext);
                    break;
                }
                case InvalidOperationException:
                case ArgumentException:
                {
                    await e.Context.RespondAsync("Příkaz jsi zadal špatně.");
                    CommandContext? fakeContext = Commands.CreateFakeContext(e.Context.Member, e.Context.Channel,
                        $"help {e.Command?.QualifiedName}", e.Context.Prefix,
                        Commands.FindCommand($"help {e.Command?.QualifiedName}", out string args), args
                    );
                    await Commands.ExecuteCommandAsync(fakeContext);
                    break;
                }
                case ChecksFailedException:
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                    DiscordEmbedBuilder embed = new()
                    {
                        Title = "Přístup zakázán",
                        Description =
                            $"{emoji} Na vykonání příkazu nemáte dostatečná práva. Pokud si myslíte že ano, kontaktujte svého MODa.",
                        Color = new DiscordColor(0xFF0000) // red
                    };
                    await e.Context.RespondAsync("", embed: embed);
                    break;
                }
                default:
                    e.Context.Client.Logger.LogError(e.Exception, "Unhandled CommandsNext exception");
                    await e.Context.RespondAsync("Něco se pokazilo. Hups. :scream_cat:");
                    break;
            }
        }

        private Task Client_MessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
        {
            return _reactionHandler.HandleAddAsync(args);
        }

        private Task Client_MessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs args)
        {
            return _reactionHandler.HandleRemoveAsync(args);
        }
    }
}
