﻿using System;

namespace HonzaBotner.Services.Contract.Dto
{
    public record NewsConfig(int Id, string Name, string Source, DateTime LastFetched, string NewsProviderType, string PublisherType, bool Active = false, params ulong[] Channels);
}
