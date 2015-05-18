using System.Net;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Retrieves a ValidationContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValidateClientAuthenticationContext GetValidationContext(this HttpListenerContext context)
        {
            return new ValidateClientAuthenticationContext(context);
        }

        /// <summary>
        /// Rejects a authentication challenge
        /// </summary>
        /// <param name="context"></param>
        public static void Rejected(this HttpListenerContext context)
        {
            context.JsonResponse("{'error': 'invalid_grant'}");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
