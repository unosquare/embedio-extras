using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Unosquare.EmbedIO.AspNetCore.Wrappers;
using Unosquare.Labs.EmbedIO;
using HeaderDictionary = Unosquare.EmbedIO.AspNetCore.Wrappers.HeaderDictionary;

namespace Unosquare.EmbedIO.AspNetCore
{
    internal class FeatureContext : IHttpRequestFeature, IHttpConnectionFeature, IHttpResponseFeature, IHttpAuthenticationFeature, IHttpRequestIdentifierFeature
    {
        public IHttpContext Context { get; }
        public IFeatureCollection Features { get; }

        private List<Tuple<Func<object, Task>, object>> onStartingActions = new List<Tuple<Func<object, Task>, object>>();
        private List<Tuple<Func<object, Task>, object>> onCompletedActions = new List<Tuple<Func<object, Task>, object>>();


        // Dropped features
        // - ITlsConnectionFeature
        // - IHttpRequestLifetimeFeature
        // - IHttpUpgradeFeature
        // - IHttpWebSocketFeature
        // - IHttpBufferingFeature
        // - IHttpSendFileFeature

        #region IHttpRequestFeature

        private Stream requestBody;
        private string requestPath;
        private IHeaderDictionary requestHeaders;

        Stream IHttpRequestFeature.Body
        {
            get
            {
                return requestBody;
            }
            set
            {
                requestBody = value;
            }
        }
        IHeaderDictionary IHttpRequestFeature.Headers
        {
            get
            {
                if (requestHeaders == null)
                    requestHeaders = new HeaderDictionary(Context.Request.Headers);

                return requestHeaders;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpRequestFeature.Method
        {
            get
            {
                return Context.Request.HttpMethod;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpRequestFeature.Path
        {
            get
            {
                return requestPath;
            }
            set
            {
                requestPath = value;
            }
        }
        string IHttpRequestFeature.PathBase
        {
            get
            {
                return "";
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpRequestFeature.Protocol
        {
            get
            {
                Version protocolVersion = Context.Request.ProtocolVersion;
                return protocolVersion.Major != 1 || protocolVersion.Minor != 1 ? (protocolVersion.Major != 1 || protocolVersion.Minor != 0 ? "HTTP/" + protocolVersion.ToString(2) : "HTTP/1.0") : "HTTP/1.1";
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpRequestFeature.QueryString
        {
            get
            {
                if (Context.Request.QueryString.Count == 0)
                    return "";

                return "?" + string.Join("&", Context.Request.QueryString.Keys.OfType<string>().Select(k => k + "=" + Context.Request.QueryString[k]));
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpRequestFeature.RawTarget
        {
            get
            {
                return Context.Request.RawUrl;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpRequestFeature.Scheme
        {
            get
            {
                return Context.Request.Url.Scheme;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
        #region IHttpConnectionFeature

        IPAddress IHttpConnectionFeature.LocalIpAddress
        {
            get => Context.Request.LocalEndPoint.Address;
            set => throw new NotSupportedException();
        }
        IPAddress IHttpConnectionFeature.RemoteIpAddress
        {
            get => Context.Request.RemoteEndPoint.Address;
            set => throw new NotSupportedException();
        }
        int IHttpConnectionFeature.LocalPort
        {
            get => Context.Request.LocalEndPoint.Port;
            set => throw new NotSupportedException();
        }
        int IHttpConnectionFeature.RemotePort
        {
            get
            {
                return Context.Request.RemoteEndPoint.Port;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        string IHttpConnectionFeature.ConnectionId
        {
            get
            {
                return Context.Request.RequestTraceIdentifier.ToString();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
        #region IHttpResponseFeature

        private Stream responseBody;
        private IHeaderDictionary responseHeaders;

        Stream IHttpResponseFeature.Body
        {
            get
            {
                return responseBody;
            }
            set
            {
                responseBody = value;
            }
        }
        IHeaderDictionary IHttpResponseFeature.Headers
        {
            get
            {
                if (responseHeaders == null)
                    responseHeaders = new ResponseHeaderDictionary(Context.Response);

                return responseHeaders;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        bool IHttpResponseFeature.HasStarted
        {
            get
            {
                return responseStarted;
            }
        }
        string IHttpResponseFeature.ReasonPhrase
        {
            get => Context.Response.StatusDescription;
            set => Context.Response.StatusDescription = value;
        }
        int IHttpResponseFeature.StatusCode
        {
            get
            {
                return Context.Response.StatusCode;
            }
            set
            {
                Context.Response.StatusCode = value;
            }
        }

        void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (responseStarted)
                throw new InvalidOperationException("Cannot register new callbacks, the response has already started.");

            onStartingActions.Add(Tuple.Create(callback, state));
        }
        void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (completed)
                throw new InvalidOperationException("Cannot register new callbacks, the response has already completed.");

            onCompletedActions.Add(Tuple.Create(callback, state));
        }

        #endregion
        #region IHttpAuthenticationFeature

        ClaimsPrincipal IHttpAuthenticationFeature.User
        {
            get
            {
                return new ClaimsPrincipal(Context.User);
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        IAuthenticationHandler IHttpAuthenticationFeature.Handler
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
        #region IHttpRequestIdentifierFeature

        string IHttpRequestIdentifierFeature.TraceIdentifier
        {
            get
            {
                return Context.Request.RequestTraceIdentifier.ToString();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        internal bool responseStarted = false;
        internal bool completed = false;

        internal FeatureContext(IHttpContext context)
        {
            Context = context;
            Features = new FeatureCollection(this);

            requestBody = context.Request.InputStream;
            requestPath = Context.Request.Url.AbsolutePath;

            responseBody = context.Response.OutputStream;
        }

        internal async Task OnStart()
        {
            if (!responseStarted)
            {
                responseStarted = true;

                foreach (var tuple in Enumerable.Reverse(onStartingActions))
                    await tuple.Item1(tuple.Item2);
            }
        }
        internal async Task OnCompleted()
        {
            if (!completed)
            {
                completed = true;

                foreach (var tuple in Enumerable.Reverse(onCompletedActions))
                    await tuple.Item1(tuple.Item2);
            }
        }
    }
}
