using System.ServiceProcess;

namespace DistrEx.Worker.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.workerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.workerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // workerServiceProcessInstaller
            // 
            this.workerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.workerServiceProcessInstaller.Password = null;
            this.workerServiceProcessInstaller.Username = null;
            // 
            // workerServiceInstaller
            // 
            this.workerServiceInstaller.Description = "Exposes WCF endpoints. ";
            this.workerServiceInstaller.ServiceName = "Worker";
            this.workerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.workerServiceProcessInstaller,
            this.workerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller workerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller workerServiceInstaller;
    }
}