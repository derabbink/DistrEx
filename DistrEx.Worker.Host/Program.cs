using System;
using DistrEx.Worker.Workers;

namespace DistrEx.Worker.Host
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Interface.Worker worker = new DefaultWorker();
            worker.StartServices();

            Console.WriteLine("Service started.");
            Console.WriteLine("Press <ENTER> to quit.");
            Console.ReadLine();

            worker.Dispose();
        }
    }
}
