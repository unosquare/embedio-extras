namespace EmbedIO.BearerToken
{
    using System.Threading.Tasks;

    /// <summary>
    /// Authorization Server Provider interface.
    /// </summary>
    public interface IAuthorizationServerProvider
    {
        /// <summary>
        /// Validates a Client Authentication.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A task representing the client authentication.
        /// </returns>
        Task ValidateClientAuthentication(ValidateClientAuthenticationContext context);

        /// <summary>
        /// Gets a Expiration Date.
        /// </summary>
        /// <returns>Ticks until expiration date.</returns>
        long GetExpirationDate();

        /// <summary>
        /// Gets the Token Issuer.
        /// </summary>
        /// <returns>Name of the token issuer.</returns>
        string GetTokenIssuer();
    }
}
