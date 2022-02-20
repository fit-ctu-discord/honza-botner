using System;

namespace HonzaBotner.Scheduler.Contract;

[AttributeUsage(AttributeTargets.Class)]
public class CronAttribute : Attribute
{
    public string Expression { get; }

    public CronAttribute(string expression)
    {
        Expression = expression;
    }
}
