using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace oAuthConsoleConsumer
{
    public class OAuthHttpClient : HttpClient
    {
        public OAuthHttpClient(string accessToken)
            : base(new OAuthTokenHandler(accessToken))
        {

        }

        class OAuthTokenHandler : MessageProcessingHandler
        {
            string _accessToken;
            public OAuthTokenHandler(string accessToken)
                : base(new HttpClientHandler())
            {
                _accessToken = accessToken;

            }
            protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                return request;
            }

            protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, System.Threading.CancellationToken cancellationToken)
            {
                return response;
            }
        }

    }
}
