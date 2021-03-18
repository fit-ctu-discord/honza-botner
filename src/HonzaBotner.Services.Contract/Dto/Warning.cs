using System;

namespace HonzaBotner.Services.Contract.Dto
{
    public record Warning(int Id, ulong UserId, string Reason, DateTime IssuedAt, ulong IssuerId);
}
