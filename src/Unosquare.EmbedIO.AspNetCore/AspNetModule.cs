namespace Unosquare.EmbedIO.AspNetCore
{
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Unosquare.Labs.EmbedIO;
    using Unosquare.Labs.EmbedIO.Constants;

    internal class AspNetModule : WebModuleBase
    {
        public override string Name => "ASP.NET Core module";

        public AspNetModule(IHttpApplication<object> application, IFeatureCollection features)
        {
            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (context, ct) =>
            {
                try
                {
                    var featureContext = new FeatureContext(context);
                    var applicationContext = application.CreateContext(featureContext.Features);

                    application.ProcessRequestAsync(applicationContext).Wait();

                    featureContext.OnStart().Wait();

                    application.DisposeContext(applicationContext, null);

                    featureContext.OnCompleted().Wait();
                }
                catch (Exception e)
                {
                    context.Response.StatusCode = 500;

                    using (var writer = new StreamWriter(context.Response.OutputStream))
                        writer.Write(e.ToString());
                }

                return Task.FromResult(true);
            });
        }
    }
}
