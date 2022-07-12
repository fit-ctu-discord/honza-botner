using System;

namespace HonzaBotner.Discord.Services.SCommands.Polls;

public class PollException : Exception
{
    public PollException ()
    {}

    public PollException (string message)
        : base(message)
    {}

    public PollException (string message, Exception innerException)
        : base (message, innerException)
    {}
}
