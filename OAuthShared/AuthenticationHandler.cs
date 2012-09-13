using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetOpenAuth.OAuth2;
using System.IdentityModel.Tokens;
using System.Net;
using System.Threading;
using System.Security.Principal;
using System.Web;
using Microsoft.IdentityModel.Claims;

namespace OAuthShared
{
    /// <summary>
    /// The authentication handler - this is a delegating handler and so will only be run for WebAPI derived requests.
    /// </summary>
    public class AuthenticationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                string authHeader = request.Headers.GetValues("Authorization").First();

                string header = "Bearer ";

                if (string.CompareOrdinal(authHeader, 0, header, 0, header.Length) == 0)
                {
                    using (AuthenticationConfiguration config = new AuthenticationConfiguration())
                    {
                        var resourceServer = new WebAPIResourceServer(new StandardAccessTokenAnalyzer(config.CreateAuthorizationServerSigningServiceProvider(), config.CreateResourceServerEncryptionServiceProvider()));
                        var principal = resourceServer.GetPrincipal(request, request.RequestUri.AbsoluteUri);
                        if (principal != null)
                        {
                            SetPrincipal(principal);
                        }
                    }
                }
                else
                {
                    return SendUnauthorizedResponse();
                }
            }
            catch (SecurityTokenValidationException)
            {
                return SendUnauthorizedResponse();
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(
                (task) =>
                {
                    var response = task.Result;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        SetAuthenticateHeader(response);
                    }

                    return response;
                }, TaskContinuationOptions.ExecuteSynchronously);    // ### Need to ExecuteSynchronously as doing Asyc hangs the app
        }

        private Task<HttpResponseMessage> SendUnauthorizedResponse()
        {
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                SetAuthenticateHeader(response);
                return response;
            });
        }

        protected virtual void SetAuthenticateHeader(HttpResponseMessage response)
        {
            //response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(_authN.Configuration.DefaultAuthenticationScheme));
        }

        protected virtual void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
    public static class Principal
    {
        public static ClaimsPrincipal Anonymous
        {
            get
            {
                var anonId = new ClaimsIdentity();
                var anonPrincipal = ClaimsPrincipal.CreateFromIdentity(anonId);
                return anonPrincipal as ClaimsPrincipal;
            }
        }

        public static ClaimsPrincipal Create(string authenticationType, params Claim[] claims)
        {
            return new ClaimsPrincipal(new IClaimsIdentity[] { new ClaimsIdentity(claims, authenticationType) });
        }
    }
}
