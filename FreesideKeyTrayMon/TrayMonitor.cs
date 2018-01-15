using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace FreesideKeyTrayMon
{
    class TrayMonitor : ApplicationContext
    {
        NotifyIcon fsNotifyIcon;
        Thread statusMonThread;
        Thread serviceMonThread;
        bool stopMonitor;

        
        


        //MenuItems
        MenuItem exitMenuItem;
        MenuItem startService;
        MenuItem restartService;
        MenuItem stopService;

        //Server Status For ToolTips
        enum ServiceStatus
        {
            kUnknown,
            kNotInstalled,
            kStopped,
            kPaused,
            kRunning,
            kRestarting,
        };

        ServiceStatus serviceStatus = ServiceStatus.kUnknown;
        private void ServiceMonitor(object Data)
        {
            ServiceController sc = ServiceController.GetServices()
                   .FirstOrDefault(s => s.ServiceName == FSKeyCommon.Settings.serviceName);

            while (!stopMonitor)
            {

                //Check Service Installed
                if (sc == null)
                {
                    serviceStatus = ServiceStatus.kNotInstalled;
                }
                else
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Paused:
                        case ServiceControllerStatus.PausePending:
                            serviceStatus = ServiceStatus.kPaused;
                            break;

                        case ServiceControllerStatus.ContinuePending:
                        case ServiceControllerStatus.Running:
                            serviceStatus = ServiceStatus.kRunning;
                            break;

                        case ServiceControllerStatus.StartPending:
                            serviceStatus = ServiceStatus.kRestarting;
                            break;

                        case ServiceControllerStatus.Stopped:
                        case ServiceControllerStatus.StopPending:
                            serviceStatus = ServiceStatus.kStopped;
                            break;
                    }
                }
                
                switch (serviceStatus)
                {
                    case ServiceStatus.kUnknown:
                        fsNotifyIcon.Icon = Resources.fstray;
                        fsNotifyIcon.Text = "";
                        startService.Enabled = false;
                        restartService.Enabled = false;
                        stopService.Enabled = false;
                        break;
                    case ServiceStatus.kNotInstalled:
                        fsNotifyIcon.Icon = Resources.fstrayerror;
                        fsNotifyIcon.Text = "FS Key Service Not Installed!!!";
                        startService.Enabled = false;
                        restartService.Enabled = false;
                        stopService.Enabled = false;
                        break;
                    case ServiceStatus.kStopped:
                        fsNotifyIcon.Icon = Resources.fstrayerror;
                        fsNotifyIcon.Text = "FS Key Service Stopped.";
                        startService.Enabled = true;
                        restartService.Enabled = false;
                        stopService.Enabled = false;
                        break;
                    case ServiceStatus.kRunning:
                        fsNotifyIcon.Icon = Resources.fstray;
                        fsNotifyIcon.Text = "FS Key Monitor Running.";
                        startService.Enabled = false;
                        restartService.Enabled = true;
                        stopService.Enabled = true;
                        break;
                    case ServiceStatus.kRestarting:
                        fsNotifyIcon.Icon = Resources.fstrayerror;
                        fsNotifyIcon.Text = "FS KEy Service Restarting...";
                        startService.Enabled = false;
                        restartService.Enabled = false;
                        stopService.Enabled = false;
                        break;
                }


                Thread.Sleep(500);

            }
        }

        public TrayMonitor()
        {
            //Create Tray Icon
            fsNotifyIcon = new NotifyIcon();
            fsNotifyIcon.Icon = Resources.fstray;


            //Menu Items
            
            startService = new MenuItem("Start Service", new EventHandler(StartServiceHandler));
            startService.Enabled = false;
            restartService = new MenuItem("Restart Service", new EventHandler(RestartServiceHandler));
            restartService.Enabled = false;
            stopService = new MenuItem("Stop Service", new EventHandler(StopServiceHandler));
            stopService.Enabled = false;

            exitMenuItem = new MenuItem("Exit", new EventHandler(ExitHandler));


            fsNotifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
                {startService, restartService, stopService, new MenuItem("-"), exitMenuItem });
            fsNotifyIcon.Visible = true;

            stopMonitor = false;

            serviceMonThread = new Thread(ServiceMonitor);
            serviceMonThread.Start();
            statusMonThread = new Thread(StatusMon);
            statusMonThread.Start();
            
        }


        //Thread to monitor Status
        private void StatusMon(object data)
        {
            
            
            return;
            

        }

        //Context Menu Item Handlers
        //Basic End Process
        void ExitHandler(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            fsNotifyIcon.Visible = false;

            //Exit Monitor Thread
            stopMonitor = true;
            statusMonThread.Join(1000);
            serviceMonThread.Join(1000);

            Application.Exit();
        }

        void StartServiceHandler(object sender, EventArgs e)
        {
            //Get Handle to Service
            ServiceController sc = ServiceController.GetServices()
               .FirstOrDefault(s => s.ServiceName == FSKeyCommon.Settings.serviceName);
            if (sc == null)
            {
                serviceStatus = ServiceStatus.kNotInstalled;
            } else
            {
                sc.Start();
            }
        }

        void RestartServiceHandler(object sender, EventArgs e)
        {
            //Get Handle to Service
            ServiceController sc = ServiceController.GetServices()
               .FirstOrDefault(s => s.ServiceName == FSKeyCommon.Settings.serviceName);
            if (sc == null)
            {
                serviceStatus = ServiceStatus.kNotInstalled;
            }
            else
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Start();
            }

        }

        void StopServiceHandler(object sender, EventArgs e)
        {
            //Get Handle to Service
            ServiceController sc = ServiceController.GetServices()
               .FirstOrDefault(s => s.ServiceName == FSKeyCommon.Settings.serviceName);
            if (sc == null)
            {
                serviceStatus = ServiceStatus.kNotInstalled;
            }
            else
            {
                sc.Stop();
            }

        }
    }
}
