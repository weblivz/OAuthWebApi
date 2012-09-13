using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OAuthWebApi.Controllers
{
    public class AccountController : Controller
    {
        public FormsAuthentication FormsAuth { get; private set; }

        // **************************************
        // URL: /Account/LogOn
        // **************************************
        public ActionResult LogOn(string returnURL)
        {
            ViewBag.ReturnURL = returnURL;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LogOn(string userName, string password, string returnUrl, bool rememberMe = false)
        {
            // authenticate against the database
            if (MvcApplication.DataContext.Users.FirstOrDefault(u => u.Username == userName && u.Password == password) != null)
            {
                FormsAuthentication.SetAuthCookie(userName, rememberMe);
            }

            if (!String.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        // **************************************
        // URL: /Account/LogOff
        // **************************************
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}
