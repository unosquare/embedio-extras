namespace Unosquare.EmbedIO.AspNetCore
{
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Http.Features.Authentication;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    internal class FeatureCollection : IFeatureCollection
    {
        private static readonly Func<FeatureContext, object> IdentityFeature = x => x;

        private static readonly Dictionary<Type, Func<FeatureContext, object>> DefaultFeatures = new Dictionary<Type, Func<FeatureContext, object>>()
        {
            { typeof(HttpListenerContext), c => c /* new Func<FeatureContext, object>(StandardFeatureCollection.\u003C\u003Ec.\u003C\u003E9.\u003C\u002Ecctor\u003Eb__16_1) */ },
            { typeof(IHttpRequestFeature), IdentityFeature },
            { typeof(IHttpConnectionFeature), IdentityFeature },
            { typeof(IHttpResponseFeature), IdentityFeature },
            { typeof(IHttpSendFileFeature), IdentityFeature },
            { typeof(ITlsConnectionFeature), c => c /* new Func<FeatureContext, object>(StandardFeatureCollection.\u003C\u003Ec.\u003C\u003E9.\u003C\u002Ecctor\u003Eb__16_0) */ },
            { typeof(IHttpBufferingFeature), IdentityFeature },
            { typeof(IHttpRequestLifetimeFeature), IdentityFeature },
            { typeof(IHttpUpgradeFeature), IdentityFeature },
            { typeof(IHttpWebSocketFeature), IdentityFeature },
            { typeof(IHttpAuthenticationFeature), IdentityFeature },
            { typeof(IHttpRequestIdentifierFeature), IdentityFeature },
        };

        public FeatureContext Context { get; }
        public bool IsReadOnly => true;
        public int Revision => 0;

        public object this[Type key]
        {
            get => !_features.TryGetValue(key, out var result) ? null : result(Context);
            set
            {
                _features[key] = c => value;
            }
        }

        private readonly Dictionary<Type, Func<FeatureContext, object>> _features;

        public FeatureCollection(FeatureContext context)
        {
            Context = context;
            _features = DefaultFeatures.ToDictionary(p => p.Key, p => p.Value);
        }

        public TFeature Get<TFeature>() => this[typeof(TFeature)] is TFeature feature ? feature : default(TFeature);

        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            foreach (var pair in _features)
                yield return new KeyValuePair<Type, object>(pair.Key, pair.Value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
