using System;
using System.IO;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract;

public static class NewsConfigHelper
{
    public static string ToType(this NewsProviderType newsProvider)
    {
        return newsProvider switch
        {
            NewsProviderType.Courses => "HonzaBotner.Services.CoursesNewsService, HonzaBotner.Services",
            _ => throw new ArgumentOutOfRangeException(nameof(newsProvider), newsProvider, null)
        };
    }


    public static string ToType(this PublisherType publisher)
    {
        return publisher switch
        {
            PublisherType.DiscordEmbed =>
                "HonzaBotner.Discord.Services.Publisher.DiscordEmbedPublisher, HonzaBotner.Discord.Services",
            _ => throw new ArgumentOutOfRangeException(nameof(publisher), publisher, null)
        };
    }

    public static T StringToEnum<T>(string value) where T : struct
    {
        if (Enum.TryParse(value, true, out T result))
            return result;

        throw new InvalidDataException("Invalid state of enum data");
    }
}
