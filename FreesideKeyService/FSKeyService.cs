
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace FreesideKeyService
{
    public partial class FSKeyService : ServiceBase
    {
        private IDisposable _server = null;

        public FSKeyService()
        {
            InitializeComponent();
        }

        //Hooks For Testing Without Installing as a Service
        public void StartHook()
        {
            OnStart(null);
        }
        public void StopHook()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            //Setup Port For SSL;
            if (!SSLKeyManager.SetupSSLCert())
            {
                //TODO: Do Some Logging Here on Failure;
                OnStop();
                return;
            }

            //Validate/Initialize Database
            String ErrorMsg;
            //Check For Valid DataBase
            if(!FSLocalDb.KeyDbManager.ValidateDB(out ErrorMsg))
                if (FSLocalDb.KeyDbManager.DeleteDB(out ErrorMsg))      //Delete Invalid Database. Bit Extreme, but we don't have old DB versions to support. Someone has messed with the DB file.
                    if (FSLocalDb.KeyDbManager.CreateDB(out ErrorMsg))  //Create New Database
                        FSLocalDb.KeyDbManager.InitDB(out ErrorMsg);    //Initialize New Databse


            

            //Start WebApp
            StartOptions options = new StartOptions();
            options.Urls.Add($"https://*:{Properties.Settings.Default.serverPort}/");


            _server = WebApp.Start<Startup>(options);
            
        }

        protected override void OnStop()
        {
            if (_server != null)
            {
                _server.Dispose();
            }
            base.OnStop();
        }

    }
}