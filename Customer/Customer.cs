using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Customer.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Customer
{
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    internal class Customer : Actor, ICustomer
    {
        private readonly string _state = "tickets";

        public Customer(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            await StateManager.TryAddStateAsync(_state, 0);
        }

        async Task ICustomer.ReserveTicket()
        {
            //var tickets = await StateManager.GetStateAsync<int>(_state);

            //await StateManager.SetStateAsync(_state, tickets + 100);

            return;
        }

        async Task<int> ICustomer.GetTickets()
        {
            //var result = await StateManager.GetStateAsync<int>(_state);

            //if (result != default(int))
            //{
            //    return result;
            //}

            return 0;
        }
    }
}
