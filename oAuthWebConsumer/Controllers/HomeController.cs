using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using OAuthShared;

namespace oAuthWebConsumer.Controllers
{
    public class HomeController : Controller
    {
        private static string CLIENT_ID = "samplewebapiconsumer";
        private static string CLIENT_SECRET = "samplesecret";
        private static string API_ENDPOINT = "http://localhost:30777/api/values";
        private static string AUTHORIZATION_ENDPOINT = "http://localhost:30777/OAuth/Authorise";
        private static string TOKEN_ENDPOINT = "http://localhost:30777/OAuth/Token";
        private static string AUTHORIZATION_CALLBACK = "http://localhost:40551/";


        public IAuthorizationState Authorization { get; private set; }
        public UserAgentClient Client { get; set; }

        public HomeController()
        {
            var authServer = new AuthorizationServerDescription()
            {
                AuthorizationEndpoint = new Uri(AUTHORIZATION_ENDPOINT),
                TokenEndpoint = new Uri(TOKEN_ENDPOINT),
            };
            this.Client = new UserAgentClient(authServer, CLIENT_ID, CLIENT_SECRET);
            this.Authorization = new AuthorizationState();
            this.Authorization.Callback = new Uri(AUTHORIZATION_CALLBACK);
        }

        /// <summary>
        /// Check if we have a code that means it is a return from an oAuth redirect where we want to 
        /// pass the code to the server and make the oAuth call. In this case the code is used for a
        /// one time only call and will change upon every refresh.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["code"]))
            {
                try
                {
                    this.Client.ProcessUserAuthorization(Request.Url, this.Authorization);
                    var valueString = string.Empty;
                    if (!string.IsNullOrEmpty(this.Authorization.AccessToken))
                    {
                        valueString = CallAPI(this.Authorization);
                    }
                    ViewBag.Values = valueString;
                }
                catch (ProtocolException ex)
                {
                }
            }
            return View();
        }

        /// <summary>
        /// A method that calls onto the API from the server using the code that has been retrieved using
        /// a previous oAuth call.
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        private string CallAPI(IAuthorizationState authorization)
        {
            var webClient = new WebClient();
            webClient.Headers["Content-Type"] = "application/json";
            webClient.Headers["X-JavaScript-User-Agent"] = "API Explorer";
            this.Client.AuthorizeRequest(webClient, this.Authorization);
            var valueString = webClient.DownloadString(API_ENDPOINT);
            return valueString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult GetValues()
        {
            bool isOK = false;
            bool requiresAuth = false;
            string redirectURL = "";
            if (Session["AccessToken"] == null)
            {
                this.Authorization.Scope.AddRange(OAuthUtilities.SplitScopes(API_ENDPOINT));
                Uri authorizationUrl = this.Client.RequestUserAuthorization(this.Authorization);
                requiresAuth = true;
                redirectURL = authorizationUrl.AbsoluteUri;
                isOK = true;
            }
            else
            {
                requiresAuth = false;
            }
            return new JsonResult()
            {
                Data = new
                {
                    OK = isOK,
                    RequiresAuth = requiresAuth,
                    RedirectURL = redirectURL
                }
            };
        }
    }
}
