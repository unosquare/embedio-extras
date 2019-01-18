using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;

namespace Unosquare.EmbedIO.AspNetCore
{
    internal class AspNetModule : WebModuleBase
    {
        public override string Name => "ASP.NET Core module";

        public AspNetModule(IHttpApplication<object> application, IFeatureCollection features)
        {
            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (context, ct) =>
            {
                try
                {
                    FeatureContext featureContext = new FeatureContext(context);

                    object applicationContext = application.CreateContext(featureContext.Features);

                    application.ProcessRequestAsync(applicationContext).Wait();

                    featureContext.OnStart().Wait();

                    //context.Dispose();
                    application.DisposeContext(applicationContext, null);

                    featureContext.OnCompleted().Wait();

                }
                catch (Exception e)
                {
                    context.Response.StatusCode = 500;

                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                        writer.Write(e.ToString());
                }

                return Task.FromResult(true);
            });
        }
    }
}
