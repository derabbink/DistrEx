using System.ServiceProcess;

namespace DistrEx.Worker.Service
{
    public partial class Worker : ServiceBase
    {
        public Worker()
        {
            InitializeComponent();

            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Starting Service");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Stopping Service");
        }
    }
}
