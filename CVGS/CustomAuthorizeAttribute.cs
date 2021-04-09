using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CVGS
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomAuthorizeAttribute  :AuthorizeAttribute
    {
        public string ViewName { get; set; }

        public CustomAuthorizeAttribute()
        {
            ViewName = "AuthorizationFailed";
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            IsUserAuthorized(filterContext);
        }

        void IsUserAuthorized(AuthorizationContext filterContext)
        {
            if (filterContext.Result == null)
            {
                return;
            }
            if(filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                ViewDataDictionary dic = new ViewDataDictionary();
                dic.Add("Message", "You don't have sufficient permissions for this operation.");
                var result = new ViewResult() { ViewName = this.ViewName, ViewData = dic };
                filterContext.Result = result;

                //or just
                //filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                // or
               // RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { action = "NotAuthorized", controller = "Error" }));

            }
            else
            {
                //ViewDataDictionary dic = new ViewDataDictionary();
                //dic.Add("Message", "Please login to complete this operation.");
                //var result = new ViewResult() { ViewName = "~/Views/Account/Login.cshtml", ViewData = dic };
                //filterContext.Result = result;
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}