// This code is base on Katana.
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. 

namespace Unosquare.Labs.EmbedIO.OwinMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Unosquare.Labs.EmbedIO.OwinMiddleware.Collections;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// EmbedIO Middleware to use with OWIN
    /// </summary>
    public class MiddlewareOwin : Middleware
    {
        private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

        private static readonly FieldInfo CookedPathField = typeof(HttpListenerRequest).GetField("m_CookedUrlPath",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo CookedQueryField = typeof(HttpListenerRequest).GetField("m_CookedUrlQuery",
            BindingFlags.NonPublic | BindingFlags.Instance);

        protected AppFunc OwinApp;
        protected IDictionary<string, object> Environment;
        
        /// <summary>
        /// Creates a new Middleware instance
        /// </summary>
        /// <param name="app"></param>
        /// <param name="properties"></param>
        public MiddlewareOwin(AppFunc app, IDictionary<string, object> properties)
        {
            OwinApp = app;
            Environment = properties;
        }

        /// <summary>
        /// Invokes Middleware
        /// </summary>
        /// <param name="context"></param>
        public override async Task Invoke(MiddlewareContext context)
        {
            try
            {
                // Setup Request Env vars
                Environment["owin.RequestProtocol"] = GetProtocol(context.HttpContext.Request.ProtocolVersion);
                Environment["owin.RequestScheme"] = context.HttpContext.Request.IsSecureConnection
                    ? Uri.UriSchemeHttps
                    : Uri.UriSchemeHttp;
                Environment["owin.RequestMethod"] = context.HttpContext.Request.HttpMethod;
                Environment["owin.RequestHeaders"] = new RequestHeadersDictionary(context.HttpContext.Request);

                string basePath, path, query;
                GetPathAndQuery(context.HttpContext.Request, out basePath, out path, out query);

                Environment["owin.RequestPathBase"] = basePath;
                Environment["owin.RequestPath"] = path;
                Environment["owin.RequestQueryString"] = query;

                // Setup Response Env vars
                Environment["owin.ResponseStatusCode"] = (int) HttpStatusCode.OK;
                Environment["owin.ResponseHeaders"] = new ResponseHeadersDictionary(context.HttpContext.Response);
                Environment["owin.ResponseBody"] = context.HttpContext.Response.OutputStream;

                context.WebServer.Log.DebugFormat("OWIN Path {0}", path);
                
                await OwinApp(Environment);

                SetStatusCode(context.HttpContext.Response);
                context.WebServer.Log.DebugFormat("OWIN Status Code {0}", context.HttpContext.Response.StatusCode);
                
                // TODO: I need to know if previous middleware completed the request
                context.WebServer.ProcessRequest(context.HttpContext);
                
                context.Handled = true;
            }
            catch (Exception ex)
            {
                context.WebServer.Log.Error(ex);
            }
        }

        /// <summary>
        /// Sets status code
        /// </summary>
        /// <param name="response"></param>
        private void SetStatusCode(HttpListenerResponse response)
        {
            var statusCode = (int) Environment["owin.ResponseStatusCode"];
            // Default / not present
            if (statusCode != 0)
            {
                if (statusCode == 100 || statusCode < 100 || statusCode >= 1000)
                {
                    throw new ArgumentOutOfRangeException("owin.ResponseStatusCode", statusCode, string.Empty);
                }

                response.StatusCode = statusCode;
            }
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

            if (IsMono)
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
