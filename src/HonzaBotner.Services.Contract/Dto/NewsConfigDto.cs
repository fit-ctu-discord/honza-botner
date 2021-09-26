using System;

namespace HonzaBotner.Services.Contract
{
    public record NewsConfigDto(int Id, string Name, string Source, DateTime LastFetched, string NewsProviderType, string PublisherType, params ulong[] Channels)
    {
    }
}
