﻿namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Basic Authorization Server Provider implementation.
    /// </summary>
    public class BasicAuthorizationServerProvider : IAuthorizationServerProvider
    {
        /// <inheritdoc />
#pragma warning disable 1998
        public async Task ValidateClientAuthentication(ValidateClientAuthenticationContext context)
#pragma warning restore 1998
        {
            var data = context.HttpContext.RequestFormDataDictionary();

            if (data.ContainsKey("grant_type") == false || data["grant_type"].ToString() != "password")
            {
                context.Rejected();
            }
            else
            {
                context.Validated(data.ContainsKey("username") ? data["username"].ToString() : string.Empty);
            }
        }

        /// <inheritdoc />
        public long GetExpirationDate() => DateTime.UtcNow.AddHours(12).Ticks;
    }
}