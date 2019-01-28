namespace Unosquare.EmbedIO.AspNetCore
{
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Wrappers;
    using Unosquare.Labs.EmbedIO;

    internal class EmbedIOServer : IServer
    {
        public IFeatureCollection Features { get; } = new Microsoft.AspNetCore.Http.Features.FeatureCollection();

        private WebServer webServer;
        private AspNetModule aspNetModule;

        private ServerAddressesFeature serverAddresses = new ServerAddressesFeature();

        public EmbedIOServer(ILoggerFactory loggerFactory)
        {
            Features.Set<IServerAddressesFeature>(serverAddresses);
        }

        public void Dispose()
        {
            webServer.Dispose();
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            // Setup handler module
            aspNetModule = new AspNetModule(new HttpApplicationWrapper<TContext>(application), Features);

            // Setup web server
            webServer = new WebServer(serverAddresses.Addresses.Select(x => x + "/").ToArray());
            webServer.RegisterModule(aspNetModule);

            // Start listener
            webServer.RunAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            webServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
