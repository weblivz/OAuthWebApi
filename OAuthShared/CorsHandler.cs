using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OAuthShared
{
    /// <summary>
    /// The cors handler - this is a delegating handler and so will only be run for WebAPI derived requests.
    /// </summary>
    public class CorsHandler : DelegatingHandler {
        const string Origin = "Origin";
        const string AccessControlRequestMethod = "Access-Control-Request-Method";
        const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            bool isCorsRequest = request.Headers.Contains(Origin);
            bool isPreflightRequest = request.Method == HttpMethod.Options;
            if (isCorsRequest) {
                if (isPreflightRequest) {
                    return Task.Factory.StartNew<HttpResponseMessage>(() => {
                                                                          HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                                                                          response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

                                                                          string accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
                                                                          if (accessControlRequestMethod != null) {
                                                                              response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
                                                                          }

                                                                          string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
                                                                          if (!string.IsNullOrEmpty(requestedHeaders)) {
                                                                              response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
                                                                          }

                                                                          return response;
                                                                      }, cancellationToken);
                } else {
                    return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(t => {
                                                                                                            HttpResponseMessage resp = t.Result;
                                                                                                            resp.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                                                                                                            return resp;
                    }, TaskContinuationOptions.ExecuteSynchronously);   // ### Need to ExecuteSynchronously as doing Asyc hangs the app
                }
            } else {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}