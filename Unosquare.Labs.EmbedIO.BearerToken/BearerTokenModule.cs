using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    public class BearerTokenModule : WebModuleBase
    {
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

                var authHeader = context.RequestHeader("Authorization");

                if (String.IsNullOrWhiteSpace(authHeader) == false && authHeader.StartsWith("Bearer "))
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

        public override string Name
        {
            get { return "Bearer Token Module"; }
        }
    }

    public static class Extensions
    {
        public static ValidateClientAuthenticationContext GetValidationContext(this HttpListenerContext context)
        {
            return new ValidateClientAuthenticationContext(context);
        }

        public static void Rejected(this HttpListenerContext context)
        {
            context.JsonResponse("{'error': 'invalid_grant'}");
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }

        public static Dictionary<string, string> RequestFormData(this HttpListenerContext context)
        {
            var request = context.Request;
            if (request.HasEntityBody == false) return null;

            using (var body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    var stringData = reader.ReadToEnd();

                    if (String.IsNullOrWhiteSpace(stringData)) return null;

                    return stringData.Split('&')
                        .ToDictionary(c => c.Split('=')[0],
                            c => Uri.UnescapeDataString(c.Split('=')[1]));
                }
            }
        }
    }
}