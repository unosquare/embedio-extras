using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using EmbedIO.AspNetCore.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace EmbedIO.AspNetCore
{
    using HeaderDictionary = Wrappers.HeaderDictionary;

    internal class FeatureContext : IHttpRequestFeature, IHttpConnectionFeature, IHttpResponseFeature,
        IHttpAuthenticationFeature, IHttpRequestIdentifierFeature
    {
        public IHttpContext Context { get; }
        public IFeatureCollection Features { get; }

        private readonly List<Tuple<Func<object, Task>, object>> _onStartingActions =
            new List<Tuple<Func<object, Task>, object>>();

        private readonly List<Tuple<Func<object, Task>, object>> _onCompletedActions =
            new List<Tuple<Func<object, Task>, object>>();


        // Dropped features
        // - ITlsConnectionFeature
        // - IHttpRequestLifetimeFeature
        // - IHttpUpgradeFeature
        // - IHttpWebSocketFeature
        // - IHttpBufferingFeature
        // - IHttpSendFileFeature

        #region IHttpRequestFeature

        private IHeaderDictionary _requestHeaders;

        Stream IHttpRequestFeature.Body { get; set; }

        IHeaderDictionary IHttpRequestFeature.Headers
        {
            get => _requestHeaders ??= new HeaderDictionary(Context.Request.Headers);
            set => throw new NotSupportedException();
        }

        string IHttpRequestFeature.Method
        {
            get => Context.Request.HttpMethod;
            set => throw new NotSupportedException();
        }

        string IHttpRequestFeature.Path { get; set; }

        string IHttpRequestFeature.PathBase
        {
            get => "";
            set => throw new NotSupportedException();
        }

        string IHttpRequestFeature.Protocol
        {
            get
            {
                var protocolVersion = Context.Request.ProtocolVersion;
                return protocolVersion.Major != 1 || protocolVersion.Minor != 1
                    ? (protocolVersion.Major != 1 || protocolVersion.Minor != 0
                        ? "HTTP/" + protocolVersion.ToString(2)
                        : "HTTP/1.0")
                    : "HTTP/1.1";
            }
            set => throw new NotSupportedException();
        }

        string IHttpRequestFeature.QueryString
        {
            get
            {
                if (Context.Request.QueryString.Count == 0)
                    return "";

                return "?" + string.Join("&",
                           Context.Request.QueryString.Keys.OfType<string>()
                               .Select(k => k + "=" + Context.Request.QueryString[k]));
            }
            set => throw new NotSupportedException();
        }

        string IHttpRequestFeature.RawTarget
        {
            get => Context.Request.RawUrl;
            set => throw new NotSupportedException();
        }

        string IHttpRequestFeature.Scheme
        {
            get => Context.Request.Url.Scheme;
            set => throw new NotSupportedException();
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
            get => Context.Request.RemoteEndPoint.Port;
            set => throw new NotSupportedException();
        }

        string IHttpConnectionFeature.ConnectionId
        {
            get => Context.Id;
            set => throw new NotSupportedException();
        }

        #endregion

        #region IHttpResponseFeature

        private IHeaderDictionary _responseHeaders;

        Stream IHttpResponseFeature.Body { get; set; }

        IHeaderDictionary IHttpResponseFeature.Headers
        {
            get => _responseHeaders ??= new ResponseHeaderDictionary(Context.Response);
            set => throw new NotSupportedException();
        }

        bool IHttpResponseFeature.HasStarted => ResponseStarted;

        string IHttpResponseFeature.ReasonPhrase
        {
            get => Context.Response.StatusDescription;
            set => Context.Response.StatusDescription = value;
        }

        int IHttpResponseFeature.StatusCode
        {
            get => Context.Response.StatusCode;
            set => Context.Response.StatusCode = value;
        }

        void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (ResponseStarted)
                throw new InvalidOperationException("Cannot register new callbacks, the response has already started.");

            _onStartingActions.Add(Tuple.Create(callback, state));
        }

        void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (Completed)
                throw new InvalidOperationException(
                    "Cannot register new callbacks, the response has already completed.");

            _onCompletedActions.Add(Tuple.Create(callback, state));
        }

        #endregion

        #region IHttpAuthenticationFeature

        ClaimsPrincipal IHttpAuthenticationFeature.User
        {
            get => new ClaimsPrincipal(Context.User);
            set => throw new NotSupportedException();
        }

        #endregion

        #region IHttpRequestIdentifierFeature

        string IHttpRequestIdentifierFeature.TraceIdentifier
        {
            get => Context.Id;
            set
            {
                //  ignore
            }
        }

        #endregion

        internal bool ResponseStarted;
        internal bool Completed;

        internal FeatureContext(IHttpContext context)
        {
            Context = context;
            Features = new FeatureCollection(this);

            ((IHttpRequestFeature) this).Body = context.Request.InputStream;
            ((IHttpRequestFeature) this).Path = Context.Request.Url.AbsolutePath;

            ((IHttpResponseFeature) this).Body = context.Response.OutputStream;
        }

        internal async Task OnStart()
        {
            if (!ResponseStarted)
            {
                ResponseStarted = true;

                foreach (var tuple in Enumerable.Reverse(_onStartingActions))
                    await tuple.Item1(tuple.Item2);
            }
        }

        internal async Task OnCompleted()
        {
            if (!Completed)
            {
                Completed = true;

                foreach (var tuple in Enumerable.Reverse(_onCompletedActions))
                    await tuple.Item1(tuple.Item2);
            }
        }
    }
}