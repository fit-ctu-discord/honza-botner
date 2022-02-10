using System;
using System.Threading;
using System.Threading.Tasks;
using HonzaBotner.Scheduler.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Scheduler;

internal class ScopedSchedulerJobProvider<TJob> : ICronJob where TJob : class, IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScopedSchedulerJobProvider<TJob>> _logger;
    public string Name { get; } = typeof(TJob).Name;
    public string CronExpression { get; }

    public ScopedSchedulerJobProvider(string cronExpression, IServiceProvider serviceProvider,
        ILogger<ScopedSchedulerJobProvider<TJob>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        CronExpression = cronExpression;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating scope for {JobName}", Name);
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

        IJob cronJob = scope.ServiceProvider.GetRequiredService<TJob>();

        await cronJob.ExecuteAsync(cancellationToken);
    }
}
