// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;

namespace Unosquare.Labs.EmbedIO.OwinMiddleware.Collections
{
    /// <summary>
    /// This wraps HttpListenerRequest's WebHeaderCollection (NameValueCollection) and adapts it to 
    /// the OWIN required IDictionary surface area. It remains fully mutable, but you will be subject 
    /// to the header validations performed by the underlying collection.
    /// </summary>
    internal sealed class RequestHeadersDictionary : HeadersDictionaryBase
    {
        private readonly HttpListenerRequest _request;

        internal RequestHeadersDictionary(HttpListenerRequest request)
        {
            _request = request;
        }

        // This override enables delay load of headers
        protected override WebHeaderCollection Headers
        {
            get { return (WebHeaderCollection)_request.Headers; }
            set { throw new InvalidOperationException(); }
        }
    }

}
