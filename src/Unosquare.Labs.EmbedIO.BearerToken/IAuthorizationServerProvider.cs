namespace Unosquare.Labs.EmbedIO.BearerToken
{
    using System.Threading.Tasks;

    /// <summary>
    /// Authorization Server Provider interface
    /// </summary>
    public interface IAuthorizationServerProvider
    {
        /// <summary>
        /// Validates a Client Authentication
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task ValidateClientAuthentication(ValidateClientAuthenticationContext context);

        /// <summary>
        /// Gets a Expiration Date
        /// </summary>
        /// <returns></returns>
        long GetExpirationDate();
    }
}
