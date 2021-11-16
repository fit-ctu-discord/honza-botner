using System;
using System.Reflection;
using HonzaBotner.Scheduler.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Scheduler
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCronJob<TJob>(this IServiceCollection services) where TJob : class, ICronJob
        {
            return services.AddSingleton<ICronJob, TJob>();
        }

        public static IServiceCollection AddScopedCronJob<TJob>(this IServiceCollection services, string cronExpression) where TJob : class, IJob
        {
            services.AddScoped<TJob>();

            return services.AddSingleton<ICronJob, ScopedSchedulerJobProvider<TJob>>(
                provider => new ScopedSchedulerJobProvider<TJob>(cronExpression, provider,
                provider.GetRequiredService<ILogger<ScopedSchedulerJobProvider<TJob>>>()));
        }

        public static IServiceCollection AddScopedCronJob<TJob>(this IServiceCollection services) where TJob : class, IJob
        {
            Type jobType = typeof(TJob);

            CronAttribute cronAttribute = jobType.GetCustomAttribute<CronAttribute>()
                                          ?? throw new InvalidConfigurationException("Type doesnt have CronAttribute",
                                              jobType);

            return services.AddScopedCronJob<TJob>(cronAttribute.Expression);
        }

        public static IServiceCollection AddScheduler(this IServiceCollection services, int schedulerDelay)
        {
            return services.AddSingleton<IHostedService, SchedulerHostedService>(provider => new SchedulerHostedService(
                schedulerDelay,
                provider.GetServices<ICronJob>(),
                provider.GetRequiredService<ILogger<SchedulerHostedService>>()
            ));
        }
    }
}
