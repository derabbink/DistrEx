using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Windows;

namespace DistrEx.Worker.Service.Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InstallButtonClick(object sender, RoutedEventArgs e)
        {
            //const string commandText = "installutil DistrEx.Worker.Service";
            //var process = new Process();
            //var startInfo = new ProcessStartInfo
            //{
            //    WindowStyle = ProcessWindowStyle.Normal,
            //    FileName = "cmd.exe",
            //    Arguments = commandText
            //};
            //process.StartInfo = startInfo;
            //process.Start(); 
        }

        private void StartServiceButtonClick(object sender, RoutedEventArgs e)
        {
            ServiceController serviceController = ServiceController.GetServices().FirstOrDefault(i => i.ServiceName.Equals("Worker"));

            if (serviceController != null && (serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.Paused))
            {
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            UpdateStatus(ServiceControllerStatus.Running.ToString());

        }

        private void StopServiceButtonClick(object sender, RoutedEventArgs e)
        {
            ServiceController service = ServiceController.GetServices().FirstOrDefault(i => i.ServiceName.Contains("Worker"));

            if (service != null && service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            UpdateStatus(ServiceControllerStatus.Stopped.ToString());
        }

        private void UpdateStatus(string status)
        {
            StatusLable.Content = status; 
        }
    }
}
