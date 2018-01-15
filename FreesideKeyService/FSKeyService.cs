using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace FreesideKeyService
{
    public partial class FSKeyService : ServiceBase
    {
        
        private Thread serviceThread;
        private bool stopService; //Stop service flag

        private bool serverInit; //signals Server is ready to respond to client requests




        private string APIKey;

        public FSKeyService()
        {
            InitializeComponent();
            stopService = false;
            serviceThread = new Thread(mainProcess);
        }

        protected override void OnStart(string[] args)
        {
            serviceThread.Start();
        }

        protected override void OnStop()
        {
            stopService = true;
            serviceThread.Join(500);
            serviceThread.Abort();
        }


        //Listen on Named Pipe for 
        private void IPCClientListener(object Data)
        {

        }

        private void IPCClientHandler(object Data)
        {
            //Message Pump
            //Read in struct. Copy to Local. Write Out Struct
            MemoryMappedFile ipcFile = MemoryMappedFile.OpenExisting(FSKeyIPCComm.FSIPCConsensus.IPCFile.ToString()+"Client", 4096, MemoryMappedFileAccess.ReadWrite);
        }

        private void IPCServerStatus(object Data)
        {

        }


        private void mainProcess(object Data)
        {
            //Initialization
            
            //try and load API key
            APIKey = (String) Registry.LocalMachine.GetValue("SOFTWARE\\FSKeyMon", "APIkey");

            Thread ClientThread = new Thread(IPCClientHandler);
            Thread ServerThread = new Thread(IPCServerStatus);


                



        }
    }
}
