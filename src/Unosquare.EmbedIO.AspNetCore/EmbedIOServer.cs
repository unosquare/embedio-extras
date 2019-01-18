using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.EmbedIO.AspNetCore.Wrappers;
using Unosquare.Labs.EmbedIO;

namespace Unosquare.EmbedIO.AspNetCore
{
    internal class EmbedIOServer : IServer, IDisposable
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
            webServer = new WebServer();
            webServer.RegisterModule(aspNetModule);

            webServer.UrlPrefixes.Remove("http://*/");
            foreach (string address in serverAddresses.Addresses)
                webServer.UrlPrefixes.Add(address + "/");

            // Start listener
            webServer.RunAsync();

            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            webServer.Dispose();

            return Task.FromResult(0);
        }
    }
}
