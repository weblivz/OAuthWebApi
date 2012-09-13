using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DotNetOpenAuth.OAuth2;

namespace oAuthConsoleConsumer
{
    public static class ResourceOwnerCredentials
    {
        private static string CLIENT_ID = "samplewebapiconsumer";
        private static string CLIENT_SECRET = "samplesecret";
        private static string TEST_USERNAME = "steven";
        private static string TEST_PASSWORD = "pwd";
        private static string API_ENDPOINT = "http://localhost:30777/api/values";
        private static string AUTHORIZATION_ENDPOINT = "http://localhost:30777/OAuth/Authorise";
        private static string TOKEN_ENDPOINT = "http://localhost:30777/OAuth/Token";


        public static void Run()
        {
            Console.WriteLine("Enter to run Resource Owner Credentials demo.");

            #region initial request

            // get an access token for the username and password
            var state = GetAccessToken();

            var tokenexpires = state.AccessTokenExpirationUtc;
            var token = state.AccessToken;
            var refresh = state.RefreshToken;

            Console.WriteLine("Expires = {0}", tokenexpires);
            Console.WriteLine();
            Console.WriteLine("Token = {0}", token);
            Console.WriteLine();
            Console.WriteLine("Refresh Token = {0}", refresh);
            Console.WriteLine();

            #region sych request

            Console.WriteLine("");
            Console.WriteLine("Hit a key to make a sychronous request.");
            Console.WriteLine("");
            Console.ReadKey();

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(API_ENDPOINT);
            myReq.Headers.Add("Authorization", "Bearer " + token);
            WebResponse myReqResp = myReq.GetResponse();
            System.IO.StreamReader myReqRespStream = new System.IO.StreamReader(myReqResp.GetResponseStream());
            Console.WriteLine(myReqRespStream.ReadToEnd());
            Console.WriteLine("");
            Console.WriteLine("Request Complete.");
            Console.ReadKey();
            Console.WriteLine("");

            #endregion



            // get a reference to the access token
            var httpClient = new OAuthHttpClient(token)
            {
                BaseAddress = new Uri(API_ENDPOINT)
            };


            Console.WriteLine("Calling web api...");
            Console.WriteLine("...");

            // make the request
            var response = httpClient.GetAsync("").Result;
            Console.WriteLine("Got Response");

            // if ok write the result
            if (response.StatusCode == HttpStatusCode.OK)
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            else
                Console.WriteLine("Error");

            Console.WriteLine();
            /*  */
            #endregion

            #region refreshing

            Console.WriteLine("Refreshing token ...");

            // first update the state to get a new token
            state = GetAccessToken(state.RefreshToken);

            tokenexpires = state.AccessTokenExpirationUtc;
            token = state.AccessToken;
            refresh = state.RefreshToken;

            Console.WriteLine("Refresh Expires = {0}", tokenexpires);
            Console.WriteLine();
            Console.WriteLine("Token = {0}", token);
            Console.WriteLine();

            httpClient = new OAuthHttpClient(token)
            {
                BaseAddress = new Uri(API_ENDPOINT)
            };


            Console.WriteLine("Enter to call web api...");
            Console.WriteLine("...");

            // make the request
            response = httpClient.GetAsync("").Result;
            Console.WriteLine("Got Response");

            // if ok write the result
            if (response.StatusCode == HttpStatusCode.OK)
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            else
                Console.WriteLine("Error");

            Console.WriteLine();
            Console.WriteLine("Finished calling API with refresh token");
            Console.WriteLine();
            /**/
            #endregion

            Console.WriteLine();
            Console.WriteLine("Done");
            Console.ReadLine();
        }


        private static IAuthorizationState GetAccessToken()
        {
            return GetAccessToken(null);
        }

        private static IAuthorizationState GetAccessToken(string refresh)
        {
            var authorizationServer = new AuthorizationServerDescription
            {
                //AuthorizationEndpoint = new Uri(AUTHORIZATION_ENDPOINT),
                TokenEndpoint = new Uri(TOKEN_ENDPOINT),
                ProtocolVersion = ProtocolVersion.V20,
            };

            // get a reference to the auth server
            var client = new UserAgentClient(authorizationServer, CLIENT_ID, CLIENT_SECRET);

            // now get a token
            IAuthorizationState ostate;
            if (refresh == null)
                ostate = client.ExchangeUserCredentialForToken(TEST_USERNAME, TEST_PASSWORD, new[] { API_ENDPOINT });
            else
            {
                // we had previously authenticated so we can use the token rather than the credentials to get a new access token
                ostate = new AuthorizationState(new[] { API_ENDPOINT });
                ostate.RefreshToken = refresh;
                client.RefreshAuthorization(ostate);
            }

            // return result
            return ostate;
        }
    }
}
