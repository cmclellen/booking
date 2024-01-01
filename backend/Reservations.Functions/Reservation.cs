using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace Reservations.Functions
{
    public static class Reservation
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")]
            SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName(nameof(Reservation))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            Console.WriteLine(context.InstanceId);
            var makeReservationRequest = context.GetInput<ReservationRequest>();

            await context.CallActivityAsync(nameof(ReserveFlight), makeReservationRequest.ConnectionId);
            await context.CallActivityAsync(nameof(ReserveCar), makeReservationRequest.ConnectionId);
            await context.CallActivityAsync(nameof(ReserveHotel), makeReservationRequest.ConnectionId);
        }

        [FunctionName(nameof(ReserveCar))]
        public static async Task ReserveCar([ActivityTrigger] string connectionId, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "FlightBookedEvent",
                Arguments = new[] { $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}; Booking car..." }
            });

            await Task.Delay(1000);

            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "FlightBookedEvent",
                Arguments = new[] { $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}; Car booked." }
            });
        }

        [FunctionName(nameof(ReserveHotel))]
        public static async Task ReserveHotel([ActivityTrigger] string connectionId, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "FlightBookedEvent",
                Arguments = new[] { $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}; Booking hotel..." }
            });

            await Task.Delay(1000);

            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "FlightBookedEvent",
                Arguments = new[] { $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}; Hotel booked." }
            });
        }

        [FunctionName(nameof(ReserveFlight))]
        public static async Task ReserveFlight([ActivityTrigger] string connectionId, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "FlightBookedEvent",
                Arguments = new[] { $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}; Booking flight..." }
            });

            await Task.Delay(1000);

            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "FlightBookedEvent",
                Arguments = new[] { $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}; Flight booked." }
            });
        }

        [FunctionName("Reservation_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
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