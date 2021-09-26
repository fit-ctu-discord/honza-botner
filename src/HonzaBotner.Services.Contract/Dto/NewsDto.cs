using System;

namespace HonzaBotner.Services.Contract.Dto
{
    public record NewsDto(string Link, string Author, string Title, string Content, DateTime CreatedAt);
}
