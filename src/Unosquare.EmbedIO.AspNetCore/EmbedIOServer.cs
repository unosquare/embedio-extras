using EmbedIO;

namespace Unosquare.EmbedIO.AspNetCore
{
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging;
    using Swan;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Wrappers;

    internal class EmbedIOServer : IServer
    {
        public IFeatureCollection Features { get; } = new Microsoft.AspNetCore.Http.Features.FeatureCollection();

        private IWebServer _webServer;
        private AspNetModule _aspNetModule;

        private readonly ServerAddressesFeature serverAddresses = new ServerAddressesFeature();

        public EmbedIOServer(ILoggerFactory loggerFactory)
        {
            Features.Set<IServerAddressesFeature>(serverAddresses);
        }

        public void Dispose()
        {
            _webServer.Dispose();
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            // Setup handler module
            _aspNetModule = new AspNetModule(new HttpApplicationWrapper<TContext>(application), Features);

            // Setup web server
            _webServer = new WebServer(serverAddresses.Addresses.Select(x => x + "/").ToArray());
            _webServer.Modules.Add(_aspNetModule);

            // Start listener
            _webServer.RunAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _webServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
