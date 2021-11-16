using System;

namespace HonzaBotner.Scheduler
{
    public class InvalidConfigurationException : Exception
    {
        public Type Type { get; }

        public InvalidConfigurationException(string message, Type type) : base(message)
        {
            Type = type;
        }
    }
}
