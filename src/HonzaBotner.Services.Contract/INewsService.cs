using System;
using System.Collections.Generic;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface INewsService
    {
        IAsyncEnumerable<NewsDto> FetchDataAsync(string source, DateTime since);
    }
}
