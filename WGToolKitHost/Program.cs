using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WGTookKitTest
{
    class Program
    {
        public static void recordReceived(Object sender, WGToolKit.ControllerRecord recvRecord)
        {
            Console.WriteLine($"\rRecord Received @ {DateTime.Now.ToString()}: Card ID: {recvRecord.cardID}, Controller Number {((WGToolKit.WGController)sender).Connection.ID}.");
        }
        static void Main(string[] args)
        {
            List<WGToolKit.WGController> controllers = WGToolKit.WGController.ScanNet();

            Console.WriteLine("**JankREadTest**\n");
            foreach (WGToolKit.WGController controller in controllers)
            {

                controller.ReadCard(0);

            }


            Console.WriteLine("**Count Records**\n");
            foreach (WGToolKit.WGController controller in controllers)
            {
                int n = controller.GetNumRecords();
                if (n >= 0)
                {
                    Console.WriteLine($"Controller Number { controller.Connection.ID} has {n} records.");
                    controller.GetOneRecord(0);
                }
            }


            //List All Records
            Console.WriteLine("**Show All Records**\n");
            foreach (WGToolKit.WGController controller in controllers)
            {
                Console.WriteLine($"\n  Records for Controller Number { controller.Connection.ID}:");
                for (UInt32 recordIndex = 0; recordIndex < 60000; recordIndex++)
                {
                    WGToolKit.ControllerRecord[] crecords = controller.GetOneRecord(recordIndex);
                    if (crecords == null)
                        break;
                    if (crecords[0] == null)
                        break;
                    Console.WriteLine($"    Date: {crecords[0].readDateTime.ToString()}; Controller Number { controller.Connection.ID} CardId: {crecords[0].cardID}. Record Number: {crecords[0].recordIndex}.");

                    if (crecords[1] == null)
                        continue;
                    Console.WriteLine($"    Date: {crecords[1].readDateTime.ToString()}; Controller Number { controller.Connection.ID} CardId: {crecords[1].cardID}. Record Number: {crecords[1].recordIndex}.");


                }
            }


            Console.WriteLine("\nBeginning Watch.\n");
            Console.WriteLine("**Begin Initial Records**");



            foreach (WGToolKit.WGController controller in controllers)
                controller.startWatch(new EventHandler<WGToolKit.ControllerRecord>(recordReceived));
            Thread.Sleep(2000);
            Console.WriteLine("**End Initial Records**\n\n");

            while (true)
            {
                Console.Write("\rPress any Key To Stop.   Monitoring /  /  /  /  / ");
                Thread.Sleep(300);
                Console.Write("\rPress any Key To Stop.   Monitoring -  -  -  -  - ");
                Thread.Sleep(300);
                Console.Write("\rPress any Key To Stop.   Monitoring \\  \\  \\  \\  \\ ");
                Thread.Sleep(300);
                Console.Write("\rPress any Key To Stop.   Monitoring |  |  |  |  | ");
                Thread.Sleep(300);

                if (Console.KeyAvailable)
                    break;

            }
            //Flush Key Buffer
            while (Console.KeyAvailable)
                Console.ReadKey();

            foreach (WGToolKit.WGController controller in controllers)
                controller.stopWatch();


            Console.WriteLine("\n\nWatch Complete. Press Any Key To Exit.");

            while (!Console.KeyAvailable)
            {
                Thread.Sleep(300);
            }




        }


    }
}
