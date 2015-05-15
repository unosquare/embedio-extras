namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Authorization Server Provider interface
    /// </summary>
    public interface IAuthorizationServerProvider
    {
        /// <summary>
        /// Validates a Client Authentication
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ValidateClientAuthentication(ValidateClientAuthenticationContext context);
        /// <summary>
        /// Gets a Expiration Date
        /// </summary>
        /// <returns></returns>
        long GetExpirationDate();
    }

    /// <summary>
    /// Basic Authorization Server Provider implementation
    /// </summary>
    public class BasicAuthorizationServerProvider : IAuthorizationServerProvider
    {
        /// <summary>
        /// Validates a Client Authentication
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a Expiration Date
        /// </summary>
        /// <returns></returns>
        public long GetExpirationDate()
        {
            return DateTime.UtcNow.AddHours(12).Ticks;
        }
    }
}