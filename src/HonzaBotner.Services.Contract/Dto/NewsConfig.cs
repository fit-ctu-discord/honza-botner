using System;

namespace HonzaBotner.Services.Contract.Dto
{
    public class NewsConfig
    {
        public NewsConfig(int Id, string Name, string Source, DateTime LastFetched, string NewsProviderType,
            string PublisherType, bool Active = false, params ulong[] Channels)
        {
            this.Id = Id;
            this.Name = Name;
            this.Source = Source;
            this.LastFetched = LastFetched;
            this.NewsProviderType = NewsProviderType;
            this.PublisherType = PublisherType;
            this.Active = Active;
            this.Channels = Channels;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime LastFetched { get; set; }
        public string NewsProviderType { get; set; }
        public string PublisherType { get; set; }
        public bool Active { get; set; }
        public ulong[] Channels { get; set; }

        public static string TypeFromPublisher(string publisher)
        {
            return publisher switch
            {
                "CoursesNews" =>
                    "HonzaBotner.Discord.Services.Publisher.DiscordEmbedPublisher, HonzaBotner.Discord.Services",
                "ProjectsTopic" => "",
                "TwitterTweet" => "",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static string TypeFromProvider(string provider)
        {
            return provider switch
            {
                "CoursesNews" => "HonzaBotner.Services.CoursesNewsService, HonzaBotner.Services",
                "ProjectsTopic" => "",
                "TwitterTweet" => "",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
