namespace Unosquare.Labs.EmbedIO.OwinMiddleware
{
    using Microsoft.Owin.Builder;
    using Owin;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Inject Owin Middleware to EmbedIO
    /// </summary>
    public class OwinModule : WebModuleBase
    {
        /// <summary>
        /// Init a Owin App into EmbedIO
        /// </summary>
        /// <param name="appConfig"></param>
        public OwinModule(Func<IAppBuilder, IAppBuilder> appConfig)
        {
            IAppBuilder builder = new AppBuilder();
            var app = appConfig(builder).Build();
            var options = new Dictionary<string, object>();
            
            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (server, context) =>
            {
                var environment = options.UseHttpContext(context);
                app(environment).Wait();

                return true;
            });
        }

        /// <summary>
        /// Module's name
        /// </summary>
        public override string Name
        {
            get { return "Owin Module"; }
        }
    }
}
