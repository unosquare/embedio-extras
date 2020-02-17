using EmbedIO.AspNetCore.Wrappers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Swan;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmbedIO.AspNetCore
{
    internal class EmbedIOServer : IServer
    {
        private IWebServer _webServer;
        private AspNetModule _aspNetModule;

        private readonly ServerAddressesFeature _serverAddresses = new ServerAddressesFeature();

        public EmbedIOServer()
        {
            Features.Set<IServerAddressesFeature>(_serverAddresses);
        }
        
        public IFeatureCollection Features { get; } = new Microsoft.AspNetCore.Http.Features.FeatureCollection();

        public void Dispose() => _webServer?.Dispose();

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            // Setup handler module
            _aspNetModule = new AspNetModule(new HttpApplicationWrapper<TContext>(application), Features);

            // Setup web server
            _webServer = new WebServer(_serverAddresses.Addresses.Select(x => $"{x}/").ToArray());
            // TODO: Fix
            //_webServer.Modules.Add(_aspNetModule);

            // Start listener
            _webServer.RunAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _webServer?.Dispose();

            return Task.CompletedTask;
        }
    }
}
