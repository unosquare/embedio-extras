// This code is base on Katana. Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. 

namespace Unosquare.Labs.EmbedIO.OwinMiddleware
{
    using Swan;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// EmbedIO Middleware to use with OWIN
    /// </summary>
    public class MiddlewareOwin : Middleware
    {
        /// <summary>
        /// Owin App instance
        /// </summary>
        protected AppFunc OwinApp;
        /// <summary>
        /// Environment variables
        /// </summary>
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
                Environment.UseHttpContext(context.HttpContext);

                $"OWIN Path {Environment["owin.RequestPath"]}".Debug(nameof(MiddlewareOwin));

                await OwinApp(Environment);

                SetStatusCode(context.HttpContext.Response);
                $"OWIN Status Code {context.HttpContext.Response.StatusCode}".Debug(nameof(MiddlewareOwin));

                // TODO: I need to know if previous middleware completed the request
                context.WebServer.ProcessRequest(context.HttpContext);

                context.Handled = true;
            }
            catch (Exception ex)
            {
                ex.Log(nameof(MiddlewareOwin));
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
    }
}