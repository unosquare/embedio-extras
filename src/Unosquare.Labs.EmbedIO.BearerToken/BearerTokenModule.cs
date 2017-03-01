using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Unosquare.Swan;

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
        /// <param name="routes">The routes to authorization</param>
        /// <param name="secretKey">The secret key to encrypt tokens</param>
        public BearerTokenModule(IAuthorizationServerProvider authorizationServerProvider,
            IEnumerable<string> routes = null, SymmetricSecurityKey secretKey = null)
        {
            if (secretKey == null)
            {
                secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            }

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
                        var token = authHeader.Replace("Bearer ", string.Empty);
                        var tokenHandler = new JwtSecurityTokenHandler();
                        SecurityToken validatedToken;
                        tokenHandler.ValidateToken(token, new TokenValidationParameters
                        {
                            IssuerSigningKey = secretKey
                        }, out validatedToken);

                        return false;
                    }
                    catch (Exception ex)
                    {
                        ex.Log(nameof(BearerTokenModule));
                    }
                }

                context.Rejected();

                return true;
            });
        }

        /// <summary>
        /// Returns Module Name
        /// </summary>
        public override string Name => nameof(BearerTokenModule).Humanize();
    }
}