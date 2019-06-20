namespace EmbedIO.BearerToken
{
    using System.Threading;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task representing the client authentication.
        /// </returns>
        Task ValidateClientAuthentication(ValidateClientAuthenticationContext context, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a Expiration Date.
        /// </summary>
        /// <returns>Ticks until expiration date.</returns>
        long GetExpirationDate();
    }
}
