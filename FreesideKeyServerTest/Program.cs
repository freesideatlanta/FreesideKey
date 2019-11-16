using System;
using FreesideKeyService;


namespace FreesideKeyServerTest
{

    class Program
    {
        static void Main(string[] args)
        {

            FSKeyService FSKeyServer = new FSKeyService();

            Console.WriteLine("Starting Server Core...");
            FSKeyServer.StartHook();
            Console.WriteLine("Server Core Started");
            Console.WriteLine("Press Any Key to Stop Server Core");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("Server Core Stopping...");
            FSKeyServer.StopHook();
            Console.WriteLine("Server Core Stopped");
            Console.WriteLine("Press Any Key to Exit");
            Console.ReadKey();
            Console.WriteLine();
        }
    }
}
