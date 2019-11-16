
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Owin;

using System.IO;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;

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

    public class AuthRequired : System.Web.Http.Filters.AuthorizationFilterAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext context)
        {
            //Get Auth Headers
            var authHeader = context.Request.Headers.Authorization;

            if (authHeader == null)
            {
                context.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (authHeader.Scheme != "Bearer")
            {
                context.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            String apiKey = authHeader.Parameter;

            base.OnAuthorization(context);
        }
    }



    /// <summary>
    /// A do nothing task that can be returned
    /// from functions that require task results
    /// when there's nothing to do.
    /// 
    /// This essentially returns a completed task
    /// with an empty value structure result.
    /// </summary>
    public class EmptyTask
    {
        public static Task Start()
        {
            var taskSource = new TaskCompletionSource<AsyncVoid>();
            taskSource.SetResult(default(AsyncVoid));
            return taskSource.Task as Task;
        }

        private struct AsyncVoid
        {
        }
    }



    /// <summary>
    /// Reads the Request body into a string/byte[] and
    /// assigns it to the parameter bound.
    /// 
    /// Should only be used with a single parameter on
    /// a Web API method using the [NakedBody] attribute
    /// </summary>
    public class NakedBodyParameterBinding : HttpParameterBinding
    {
        public NakedBodyParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {

        }


        public override Task ExecuteBindingAsync(System.Web.Http.Metadata.ModelMetadataProvider metadataProvider,
                                                    HttpActionContext actionContext,
                                                    CancellationToken cancellationToken)
        {
            var binding = actionContext
                .ActionDescriptor
                .ActionBinding;

            if (binding.ParameterBindings.Length > 1 ||
                actionContext.Request.Method == HttpMethod.Get)
                return EmptyTask.Start();

            var type = binding
                        .ParameterBindings[0]
                        .Descriptor.ParameterType;

            if (type == typeof(string))
            {
                return actionContext.Request.Content
                        .ReadAsStringAsync()
                        .ContinueWith((task) =>
                        {
                            var stringResult = task.Result;
                            SetValue(actionContext, stringResult);
                        });
            }
            else if (type == typeof(byte[]))
            {
                return actionContext.Request.Content
                    .ReadAsByteArrayAsync()
                    .ContinueWith((task) =>
                    {
                        byte[] result = task.Result;
                        SetValue(actionContext, result);
                    });
            }

            throw new InvalidOperationException("Only string and byte[] are supported for [NakedBody] parameters");
        }

        public override bool WillReadBody
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// An attribute that captures the entire content body and stores it
    /// into the parameter of type string or byte[].
    /// </summary>
    /// <remarks>
    /// The parameter marked up with this attribute should be the only parameter as it reads the
    /// entire request body and assigns it to that parameter.    
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class NakedBodyAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            if (parameter == null)
                throw new ArgumentException("Invalid parameter");

            return new NakedBodyParameterBinding(parameter);
        }
    }


    class OwinStartup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {


            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();


            config.Formatters.Clear();


            var jsonFormatter = new System.Net.Http.Formatting.JsonMediaTypeFormatter();
            jsonFormatter.UseDataContractJsonSerializer = false; // defaults to false, but no harm done
            jsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            jsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            

            config.Formatters.Add(jsonFormatter);


            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);

            //https redirect
            appBuilder.Use(async (context, next) =>
            {
                if (context.Request.IsSecure)
                    await next();
                else
                {
                    var withHttps = "https://" + context.Request.Host + context.Request.Path;
                    context.Response.Redirect(withHttps);
                }
            });

        }
    }
}


