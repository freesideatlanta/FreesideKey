using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net.Http;

using Microsoft.Owin.FileSystems;

using Owin;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FreesideKeyService
{


    public class StaticController : ApiController
    {

        [HttpGet]
        [Route("")]
        public HttpResponseMessage RootPage() //Here To Catch Initial Page. All Other Requests are mapped to static
        {
            return StaticPages();
        }

        //Serve Non-Access Controlled Pages
        [HttpGet]
        [Route("static/{*reqString?}")]
        public HttpResponseMessage StaticPages(string reqString = "index.html")
        {
            //Static Pages Directory.
            EmbeddedResourceFileSystem staticPages = new EmbeddedResourceFileSystem("FreesideKeyService.StaticContent");

            //Parse path
            String reqNoQueryString = (reqString.IndexOf("?") >= 0) ? reqString.Substring(0, reqString.IndexOf("?")) : reqString; //Strip QueryString

            String fileName = (reqNoQueryString.LastIndexOf("/") > 0) ? reqNoQueryString.Substring(reqNoQueryString.LastIndexOf("/")).TrimStart('/') : reqNoQueryString;  //Get FileName Alone
            fileName += fileName.Contains(".") ? "" : ".html"; //Add .html extension by default
            String pathName = (reqNoQueryString.LastIndexOf("/") > 0) ? reqNoQueryString.Substring(0, reqNoQueryString.LastIndexOf("/")) : ""; //Get Path Alone.
            pathName = "/" + pathName.Replace("/", "."); //Convert flat file format;
            pathName += pathName.Length > 1 ? "." : "";  //Add Trailing . unless root directory

            String extention = fileName.Substring(fileName.LastIndexOf(".")+1);

            //Try Get File
            IFileInfo fileInfo;
            staticPages.TryGetFileInfo(pathName + fileName, out fileInfo);

            
            //404 If Not Found
            if(fileInfo == null)
            {
                HttpResponseMessage notFoundResp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
                return notFoundResp;
            }
            

            //Create Read Stream
            System.IO.Stream s = fileInfo.CreateReadStream();

            //Respond with Content
            HttpResponseMessage resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);

            resp.Content = new System.Net.Http.StreamContent(s);

            switch (extention)
            {
                case "html":
                    resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
                    resp.Content.Headers.ContentType.CharSet = "utf-8";
                    break;
                case "css":
                    resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/css");
                    resp.Content.Headers.ContentType.CharSet = "utf-8";
                    break;
                case "js":
                    resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/javascript");
                    resp.Content.Headers.ContentType.CharSet = "utf-8";
                    break;
                case "png":
                    resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                    break;
                case "ico":
                    resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/x-icon");
                    break;
                case "manifest":
                    resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/manifest+json");
                    break;


                    
            }


            return resp;
        }
    }
}
