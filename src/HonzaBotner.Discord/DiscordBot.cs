using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord
{
    internal class DiscordBot : IDiscordBot
    {
        private readonly DiscordWrapper _discordWrapper;
        private readonly EventHandler.EventHandler _eventHandler;
        private readonly CommandConfigurator _configurator;
        private readonly SlashCommandsConfigurator _slashConfigurator;
        private readonly IVoiceManager _voiceManager;
        private readonly DiscordConfig _discordOptions;

        private DiscordClient Client => _discordWrapper.Client;
        private CommandsNextExtension Commands => _discordWrapper.Commands;
        private SlashCommandsExtension SlashCommands => _discordWrapper.SlashCommands;

        public DiscordBot(DiscordWrapper discordWrapper, EventHandler.EventHandler eventHandler,
            CommandConfigurator configurator, SlashCommandsConfigurator slashConfigurator, IVoiceManager voiceManager, 
            IOptions<DiscordConfig> discordOptions)
        {
            _discordWrapper = discordWrapper;
            _eventHandler = eventHandler;
            _configurator = configurator;
            _slashConfigurator = slashConfigurator;
            _voiceManager = voiceManager;
            _discordOptions = discordOptions.Value;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;
            Client.GuildDownloadCompleted += Client_GuildDownloadCompleted;

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            Client.MessageReactionAdded += Client_MessageReactionAdded;
            Client.MessageReactionRemoved += Client_MessageReactionRemoved;
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;
            Client.ChannelCreated += Client_ChannelCreated;

            _configurator.Config(Commands);
            Commands.RegisterConverter(new EnumConverter<ActivityType>());
            _slashConfigurator.Config(SlashCommands);

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
            sender.Logger.LogInformation("Guild available: {GuildName}", e.Guild.Name);
            return Task.CompletedTask;
        }

        private async Task Client_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            sender.Logger.LogInformation("Guild download completed");

            // Run managers' init processes.
            await _voiceManager.DeleteAllUnusedVoiceChannelsAsync();
        }

        private async Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(e.Exception, "Exception occured");

            if (_discordOptions.GuildId == null)
                return;

            DiscordGuild guild = await sender.GetGuildAsync(_discordOptions.GuildId.Value);

            await ReportException(guild, "Client error", e.Exception);
            e.Handled = true;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(
                "{Username} successfully executed '{CommandName}'", e.Context.User.Username, e.Command.QualifiedName);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            switch (e.Exception)
            {
                case CommandNotFoundException:
                    {
                        await e.Context.RespondAsync("Tento příkaz neznám.");
                        CommandContext? fakeContext = Commands.CreateFakeContext(e.Context.Member, e.Context.Channel,
                            "help", e.Context.Prefix,
                            Commands.FindCommand("help", out string args), args
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
                            Color = DiscordColor.Red // red
                        };
                        await e.Context.RespondAsync("", embed.Build());
                        break;
                    }
                default:
                    await e.Context.RespondAsync("Něco se pokazilo. Hups. :scream_cat:");
                    await ReportException(e.Context.Guild, $"Command {e.Command.QualifiedName}", e.Exception);
                    e.Context.Client.Logger.LogError(e.Exception,
                        "{Username} tried executing '{CommandName}' but it errored: {ExceptionType}: {ExceptionMessage}",
                        e.Context.User.Username,
                        e.Command?.QualifiedName ?? "<unknown command>", e.Exception.GetType(),
                        e.Exception.Message);
                    break;
            }

            e.Handled = true;
        }


        private Task Client_ComponentInteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            return _eventHandler.Handle(args);
        }

        private Task Client_MessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
        {
            return _eventHandler.Handle(args);
        }

        private Task Client_MessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs args)
        {
            return _eventHandler.Handle(args);
        }

        private Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
        {
            return _eventHandler.Handle(args);
        }

        private Task Client_GuildMemberUpdated(DiscordClient client, GuildMemberUpdateEventArgs args)
        {
            return _eventHandler.Handle(args);
        }

        private Task Client_ChannelCreated(DiscordClient client, ChannelCreateEventArgs args)
        {
            return _eventHandler.Handle(args);
        }

        private async Task ReportException(DiscordGuild guild, string source, Exception exception)
        {
            ulong logChannelId = _discordOptions.LogChannelId;

            if (logChannelId == default)
            {
                return;
            }

            DiscordChannel channel = guild.GetChannel(logChannelId);

            await channel.ReportException(source, exception);
        }
    }
}
