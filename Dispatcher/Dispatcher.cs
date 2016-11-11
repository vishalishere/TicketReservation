using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Dispatcher.Interfaces;
using MicroServiceFabric.Dispatcher;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Customer.Interfaces;

namespace Dispatcher
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Dispatcher : StatefulService, IDispatcher
    {
        private readonly IReliableDispatcher<CustomerReservation> _reliableDispatcher;
        private readonly QueueClient _queueClient;
        private readonly IList<BrokeredMessage> _brokeredMessages;
        //private static readonly MessagingFactory _messagingFactory;

        public Dispatcher(StatefulServiceContext context)
            : base(context)
        {
            _reliableDispatcher = new ReliableDispatcher<CustomerReservation>(
                new Lazy<IReliableQueue<CustomerReservation>>(
                    () => StateManager.GetOrAddAsync<IReliableQueue<CustomerReservation>>("messages").Result),
                new TransactionFactory(StateManager));

            var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var serviceBusSection = configurationPackage.Settings.Sections["ServiceBus"];
            var connectionString = serviceBusSection.Parameters["ConnectionString"].Value;
            var queueName = serviceBusSection.Parameters["QueueName"].Value;

            _queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName);
            _brokeredMessages = new List<BrokeredMessage>();
        }

        //static Dispatcher()
        //{
            //var configurationPackage = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            //var serviceBusSection = configurationPackage.Settings.Sections["ServiceBus"];
            //var connectionString = serviceBusSection.Parameters["ConnectionString"].Value;

        //    _messagingFactory = MessagingFactory.CreateFromConnectionString("Endpoint=sb://ticketreservation.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=NMO82MrIAt6++akSy7ynLtFz35Q18r7MogK+vcncH0M=;");
        //}

        Task IDispatcher.Enqueue(Guid customerId, Guid ticketId)
        {
            return _reliableDispatcher.EnqueueAsync(new CustomerReservation(customerId, ticketId));
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(this.CreateServiceRemotingListener) };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await _reliableDispatcher.RunAsync(Dispatch, cancellationToken);
        }

        private async Task Dispatch(ITransaction transaction, CustomerReservation message, CancellationToken cancellationtoken)
        {
            cancellationtoken.ThrowIfCancellationRequested();

            var brokeredMessage = new BrokeredMessage(message);
            _brokeredMessages.Add(brokeredMessage);

            if (_brokeredMessages.Count == 10)
            {
                await _queueClient.SendBatchAsync(_brokeredMessages);
                _brokeredMessages.Clear();
            }
        }
    }
}
