using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Reservations.Functions;
using Azure.Core;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Reservations.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAzureClients(x =>
            {
                x.UseCredential(CreateTokenCredential());
            });
        }

        private static TokenCredential CreateTokenCredential()
        {
            return new ChainedTokenCredential(
#if DEBUG
                new AzureCliCredential(),
#endif
                new ManagedIdentityCredential());
        }
    }
}