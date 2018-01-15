using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace FreesideKeyServerTest
{

    class Program
    {
        static void Main(string[] args)
        {

         FreesideServerCore.FsKeySrvCore fsSrv = new FreesideServerCore.FsKeySrvCore();

            Console.WriteLine("Starting Server Core...");
            fsSrv.StartServer();
            Console.WriteLine("Server Core Started");
            Console.WriteLine("Press Any Key to Stop Server Core");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("Server Core Stopping...");
            fsSrv.StopServer();
            Console.WriteLine("Server Core Stopped");
            Console.WriteLine("Press Any Key to Exit");
            Console.ReadKey();
            Console.WriteLine();
        }
    }
}
