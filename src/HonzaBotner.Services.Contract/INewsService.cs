using System;
using System.Collections.Generic;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface INewsService
    {
        IAsyncEnumerable<News> FetchDataAsync(string source, DateTime since);
    }
}
