using System.ServiceProcess;

namespace DistrEx.Worker.Service
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new Worker()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
