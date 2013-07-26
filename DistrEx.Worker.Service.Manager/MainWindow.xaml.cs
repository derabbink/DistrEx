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
        private string ServiceName = "Worker";
        private readonly ServiceController service; 

        public MainWindow()
        {
            InitializeComponent();

            ServiceController serviceController = ServiceController.GetServices().FirstOrDefault(i => i.ServiceName.Equals(ServiceName));

            if (serviceController == null)
            {
                ServiceStatus.Content = "Service is not installed.";
            }
            else
            {
                service = serviceController;
            }
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
            if (service != null)
            {
                UpdateStatus("Service is not installed.");
            }

            if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.Paused)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            UpdateStatus(ServiceControllerStatus.Running.ToString());

        }

        private void StopServiceButtonClick(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                UpdateStatus("Service is not installed.");
            }

            if (service.Status == ServiceControllerStatus.Running)
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
        
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                UpdateStatus(service.Status.ToString());
            }
            else
            {
                UpdateStatus("Service is not installed.");
            }
        }
        
        private void WindowsServicesClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void AutomaticServiceChecked(object sender, RoutedEventArgs e)
        {
            //Set service to automatic
        }
    }
}
