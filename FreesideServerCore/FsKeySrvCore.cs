using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Owin.SelfHost;



namespace FreesideServerCore
{


    private static readonly HttpListener KeySrvListener;
        private static bool _stopServer = false;
        private static Task _mainProc;

        private static string GenApiKey()
        {
            return "267c3aa9-78e7-4579-b26e-c679cdd594fc";

        }

        static FsKeySrvCore()
        {

            KeySrvListener = new HttpListener();
            KeySrvListener.Prefixes.Add($"http://localhost:{FSKeyCommon.Properties.Settings.Default.serverPort}/");

        }
        public void StartServer()
        {
            if (_mainProc != null && !_mainProc.IsCompleted) return; //Already started
            _mainProc = mainProc();

            return;
        }
        public void StopServer()
        {
            _stopServer = true;
            lock (KeySrvListener)
            {
                //Use a lock so we don't kill a request that's currently being processed
                KeySrvListener.Stop();
            }
            try
            {
                _mainProc.Wait();
            }
            catch { /* je ne care pas */ }


            return;
        }

        private static async Task mainProc()
        {
            KeySrvListener.Start();
            while (!_stopServer)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await KeySrvListener.GetContextAsync();
                    lock (KeySrvListener)
                    {
                        if (!_stopServer) ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpListenerException) return; //this gets thrown when the listener is stopped
                    //TODO: Log the exception
                }
            }
        }


        private static void ProcessRequest(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                try
                {
                    var handled = false;
                    switch (context.Request.Url.AbsolutePath)
                    {
                        //This is where we do different things depending on the URL
                        //TODO: Add cases for each URL we want to respond to
                        case "/settings":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":
                                    //Get the current settings
                                    response.ContentType = "application/json";

                                    //This is what we want to send back
                                    var responseBody = JsonConvert.SerializeObject(MyApplicationSettings);

                                    //Write it to the response stream
                                    var buffer = Encoding.UTF8.GetBytes(responseBody);
                                    response.ContentLength64 = buffer.Length;
                                    response.OutputStream.Write(buffer, 0, buffer.Length);
                                    handled = true;
                                    break;

                                case "PUT":
                                    //Update the settings
                                    using (var body = context.Request.InputStream)
                                    using (var reader = new StreamReader(body, context.Request.ContentEncoding))
                                    {
                                        //Get the data that was sent to us
                                        var json = reader.ReadToEnd();

                                        //Use it to update our settings
                                        UpdateSettings(JsonConvert.DeserializeObject<MySettings>(json));

                                        //Return 204 No Content to say we did it successfully
                                        response.StatusCode = 204;
                                        handled = true;
                                    }
                                    break;
                            }
                            break;
                    }
                    if (!handled)
                    {
                        response.StatusCode = 404;
                    }
                }
                catch (Exception e)
                {
                    //Return the exception details the client - you may or may not want to do this
                    response.StatusCode = 500;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    //TODO: Log the exception
                }
            }
        }
    }
}
