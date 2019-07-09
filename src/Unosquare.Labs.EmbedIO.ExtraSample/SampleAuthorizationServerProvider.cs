namespace EmbedIO.ExtraSample
{
    using System;
    using Utilities;
    using System.Threading;
    using System.Threading.Tasks;
    using BearerToken;

    internal class SampleAuthorizationServerProvider : IAuthorizationServerProvider
    {
        public async Task ValidateClientAuthentication(ValidateClientAuthenticationContext context, CancellationToken cancellationToken)
        {
            var data = await context.HttpContext.GetRequestFormDataAsync(cancellationToken).ConfigureAwait(false);

            if (data?.ContainsKey("grant_type") == true && data["grant_type"] == "password")
            {
                context.Identity.AddClaim(new System.Security.Claims.Claim("Role", "Admin"));

                context.Validated(data.ContainsKey("username") ? data["username"] : string.Empty);
            }
            else
            {
                context.Rejected();
            }
        }

        public long GetExpirationDate() => DateTime.UtcNow.AddHours(12).Ticks;
    }
}
