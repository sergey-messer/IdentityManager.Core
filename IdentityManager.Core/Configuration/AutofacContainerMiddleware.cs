using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TzIdentityManager.Configuration
{
    public class AutofacContainerMiddleware
    {
        private readonly RequestDelegate _next;

        public AutofacContainerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var svc3 = httpContext.RequestServices.CreateScope();//GetService<IDataService>();
            //request.Properties[HttpPropertyKeys.DependencyScope] = new AutofacScope(scope);
            //IServiceScopeFactory ccc;
            //ccc.
            var serviceCollection = new ServiceCollection();
            //httpContext.RequestServices.GetServices(x=>x)

            //HttpPropertyKeys.DependencyScope
            //var t=new HttpContext()
            //httpContext.Response.Headers.Add("X-Xss-Protection", "1");
            //httpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            //httpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            return _next(httpContext);
        }
    }
}
