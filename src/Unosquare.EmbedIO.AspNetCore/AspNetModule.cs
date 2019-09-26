using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO.Routing;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace EmbedIO.AspNetCore
{
    internal class AspNetModule : IWebModule
    {
        public AspNetModule(IHttpApplication<object> application, IFeatureCollection features) 
        {
            Application = application;
            Features = features;
        }

        public IFeatureCollection Features { get; set; }

        public IHttpApplication<object> Application { get; set; }
        
        public void Start(CancellationToken cancellationToken)
        {
            // do nothing
        }

        public RouteMatch MatchUrlPath(string urlPath)
        {
            throw new NotImplementedException();
        }

        public async Task HandleRequestAsync(IHttpContext context)
        {
            try
            {
                var featureContext = new FeatureContext(context);
                var applicationContext = Application.CreateContext(featureContext.Features);

                Application.ProcessRequestAsync(applicationContext).Wait();

                await featureContext.OnStart();

                Application.DisposeContext(applicationContext, null);

                await featureContext.OnCompleted();
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;

                using var writer = new StreamWriter(context.Response.OutputStream);
                await writer.WriteAsync(e.ToString());
            }
        }

        public string BaseRoute { get; set; }

        public bool IsFinalHandler { get; } = true;
        public ExceptionHandlerCallback OnUnhandledException { get; set; }
        public HttpExceptionHandlerCallback OnHttpException { get; set; }
    }
}
