using System;

namespace HonzaBotner.Discord.Services.Commands.Polls;

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
