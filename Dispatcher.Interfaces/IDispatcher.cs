using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace Dispatcher.Interfaces
{
    public interface IDispatcher : IService
    {
        Task Enqueue(Guid customerId, Guid ticketId);
    }
}
