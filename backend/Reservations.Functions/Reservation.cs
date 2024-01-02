using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace Reservations.Functions
{
    public class Reservation
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")]
            SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName(nameof(Reservation))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            Console.WriteLine(context.InstanceId);
            var makeReservationRequest = context.GetInput<ReservationRequest>();

            await context.CallActivityAsync(nameof(ReserveFlight), makeReservationRequest.ConnectionId);
            await context.CallActivityAsync(nameof(ReserveCar), makeReservationRequest.ConnectionId);
            await context.CallActivityAsync(nameof(ReserveHotel), makeReservationRequest.ConnectionId);
        }

        [FunctionName(nameof(ReserveCar))]
        public async Task ReserveCar([ActivityTrigger] string connectionId, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            await SendMessageAsync(connectionId, signalRMessages, "Booking car...", cancellationToken);

            await Task.Delay(1000, cancellationToken);

            await SendMessageAsync(connectionId, signalRMessages, "Car booked.", cancellationToken);
        }

        private async Task SendMessageAsync(string connectionId,
            IAsyncCollector<SignalRMessage> signalRMessages,
            string message,
            CancellationToken cancellationToken)
        {
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "ReservationEvent",
                Arguments = new[] { message }
            }, cancellationToken);
        }

        [FunctionName(nameof(ReserveHotel))]
        public async Task ReserveHotel([ActivityTrigger] string connectionId, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            await SendMessageAsync(connectionId, signalRMessages, "Booking hotel...", cancellationToken);

            await Task.Delay(1000, cancellationToken);

            await SendMessageAsync(connectionId, signalRMessages, "Hotel booked.", cancellationToken);
        }

        [FunctionName(nameof(ReserveFlight))]
        public async Task ReserveFlight([ActivityTrigger] string connectionId, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            await SendMessageAsync(connectionId, signalRMessages, "Booking flight...", cancellationToken);

            await Task.Delay(1000, cancellationToken);

            await SendMessageAsync(connectionId, signalRMessages, "Flight booked.", cancellationToken);
        }

        [FunctionName("Reservation_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var makeReservationRequest = await req.Content!.ReadFromJsonAsync<ReservationRequest>();
            var instanceId = await starter.StartNewAsync(nameof(Reservation), makeReservationRequest);
            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}