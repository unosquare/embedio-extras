using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.EmbedIO.OwinMiddleware.Collections;
using Unosquare.Swan;

namespace Unosquare.Labs.EmbedIO.OwinMiddleware
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        private static readonly FieldInfo CookedPathField = typeof(HttpListenerRequest).GetField("m_CookedUrlPath",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo CookedQueryField = typeof(HttpListenerRequest).GetField("m_CookedUrlQuery",
            BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// EmbedIO WebModule Key
        /// </summary>
        public static string WebModulesKey = "embedIO.WebModule";

        /// <summary>
        /// Use EmbedIO CORS implementation
        /// </summary>
        /// <param name="app"></param>
        /// <param name="origins"></param>
        /// <param name="headers"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        public static IAppBuilder UseEmbedIOCors(this IAppBuilder app, string origins = Constants.CorsWildcard,
            string headers = Constants.CorsWildcard,
            string methods = Constants.CorsWildcard)
        {
            if (app.Properties.ContainsKey(WebModulesKey) == false)
            {
                app.Properties.Add(WebModulesKey, new List<IWebModule>());
            }

            if (app.Properties[WebModulesKey] is List<IWebModule>)
            {
                (app.Properties[WebModulesKey] as List<IWebModule>).Add(new CorsModule(origins, headers, methods));
            }

            return app;
        }

        /// <summary>
        /// Uses the EmbedIO Web API.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IAppBuilder UseWebApi(this IAppBuilder app, Assembly assembly)
        {
            if (app.Properties.ContainsKey(WebModulesKey) == false)
            {
                app.Properties.Add(WebModulesKey, new List<IWebModule>());
            }

            if (app.Properties[WebModulesKey] is List<IWebModule>)
            {
                (app.Properties[WebModulesKey] as List<IWebModule>)
                    .Add(new WebApiModule().LoadApiControllers(assembly));
            }

            return app;
        }

        /// <summary>
        /// Uses the EmbedIO web socket.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IAppBuilder UseWebSocket(this IAppBuilder app, Assembly assembly)
        {
            if (app.Properties.ContainsKey(WebModulesKey) == false)
            {
                app.Properties.Add(WebModulesKey, new List<IWebModule>());
            }

            if (app.Properties[WebModulesKey] is List<IWebModule>)
            {
                var webSocketModule = new WebSocketsModule();
                var types = (assembly ?? Assembly.GetExecutingAssembly()).GetTypes();
                var sockerServers =
                    types.Where(x => x.BaseType == typeof (WebSocketsServer)).ToArray();

                if (sockerServers.Any())
                {
                    foreach (var socketServer in sockerServers)
                    {
                        webSocketModule.RegisterWebSocketsServer(socketServer);
                    }

                    (app.Properties[WebModulesKey] as List<IWebModule>).Add(webSocketModule);
                }

            }

            return app;
        }

        /// <summary>
        /// Uses the EmbedIO web socket.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="webSocketType">Type of the web socket.</param>
        /// <returns></returns>
        public static IAppBuilder UseWebSocket(this IAppBuilder app, Type webSocketType)
        {
            if (app.Properties.ContainsKey(WebModulesKey) == false)
            {
                app.Properties.Add(WebModulesKey, new List<IWebModule>());
            }

            if (app.Properties[WebModulesKey] is List<IWebModule>)
            {
                var webSocketModule = new WebSocketsModule();
                webSocketModule.RegisterWebSocketsServer(webSocketType);

                (app.Properties[WebModulesKey] as List<IWebModule>)
                    .Add(webSocketModule);
            }

            return app;
        }

        /// <summary>
        /// Generates an Owin App
        /// </summary>
        /// <param name="webServer"></param>
        /// <param name="owinApp"></param>
        /// <returns></returns>
        public static WebServer UseOwin(this WebServer webServer, Func<IAppBuilder, IAppBuilder> owinApp)
        {
            webServer.RegisterModule(new OwinModule(owinApp));

            return webServer;
        }

        /// <summary>
        /// Fills Context information in Env vars
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDictionary<string, object> UseHttpContext(this  IDictionary<string, object> environment, HttpListenerContext context)
        {
            if (context?.Request == null)
                return environment;

            // Setup Request Env vars
            environment["owin.RequestProtocol"] = GetProtocol(context.Request.ProtocolVersion);
            environment["owin.RequestScheme"] = context.Request.IsSecureConnection
                ? Uri.UriSchemeHttps
                : Uri.UriSchemeHttp;
            environment["owin.RequestMethod"] = context.Request.HttpMethod;
            environment["owin.RequestHeaders"] = new RequestHeadersDictionary(context.Request);

            string basePath, path, query;
            GetPathAndQuery(context.Request, out basePath, out path, out query);

            environment["owin.RequestPathBase"] = basePath;
            environment["owin.RequestPath"] = path;
            environment["owin.RequestQueryString"] = query;

            // Setup Response Env vars
            environment["owin.ResponseStatusCode"] = (int)HttpStatusCode.OK;
            environment["owin.ResponseHeaders"] = new ResponseHeadersDictionary(context.Response);
            environment["owin.ResponseBody"] = context.Response.OutputStream;

            return environment;
        }
        
        /// <summary>
        /// Get Path info
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="query"></param>
        private static void GetPathAndQuery(HttpListenerRequest request, out string pathBase, out string path, out string query)
        {
            string cookedPath;

            if (Runtime.IsUsingMonoRuntime)
            {
                cookedPath = "/" + request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
                query = request.Url.Query;
            }
            else
            {
                cookedPath = (string)CookedPathField.GetValue(request) ?? string.Empty;
                query = (string)CookedQueryField.GetValue(request) ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(query) && query[0] == '?')
            {
                query = query.Substring(1); // Drop the ?
            }

            const string bestMatch = "/";

            // pathBase must be empty or start with a slash and not end with a slash (/pathBase)
            // path must start with a slash (/path)
            // Move the matched '/' from the end of the pathBase to the start of the path.
            pathBase = bestMatch.Substring(0, bestMatch.Length - 1);
            path = cookedPath.Substring(bestMatch.Length - 1);
        }

        /// <summary>
        /// Get Protocol
        /// </summary>
        /// <param name="version">Version object</param>
        /// <returns></returns>
        private static string GetProtocol(Version version)
        {
            if (version.Major == 1)
            {
                if (version.Minor == 1)
                {
                    return "HTTP/1.1";
                }

                if (version.Minor == 0)
                {
                    return "HTTP/1.0";
                }
            }

            return "HTTP/" + version.ToString(2);
        }
    }
}