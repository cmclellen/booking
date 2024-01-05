using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Storage.Queues;
using Reservations.Functions.Serilaization;

namespace Reservations.Functions.Utils
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken);
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly IJsonSerializer _jsonSerializer;
        private QueueClient _queueClient;

        public EventPublisher(
            QueueServiceClient queueServiceClient,
            IJsonSerializer jsonSerializer)
        {
            _queueServiceClient = queueServiceClient;
            _jsonSerializer = jsonSerializer;
        }

        private async Task<QueueClient> GetQueueClientAsync(string messageTypeName, CancellationToken cancellationToken)
        {
            if (_queueClient == null)
            {
                _queueClient = await _queueServiceClient.CreateQueueAsync(GetQueueName(messageTypeName), null, cancellationToken);
                await _queueClient.CreateIfNotExistsAsync(null, cancellationToken);
            }

            return _queueClient;
        }

        // TODO: Make dynamic by convention
        private string GetQueueName(string messageTypeName)
        {
            if (messageTypeName == "ReservationEvent") return "reservation-events";
            if (messageTypeName == "ReservationAckEvent") return "reservation-ack-events";
            throw new Exception($"Unexpected message type name \"{messageTypeName}\".");
        }

        public async Task PublishAsync<T>(T message,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(message, nameof(message));

            var queueClient = await GetQueueClientAsync(message.GetType().Name, cancellationToken);
            var json = _jsonSerializer.Serialize(message);
            await queueClient.SendMessageAsync(json, cancellationToken);
        }
    }
}