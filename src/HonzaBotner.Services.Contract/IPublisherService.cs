using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface IPublisherService
    {
        // TODO: Example of API. Subject to change
        Task Publish(NewsDto news);
    }
}
