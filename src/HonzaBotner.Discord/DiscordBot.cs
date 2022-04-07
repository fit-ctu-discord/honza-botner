using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using HonzaBotner.Discord.Attributes;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord;

internal class DiscordBot : IDiscordBot
{
    private readonly DiscordWrapper _discordWrapper;
    private readonly EventHandler.EventHandler _eventHandler;
    private readonly CommandConfigurator _configurator;
    private readonly SlashCommandsConfigurator _slashConfigurator;
    private readonly IVoiceManager _voiceManager;
    private readonly ISlashManager _slashManager;
    private readonly DiscordConfig _discordOptions;

    private DiscordClient Client => _discordWrapper.Client;
    private CommandsNextExtension Commands => _discordWrapper.Commands;
    private SlashCommandsExtension SCommands => _discordWrapper.SlashCommands;

    public DiscordBot(DiscordWrapper discordWrapper,
        EventHandler.EventHandler eventHandler,
        CommandConfigurator configurator,
        SlashCommandsConfigurator slashConfigurator,
        IVoiceManager voiceManager,
        IOptions<DiscordConfig> discordOptions,
        ISlashManager slashManager)
    {
        _discordWrapper = discordWrapper;
        _eventHandler = eventHandler;
        _configurator = configurator;
        _slashConfigurator = slashConfigurator;
        _voiceManager = voiceManager;
        _slashManager = slashManager;
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

        SCommands.SlashCommandExecuted += SCommands_SlashCommandExecuted;
        SCommands.SlashCommandErrored += SCommands_SlashCommandErrored;
        SCommands.ContextMenuExecuted += SCommands_ContextMenuExecuted;
        SCommands.ContextMenuErrored += SCommands_ContextMenuErrored;
        SCommands.AutocompleteErrored += SCommands_AutocompleteErrored;

        Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
        Client.MessageReactionAdded += Client_MessageReactionAdded;
        Client.MessageReactionRemoved += Client_MessageReactionRemoved;
        Client.VoiceStateUpdated += Client_VoiceStateUpdated;
        Client.GuildMemberUpdated += Client_GuildMemberUpdated;
        Client.ChannelCreated += Client_ChannelCreated;

        _configurator.Config(Commands);
        Commands.RegisterConverter(new EnumConverter<ActivityType>());
        _slashConfigurator.Config(SCommands);

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
        _ = Task.Run(() => _slashManager.UpdateStartupPermissions());
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
            case InvalidOperationException:
            case ArgumentException:
                {
                    await e.Context.RespondAsync("Tento příkaz neznám.");
                    try
                    {
                        CommandContext? fakeContext = Commands.CreateFakeContext(e.Context.Member,
                            e.Context.Channel,
                            "help", e.Context.Prefix,
                            Commands.FindCommand("help", out string args), args
                        );
                        await Commands.ExecuteCommandAsync(fakeContext);
                    }
                    catch (NullReferenceException)
                    {
                        await e.Context.Channel.SendMessageAsync("Pro více info zadej příkaz na discordovém serveru");
                    }

                    break;
                }
            case ChecksFailedException exception:
                {
                    IReadOnlyList<CheckBaseAttribute> failedChecks = exception.FailedChecks;
                    foreach (var failedCheck in failedChecks)
                    {
                        DiscordEmoji permissionEmoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                        DiscordEmoji timerEmoji = DiscordEmoji.FromName(e.Context.Client, ":alarm_clock:");

                        if (failedCheck is CooldownAttribute)
                        {
                            if (exception.Context.Guild is not null &&
                                (exception.Context.Member.Permissions & Permissions.ManageMessages) != 0)
                            {
                                await exception.Command.ExecuteAsync(exception.Context);
                                return;
                            }
                        }

                        DiscordEmbed embed = failedCheck switch
                        {
                            ICustomAttribute attribute => await attribute.BuildFailedCheckDiscordEmbed(),
                            RequireGuildAttribute => new DiscordEmbedBuilder()
                                .WithTitle("Příkaz nelze použít mimo server")
                                .WithDescription($"{permissionEmoji} Příkaz lze použít jen na discord serveru.")
                                .WithColor(DiscordColor.Red)
                                .Build(),
                            CooldownAttribute check => new DiscordEmbedBuilder()
                                .WithTitle("Příkaz používáš příliš často")
                                .WithDescription($"{timerEmoji} Příkaz můžeš opět použít až za " +
                                                 $"{(int)check.GetRemainingCooldown(exception.Context).TotalMinutes} " +
                                                 "minut")
                                .WithColor(DiscordColor.Yellow)
                                .Build(),
                            _ => new DiscordEmbedBuilder()
                                .WithTitle("Přístup zakázán")
                                .WithDescription($"{permissionEmoji} Na vykonání příkazu nemáte dostatečná práva." +
                                                 "Pokud si myslíte že ano, kontaktujte svého MODa.")
                                .WithColor(DiscordColor.Red)
                                .Build()
                        };
                        await e.Context.RespondAsync("", embed);
                        break;
                    }

                    break;
                }
            default:
                await e.Context.RespondAsync("Něco se pokazilo. Hups. :scream_cat:");
                await ReportException(e.Context.Guild, $"Command {e.Command.QualifiedName}", e.Exception);
                e.Context.Client.Logger.LogError(
                    e.Exception,
                    "{Username} tried executing '{CommandName}' but it errored: {ExceptionType}: {ExceptionMessage}",
                    e.Context.User.Username,
                    e.Command?.QualifiedName ?? "<unknown command>", e.Exception.GetType(),
                    e.Exception.Message
                );
                break;
        }

        e.Handled = true;
    }


    private Task SCommands_SlashCommandExecuted(SlashCommandsExtension e, SlashCommandExecutedEventArgs args)
    {
        e.Client.Logger.LogInformation("Executed {Command} by {Author}", args.Context.CommandName, args.Context.Member.DisplayName);
        return Task.CompletedTask;
    }

    private Task SCommands_SlashCommandErrored(SlashCommandsExtension e, SlashCommandErrorEventArgs args)
    {
        e.Client.Logger.LogError(args.Exception, "Exception occured while executing {Command}", args.Context.CommandName);
        return Task.CompletedTask;
    }
    private Task SCommands_ContextMenuExecuted(SlashCommandsExtension e, ContextMenuExecutedEventArgs args){ return Task.CompletedTask; }

    private Task SCommands_ContextMenuErrored(SlashCommandsExtension e, ContextMenuErrorEventArgs args)
    {
        e.Client.Logger.LogError(args.Exception, "Exception occured while executing context menu {ContextMenu}", args.Context.CommandName);
        return Task.CompletedTask;
    }

    private Task SCommands_AutocompleteErrored(SlashCommandsExtension e, AutocompleteErrorEventArgs args)
    {
        e.Client.Logger.LogError(args.Exception, "Autocomplete failed while looking into option {OptionName}", args.Context.FocusedOption.Name);
        return Task.CompletedTask;
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
