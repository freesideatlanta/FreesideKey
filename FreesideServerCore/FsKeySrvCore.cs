using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Dynamic;

namespace FreesideServerCore
{
    public class FsKeySrvCore
    {
        private Thread mainThread; //Main Program Thread
        private Thread clientListener; //Client Listener Thread (Listen All Connections)
        
        private bool stopServerFlag = false;

        private String ApricotAPIKey = null;

        public void StartServer()
        {
            stopServerFlag = false;
            mainThread = new Thread(mainProc);
            mainThread.Start();

            return;
        }
        public void StopServer()
        {

            stopServerFlag = true;
            mainThread.Join(1000);
            mainThread.Abort();

            return;
        }

        public void mainProc(object Data)
        {
            //Initialization
            

            //Load API Key from Registry
            ApricotAPIKey = (String) Properties.Settings.Default["ApricotAPIKey"];

            //Start Client Listener for TrayMonitor Program Connection
            clientListener = new Thread(clientListenerProc);
            while (!stopServerFlag)
            {
                Thread.Sleep(100);
            }

        }

        //Listen for new TrayClient Connections
        private List<Thread> clientHandlerList; //Client Handler Thread (Per Client)
        public void clientListenerProc(object Data)
        {
            clientHandlerList = new List<Thread>();
            TcpListener server = new TcpListener(IPAddress.Any, FSKeyCommon.Settings.serverPort);

            server.Start();

            while (!stopServerFlag)
            {
                if (server.Pending())
                {
                    Socket s = server.AcceptSocket();
                    clientHandlerList.Add(new Thread(() => clientHandlerProc(s)));
                }

                //Purge Copmpleted Threads
                foreach (Thread t in clientHandlerList)
                    if (!t.IsAlive)
                        clientHandlerList.Remove(t);
            }

            //Wait for all clientHandlers to exit before exiting Thread
            foreach (Thread t in clientHandlerList)
            {
                t.Join(250);
                t.Abort();
            }

            clientHandlerList.Clear();

            return;
        }

        public void clientHandlerProc(Socket socket)
        {
            byte[] buffer = new byte[1024];
            while(!stopServerFlag)
            {
                if(socket.Available > 0)
                {
                    socket.Receive(buffer);
                    string sData = Encoding.ASCII.GetString(buffer);

                    dynamic jData = Newtonsoft.Json.Linq.JObject.Parse(sData);

                    switch((String) jData.cmd)
                    {
                        case "setAPIKey":
                            ApricotAPIKey = (String) jData.ApricotAPIKey;
                            Properties.Settings.Default["ApricotAPIKey"] = ApricotAPIKey;
                            Properties.Settings.Default.Save();
                            break;
                        case "getAPIKey":
                            dynamic sendData = new ExpandoObject();
                            sendData.cmd = "getApiKeyResp";
                            sendData.ApricotAPIKey = ApricotAPIKey;

                            string sendString = Newtonsoft.Json.JsonConvert.SerializeObject(sendData);
                            socket.Send(Encoding.ASCII.GetBytes(sendString));

                            break;
                    }
                }

                Thread.Sleep(100);
            }

            return;
            
        }

    }
}
