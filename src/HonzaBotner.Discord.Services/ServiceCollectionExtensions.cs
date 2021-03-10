using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandOptions(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.Configure<CommonCommandOptions>(
                configuration.GetSection(CommonCommandOptions.ConfigName));
            serviceCollection.Configure<CustomVoiceOptions>(configuration.GetSection(CustomVoiceOptions.ConfigName));
            serviceCollection.Configure<PinOptions>(configuration.GetSection(PinOptions.ConfigName));
            serviceCollection.Configure<InfoOptions>(configuration.GetSection(InfoOptions.ConfigName));

            return serviceCollection;
        }
    }
}
