using System;

namespace HonzaBotner.Services.Contract.Dto;

public record Reminder(int Id, ulong OwnerId, ulong MessageId, ulong ChannelId, DateTime DateTime, string Content);
