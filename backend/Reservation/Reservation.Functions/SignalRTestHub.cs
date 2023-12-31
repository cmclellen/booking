using System.Globalization;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Reservation.Functions
{
    public class SignalRTestHub
    {
        [Function("negotiate")]
        public HttpResponseData Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "serverless")]
            string connectionInfo)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(connectionInfo);
            return response;
        }

        [Function("broadcast")]
        [SignalROutput(HubName = "serverless")]
        public static async Task<SignalRMessageAction> Broadcast([TimerTrigger("*/15 * * * * *")] TimerInfo timerInfo)
        {
            return new SignalRMessageAction("FlightBookedEvent",
                new object[] { $"The time is {DateTime.Now.ToString(CultureInfo.InvariantCulture)}" });
        }
    }
}