using System;

namespace HonzaBotner.Services.Contract.Dto
{
    public record CountedEmoji(ulong Id, ulong Used, DateTime FirstUsedAt)
    {
        public double UsagePerDay => Used / (DateTime.Now.Subtract(FirstUsedAt).TotalDays + 1);
    }
}
