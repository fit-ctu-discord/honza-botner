using System;
using System.Linq;

namespace HonzaBotner.Database;

public class NewsConfig
{

    public int Id { get; set; }

    public string Name { get; set; }
    public string Source { get; set; }
    public DateTime LastFetched { get; set; }
    public string ChannelsData { get; set; }

    public ulong[] Channels
    {
        get => ChannelsData.Split(";").Select(ulong.Parse).ToArray();
        set => ChannelsData = string.Join(';', value);
    }

    public string NewsProviderType { get; set; }
    public string PublisherType { get; set; }

    public bool Active { get; set; }
}