using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Reservations.Functions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Reservations.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Do nothing
        }
    }
}