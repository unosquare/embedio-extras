using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace EmbedIO.AspNetCore.Wrappers
{
    public class HttpApplicationWrapper<TContext> : IHttpApplication<object>
    {
        private readonly IHttpApplication<TContext> application;

        public HttpApplicationWrapper(IHttpApplication<TContext> application)
        {
            this.application = application;
        }

        public object CreateContext(IFeatureCollection contextFeatures)
        {
            return application.CreateContext(contextFeatures);
        }
        public void DisposeContext(object context, Exception exception)
        {
            application.DisposeContext((TContext)context, exception);
        }
        public Task ProcessRequestAsync(object context)
        {
            return application.ProcessRequestAsync((TContext)context);
        }
    }
}
