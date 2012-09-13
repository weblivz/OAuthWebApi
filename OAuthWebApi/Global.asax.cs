using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using OAuthShared;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using System.Data.Objects;

namespace OAuthWebApi
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        public static DatabaseKeyNonceStore KeyNonceStore { get; set; }

        protected void Application_BeginRequest()
        {
 
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            AreaRegistration.RegisterAllAreas();

            // add in the message handlers which will only be run for requests to WebAPI derived contollers.            
            GlobalConfiguration.Configuration.MessageHandlers.Add(new CorsHandler());
            GlobalConfiguration.Configuration.MessageHandlers.Add(new AuthenticationHandler());

            // initialize a nonce store
            KeyNonceStore = new DatabaseKeyNonceStore();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }


        /// <summary>
        /// Gets the transaction-protected database connection for the current request.
        /// </summary>
        public static OAuthModelContainer DataContext
        {
            get
            {
                OAuthModelContainer dataContext = DataContextSimple;
                if (dataContext == null)
                {
                    dataContext = new OAuthModelContainer();
                    dataContext.Connection.Open();
                    DataContextSimple = dataContext;
                }

                return dataContext;
            }
        }

        private static OAuthModelContainer DataContextSimple
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Items["DataContext"] as OAuthModelContainer;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items["DataContext"] = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            CommitAndCloseDatabaseIfNecessary();
        }

        private static void CommitAndCloseDatabaseIfNecessary()
        {
            var dataContext = DataContextSimple;
            if (dataContext != null)
            {
                dataContext.SaveChanges();
                dataContext.Connection.Close();
            }
        }
    }
}