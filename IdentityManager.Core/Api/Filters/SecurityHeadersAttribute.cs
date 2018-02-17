using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TzIdentityManager.Api.Filters
{
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public SecurityHeadersAttribute()
        {
            EnableXfo = true;
            EnableCto = true;
            EnableCsp = true;
        }

        public bool EnableXfo { get; set; }
        public bool EnableCto { get; set; }
        public bool EnableCsp { get; set; }

        public override void OnActionExecuting(ActionExecutingContext actionExecutedContext)
        {
            base.OnActionExecuting(actionExecutedContext);
            if (actionExecutedContext != null &&
                actionExecutedContext.HttpContext.Response != null &&
                (actionExecutedContext.HttpContext.Response.StatusCode>=200 
                && actionExecutedContext.HttpContext.Response.StatusCode<=299)
                &&
                (actionExecutedContext.HttpContext.Response.Body == null ||
                 "text/html".Equals(actionExecutedContext.HttpContext.Response.ContentType, StringComparison.OrdinalIgnoreCase))
            )
            {
                if (EnableCto)
                {
                    actionExecutedContext.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                if (EnableXfo)
                {
                    actionExecutedContext.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                if (EnableCsp)
                {
                    actionExecutedContext.HttpContext.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src *");
                }
            }
        }
    }
}
