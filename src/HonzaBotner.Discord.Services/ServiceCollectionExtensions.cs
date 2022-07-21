using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Discord.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CommonCommandOptions>(configuration.GetSection(CommonCommandOptions.ConfigName));
        services.Configure<CustomVoiceOptions>(configuration.GetSection(CustomVoiceOptions.ConfigName));
        services.Configure<PinOptions>(configuration.GetSection(PinOptions.ConfigName));
        services.Configure<InfoOptions>(configuration.GetSection(InfoOptions.ConfigName));
        services.Configure<ReminderOptions>(configuration.GetSection(ReminderOptions.ConfigName));
        services.Configure<ButtonOptions>(configuration.GetSection(ButtonOptions.ConfigName));
        services.Configure<BadgeRoleOptions>(configuration.GetSection(BadgeRoleOptions.ConfigName));

        return services;
    }
}
