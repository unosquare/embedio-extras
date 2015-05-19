namespace Unosquare.Labs.EmbedIO.OwinMiddleware
{
    using Unosquare.Labs.EmbedIO.Log;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// Server Factory
    /// </summary>
    public class OwinServerFactory
    {
        /// <summary>
        /// Log instance
        /// </summary>
        public static ILog Log { set; get; }

        /// <summary>
        /// Server Factory Name to be used in StartOptions
        /// </summary>
        public static string ServerFactoryName = "Unosquare.Labs.EmbedIO.OwinMiddleware.OwinServerFactory";

        /// <summary>
        /// Initialize a new Factory
        /// </summary>
        /// <param name="properties"></param>
        public static void Initialize(IDictionary<string, object> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
        }

        /// <summary>
        /// Creates a new WebServer
        /// </summary>
        /// <param name="app">OWIN App instance</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IDisposable Create(AppFunc app, IDictionary<string, object> properties)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            var serverUrls = properties["host.Addresses"] as List<IDictionary<string, object>>;

            if (serverUrls == null)
            {
                throw new ArgumentNullException("properties");
            }

            var serverUrl = "http://localhost:" + serverUrls[0]["port"];
            var server = new WebServer(serverUrl, Log);

            var webModules = properties[Extensions.WebModulesKey] as List<IWebModule>;

            if (webModules != null)
            {
                webModules.ForEach(server.RegisterModule);
            }

            var ct = properties["host.OnAppDisposing"] is CancellationToken
                ? (CancellationToken) properties["host.OnAppDisposing"]
                : new CancellationToken();

            server.RunAsync(ct, new MiddlewareOwin(app, properties));

            return server;
        }
    }
}