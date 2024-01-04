using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        private readonly string _queueName = "reservation-events";

        public EventPublisher(
            QueueServiceClient queueServiceClient,
            IJsonSerializer jsonSerializer)
        {
            _queueServiceClient = queueServiceClient;
            _jsonSerializer = jsonSerializer;
        }

        private async Task<QueueClient> GetQueueClientAsync(CancellationToken cancellationToken)
        {
            if (_queueClient == null)
            {
                _queueClient = await _queueServiceClient.CreateQueueAsync(_queueName, null, cancellationToken);
                await _queueClient.CreateIfNotExistsAsync(null, cancellationToken);
            }
            return _queueClient;
        }

        public async Task PublishAsync<T>(T message,
            CancellationToken cancellationToken)
        {
            var queueClient = await GetQueueClientAsync(cancellationToken);
            var json = _jsonSerializer.Serialize(message);
            await queueClient.SendMessageAsync(json, cancellationToken);
        }
    }
}