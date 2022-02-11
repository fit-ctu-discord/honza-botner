using System;

namespace HonzaBotner.Services.Contract.Dto;

public record News(string Link, string Author, string Title, string Content, DateTime CreatedAt);
