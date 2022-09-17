using System;
using System.Collections;
using System.Collections.Generic;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Xunit;

namespace HonzaBotner.Services.Test;

public class NewsConfigEnumsTest
{
    public static IEnumerable<object[]> GetNewsProviderEnums()
    {
        foreach (NewsProviderType providerType in Enum.GetValues<NewsProviderType>())
        {
            yield return new object[] { providerType };
        }
    }

    [Theory]
    [MemberData(nameof(GetNewsProviderEnums))]
    public void NewsProviderToTypeTest(NewsProviderType newsProviderType)
    {
        Type interfaceType = typeof(INewsService);
        string type = newsProviderType.ToType();

        Type? t = Type.GetType(type);

        Assert.NotNull(t);
        Assert.True(t!.IsAssignableTo(interfaceType));
    }

    public static IEnumerable<object[]> GetPublisherEnums()
    {
        foreach (PublisherType publisherType in Enum.GetValues<PublisherType>())
        {
            yield return new object[] { publisherType };
        }
    }

    [Theory]
    [MemberData(nameof(GetPublisherEnums))]
    public void PublisherToTypeTest(PublisherType publisherType)
    {
        Type interfaceType = typeof(IPublisherService);
        string type = publisherType.ToType();

        Type? t = Type.GetType(type);

        Assert.NotNull(t);
        Assert.True(t!.IsAssignableTo(interfaceType));
    }
}