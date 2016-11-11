using System;
using System.Runtime.Serialization;

namespace Customer.Interfaces
{
    [DataContract]
    public class CustomerReservation
    {
        public CustomerReservation(Guid customerId, Guid ticketId)
        {
            CustomerId = customerId;
            TicketId = ticketId;
        }

        [DataMember]
        public Guid CustomerId { get; private set; }

        [DataMember]
        public Guid TicketId { get; private set; }
    }
}
