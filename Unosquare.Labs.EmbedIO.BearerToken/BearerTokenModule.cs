using System;
using System.Collections.Generic;
using System.Linq;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    /// <summary>
    /// EmbedIO module to allow authorizations with Bearer Tokens
    /// </summary>
    public class BearerTokenModule : WebModuleBase
    {
        private const string AuthorizationHeader = "Authorization";

        /// <summary>
        /// Module's Constructor
        /// </summary>
        /// <param name="authorizationServerProvider">The AuthorizationServerProvider to use</param>
        /// <param name="routes">The routes to authorizate</param>
        /// <param name="secretKey">The secret key to encrypt tokens</param>
        public BearerTokenModule(IAuthorizationServerProvider authorizationServerProvider,
            IEnumerable<string> routes = null, string secretKey = "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF")
        {
            AddHandler("/token", HttpVerbs.Post, (server, context) =>
            {
                var validationContext = context.GetValidationContext();
                authorizationServerProvider.ValidateClientAuthentication(validationContext);

                if (validationContext.IsValidated)
                {
                    context.JsonResponse(new BearerToken
                    {
                        Token = validationContext.GetToken(secretKey),
                        TokenType = "bearer",
                        ExpirationDate = authorizationServerProvider.GetExpirationDate(),
                        Username = validationContext.ClientId
                    });
                }
                else
                {
                    context.Rejected();
                }

                return true;
            });

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Any, (server, context) =>
            {
                if (routes != null && routes.Contains(context.RequestPath()) == false) return false;

                var authHeader = context.RequestHeader(AuthorizationHeader);

                if (string.IsNullOrWhiteSpace(authHeader) == false && authHeader.StartsWith("Bearer "))
                {
                    try
                    {
                        var token = authHeader.Replace("Bearer ", "");
                        var payload = JWT.JsonWebToken.DecodeToObject(token, secretKey) as IDictionary<string, object>;

                        if (payload == null || payload.Count == 0) throw new Exception("Invalid token");

                        return false;
                    }
                    catch (JWT.SignatureVerificationException)
                    {
                        server.Log.DebugFormat("Invalid token {0}", authHeader);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        server.Log.Error(ex);
                    }
                }

                context.Rejected();

                return true;
            });
        }

        /// <summary>
        /// Returns Module Name
        /// </summary>
        public override string Name => "Bearer Token Module";
    }
}