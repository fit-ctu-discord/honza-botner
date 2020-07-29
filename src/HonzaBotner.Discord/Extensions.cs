using System;
using Microsoft.Extensions.Logging;

namespace OsBot.Core
{
    internal static class Extensions
    {
        public static LogLevel ToLoggingLevel(this DSharpPlus.LogLevel level)
        {
            return level switch
            {
                DSharpPlus.LogLevel.Debug => LogLevel.Debug,
                DSharpPlus.LogLevel.Info => LogLevel.Information,
                DSharpPlus.LogLevel.Warning => LogLevel.Warning,
                DSharpPlus.LogLevel.Error => LogLevel.Error,
                DSharpPlus.LogLevel.Critical => LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }
    }
}