using System;
using System.Web.Http;
using Customer.Interfaces;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;

namespace Tickets.Controllers
{
    [ServiceRequestActionFilter]
    public class TicketsController : ApiController
    {
        private readonly Uri _customerActorUri;

        public TicketsController()
        {
            _customerActorUri = new Uri("fabric:/TicketReservation/CustomerActorService");
        }

        public async Task Post([FromBody]Guid? customerId)
        {
            if(!customerId.HasValue)
            {
                customerId = Guid.NewGuid();
            }

            var customer = ActorProxy.Create<ICustomer>(new ActorId(customerId.Value), _customerActorUri);

            await customer.ReserveTicket();
        }
    }
}
