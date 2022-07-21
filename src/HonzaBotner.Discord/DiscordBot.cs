using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord;

internal class DiscordBot : IDiscordBot
{
    private readonly DiscordWrapper _discordWrapper;
    private readonly EventHandler.EventHandler _eventHandler;
    private readonly SlashCommandsConfigurator _slashConfigurator;
    private readonly IVoiceManager _voiceManager;
    private readonly DiscordConfig _discordOptions;

    private DiscordClient Client => _discordWrapper.Client;
    private SlashCommandsExtension SCommands => _discordWrapper.SlashCommands;

    public DiscordBot(DiscordWrapper discordWrapper,
        EventHandler.EventHandler eventHandler,
        SlashCommandsConfigurator slashConfigurator,
        IVoiceManager voiceManager,
        IOptions<DiscordConfig> discordOptions)
    {
        _discordWrapper = discordWrapper;
        _eventHandler = eventHandler;
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

    private Task SCommands_SlashCommandExecuted(SlashCommandsExtension e, SlashCommandExecutedEventArgs args)
    {
        e.Client.Logger.LogDebug("Executed {Command} by {Author}", args.Context.CommandName, args.Context.Member.DisplayName);
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
