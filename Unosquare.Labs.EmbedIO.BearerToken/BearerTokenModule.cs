using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Unosquare.Labs.EmbedIO.BearerToken
{
    public class BearerTokenModule : WebModuleBase
    {
        public delegate bool ValidateHandler(WebServer server, HttpListenerContext context, out Dictionary<string, object> payload);

        public BearerTokenModule(ValidateHandler validation, IEnumerable<string> routes = null, string secretKey = "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF")
        {
            AddHandler("/token", HttpVerbs.Post, (server, context) =>
            {
                Dictionary<string, object> payload;

                if (validation(server, context, out payload))
                {
                    if (payload == null) payload = new Dictionary<string, object>();

                    payload.Add("EmbedIOAuthKey", 1);

                    context.JsonResponse(new BearerToken
                    {
                        Token = JWT.JsonWebToken.Encode(payload, secretKey, JWT.JwtHashAlgorithm.HS256),
                        TokenType = "bearer",
                        ExpirationDate = DateTime.UtcNow.AddHours(12).Ticks,
                        Username = payload.ContainsKey("User") ? payload["User"].ToString() : string.Empty
                    });
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
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

                        if (payload.ContainsKey("User"))
                        {
                            context.Response.Headers.Add("X-User: " + payload["User"]);
                        }

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

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return true;
            });
        }

        public static bool CommonValidateHandler(WebServer server, HttpListenerContext context,
            out Dictionary<string, object> payload)
        {
            payload = new Dictionary<string, object>();
            var data = context.RequestFormData();

            if (data.ContainsKey("grant_type") == false || data["grant_type"] != "password") return false;

            payload.Add("User", data.ContainsKey("username") ? data["username"] : string.Empty);

            // TODO: Check username

            return true;
        }

        public override string Name
        {
            get { return "Bearer Token Module"; }
        }
    }

    public static class Extensions
    {
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
