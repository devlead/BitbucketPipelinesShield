using BitbucketPipelinesShield;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    services.AddHttpClient<BadgeFunctions>()
        .ConfigureHttpClient(client => client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true }))
    .Build();

await host.RunAsync();
