using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Unosquare.EmbedIO.AspNetCore
{
    internal class FeatureCollection : IFeatureCollection
    {
        private static readonly Func<FeatureContext, object> IdentityFeature = x => x;

        private static readonly Dictionary<Type, Func<FeatureContext, object>> defaultFeatures = new Dictionary<Type, Func<FeatureContext, object>>()
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
            //{ typeof(IServiceProvidersFeature), IdentityFeature },
        };

        public FeatureContext Context { get; }
        public bool IsReadOnly => true;
        public int Revision => 0;

        public object this[Type key]
        {
            get
            {
                Func<FeatureContext, object> result;

                if (!features.TryGetValue(key, out result))
                    return null;

                return result(Context);
            }
            set
            {
                features[key] = c => value;
            }
        }

        private Dictionary<Type, Func<FeatureContext, object>> features = new Dictionary<Type, Func<FeatureContext, object>>();

        public FeatureCollection(FeatureContext context)
        {
            Context = context;
            features = defaultFeatures.ToDictionary(p => p.Key, p => p.Value);
        }

        public TFeature Get<TFeature>()
        {
            object result = this[typeof(TFeature)];

            if (result is TFeature)
                return (TFeature)result;
            else
                return default(TFeature);
        }
        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            foreach (var pair in features)
                yield return new KeyValuePair<Type, object>(pair.Key, pair.Value);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
