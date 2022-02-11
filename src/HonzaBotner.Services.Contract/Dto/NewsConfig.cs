using System;

namespace HonzaBotner.Services.Contract.Dto;

public record NewsConfig(int Id, string Name, string Source, DateTime LastFetched, NewsProviderType NewsProvider,
    PublisherType Publisher, bool Active = false, params ulong[] Channels);

public enum NewsProviderType
{
    Courses
}

public enum PublisherType
{
    DiscordEmbed
}
