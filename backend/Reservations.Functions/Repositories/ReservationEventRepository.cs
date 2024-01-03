using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Reservations.Functions.Utils;

namespace Reservations.Functions.Repositories
{
    public interface IReservationEventRepository
    {
        Task AddAsync(string connectionId, string invocationId, string message, CancellationToken cancellationToken);
    }

    public class ReservationEventRepository : IReservationEventRepository
    {
        private readonly TableServiceClient _tableServiceClient;
        private TableClient _tableClient;
        private readonly string _tableName = "ReservationEvent";

        public ReservationEventRepository(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        private async Task<TableClient> GetTableClientAsync(CancellationToken cancellationToken)
        {
            if (_tableClient == null)
            {
                await _tableServiceClient.CreateTableIfNotExistsAsync(_tableName, cancellationToken);
                _tableClient = _tableServiceClient.GetTableClient(_tableName);
            }

            return _tableClient;
        }

        public async Task AddAsync(string connectionId, string invocationId, string message,
            CancellationToken cancellationToken)
        {
            var tableClient = await GetTableClientAsync(cancellationToken);
            var tableEntity = new TableEntity($"{connectionId}_{invocationId}",
                DateTimeProvider.Current.UtcNow.ToString("o", CultureInfo.InvariantCulture))
            {
                { "Type", "type" },
                { "Message", message },
                { "Acknowledged", false }
            };
            await tableClient.AddEntityAsync(tableEntity, cancellationToken);
        }
    }
}