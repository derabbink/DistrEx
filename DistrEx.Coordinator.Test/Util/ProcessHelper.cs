using System.Diagnostics;
using Microsoft.Test.ApplicationControl;

namespace DistrEx.Coordinator.Test.Util
{
    internal static class ProcessHelper
    {
        /// <summary>
        ///     blocks until started
        /// </summary>
        /// <param name="exeFile">path relative to App base path</param>
        /// <returns></returns>
        internal static AutomatedApplication Start(string exeFile)
        {
            var result = new OutOfProcessApplication(new OutOfProcessApplicationSettings
            {
                ProcessStartInfo = new ProcessStartInfo(exeFile),
                ApplicationImplementationFactory = new UIAutomationOutOfProcessApplicationFactory()
            });
            result.Start();

            return result;
        }

        /// <summary>
        ///     blocks until closed
        /// </summary>
        /// <param name="process"></param>
        internal static void Stop(AutomatedApplication process)
        {
            process.Close();
        }
    }
}
