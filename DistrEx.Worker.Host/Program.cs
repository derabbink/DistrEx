using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistrEx.Worker.Workers;

namespace DistrEx.Worker.Host
{
    class Program
    {
        static void Main(string[] args)
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
