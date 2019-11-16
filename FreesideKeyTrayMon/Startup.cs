
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace FreesideKeyService
{
    public class RestrictToLocalhostAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext context)
        {
            if (!context.RequestContext.IsLocal)
            {
                context.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                return;
            }
            base.OnActionExecuting(context);
        }
    }


    class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute("DefaultRoute", "", new { controller = "StaticContent" });
            config.Routes.MapHttpRoute("StaticRoute", "{action}", new { controller = "StaticContent" });
            //config.Routes.MapHttpRoute("DefaultApi", "api/values/{id}", new { controller = "PublicValues", id = RouteParameter.Optional });



            appBuilder.UseWebApi(config);

        }
    }
}


