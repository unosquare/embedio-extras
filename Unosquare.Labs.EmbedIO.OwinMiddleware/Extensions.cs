using Owin;
using System.Collections.Generic;
using System.Reflection;
using Unosquare.Labs.EmbedIO.Modules;

namespace Unosquare.Labs.EmbedIO.OwinMiddleware
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
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
        public static IAppBuilder UseEmbedCors(this IAppBuilder app, string origins = Constants.CorsWildcard,
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
        /// Use EmbedIO WebAPI
        /// </summary>
        /// <param name="app"></param>
        /// <param name="assembly"></param>
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
    }
}