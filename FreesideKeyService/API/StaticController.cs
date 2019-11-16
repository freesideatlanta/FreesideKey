using System.Web.Http;
using System.Net.Http;
using System.Reflection;
using System.IO;

namespace FreesideKeyService
{


    public class StaticController : ApiController
    {

        [HttpGet]
        [Route("")]
        [Route("index.html")]
        [Route("index")]
        public HttpResponseMessage RootPage() //Here To Catch Initial Page. All Other Requests are mapped to static
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "FreesideKeyService.StaticContent.index.html";

            Stream stream = assembly.GetManifestResourceStream(resourceName);


            //Respond with Content
            HttpResponseMessage resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
            resp.Content = new System.Net.Http.StreamContent(stream);

            resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            resp.Content.Headers.ContentType.CharSet = "utf-8";

            return resp;
        }
    }
}
