namespace EmbedIO.BearerToken
{
    using System;
    using System.Threading.Tasks;
    using Utilities;

    /// <summary>
    /// Basic Authorization Server Provider implementation.
    /// </summary>
    public class BasicAuthorizationServerProvider : IAuthorizationServerProvider
    {
        /// <inheritdoc />
        public async Task ValidateClientAuthentication(ValidateClientAuthenticationContext context)
        {
            var data = await context.HttpContext.GetRequestFormDataAsync().ConfigureAwait(false);

            if (data?.ContainsKey("grant_type") == true && data["grant_type"] == "password")
            {
                context.Validated(data.ContainsKey("username") ? data["username"] : string.Empty);
            }
            else
            {
                context.Rejected();
            }
        }

        /// <inheritdoc />
        public long GetExpirationDate() => DateTime.UtcNow.AddHours(12).Ticks;
    }
}