using System;
using System.Net;
using System.Threading.Tasks;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    public interface IAuthorizationServerProvider
    {
        Task ValidateClientAuthentication(ValidateClientAuthenticationContext context);
        long GetExpirationDate();
    }

    public class AuthorizationServerProvider : IAuthorizationServerProvider
    {
        public async Task ValidateClientAuthentication(ValidateClientAuthenticationContext context)
        {
            var data = context.HttpContext.RequestFormData();

            if (data.ContainsKey("grant_type") == false || data["grant_type"] != "password")
            {
                context.Rejected();
            }
            else
            {
                context.Validated(data.ContainsKey("username") ? data["username"] : string.Empty);
            }
        }

        public long GetExpirationDate()
        {
            return DateTime.UtcNow.AddHours(12).Ticks;
        }
    }
}