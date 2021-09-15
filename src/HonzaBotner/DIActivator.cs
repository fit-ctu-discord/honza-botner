using System;
using Hangfire;

namespace HonzaBotner
{
    internal class DIActivator : JobActivator
    {
        private readonly IServiceProvider _container;

        public DIActivator(IServiceProvider container)
        {
            _container = container;
        }

        public override object ActivateJob(Type jobType)
        {
            return _container.GetService(jobType) ?? throw new InvalidOperationException($"Job {jobType.FullName} was not found in DI container.");
        }
    }
}
