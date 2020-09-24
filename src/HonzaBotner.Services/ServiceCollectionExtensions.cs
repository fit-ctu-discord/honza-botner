using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HonzaBotner.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesOptions(this IServiceCollection serviceCollection,
                   IConfiguration configuration)
        {
            serviceCollection.Configure<DiscordRoleConfig>(configuration.GetSection(DiscordRoleConfig.ConfigName));
            return serviceCollection;
        }

        public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<I()

            return serviceCollection;
        }
    }
}
