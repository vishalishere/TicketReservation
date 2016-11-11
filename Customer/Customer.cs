using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Customer.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Dispatcher.Interfaces;
using Microsoft.ServiceFabric.Services.Client;

namespace Customer
{
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    internal class Customer : Actor, ICustomer
    {
        private readonly Guid _customerId;
        private readonly Uri _dispatcherUri;

        public Customer(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _customerId = actorId.GetGuidId();
            _dispatcherUri = new Uri("fabric:/TicketReservation/Dispatcher");
        }

        public async Task Completed()
        {
            await OnDeactivateAsync();
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
        }

        async Task ICustomer.ReserveTicket()
        {
            var random = new Random();
            var dispatcher = ServiceProxy.Create<IDispatcher>(_dispatcherUri, new ServicePartitionKey(1)); //random.Next(1, 5)

            await dispatcher.Enqueue(_customerId, Guid.NewGuid());
        }
    }
}
