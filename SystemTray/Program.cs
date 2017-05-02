using System;
using System.Drawing;
using System.Resources;
using System.ComponentModel;
using System.Windows.Forms;
using System.ServiceProcess;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using LogInterfaces;


namespace SystemTray
{
    public class FocusdataSystemTray : System.Windows.Forms.Form
    {
        private System.ServiceProcess.ServiceController WSController;
        private System.Windows.Forms.NotifyIcon WSNotifyIcon;
        private System.ComponentModel.IContainer components;


        static public LogForm logForm = new LogForm();

        private Icon mDirIcon = new Icon("focusdata.ico");
        MenuItem[] mnuItems = new MenuItem[7];
        public FocusdataSystemTray()
        {

            InitializeComponent();

            var host = new ServiceHost(typeof(TestService),
                       new Uri("net.pipe://localhost"));

            host.AddServiceEndpoint(typeof(ITestService),
                                    new NetNamedPipeBinding(), "Test");
            host.Open();


            //keep the form hidden
            this.Hide();
            InitializeNotifyIcon();

            InitializeServiceController();



        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

            this.components = new System.ComponentModel.Container();
            this.WSNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            // 
            // WSNotifyIcon
            // 
            this.WSNotifyIcon.Text = "";
            this.WSNotifyIcon.Visible = true;
            // 
            // SysTray
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(8, 7);
            this.ControlBox = false;
            this.Enabled = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FocusdataSystemTray";
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;



        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new FocusdataSystemTray());
        }

        //public void ShowControlForm(object sender, EventArgs e)
        //{
        //	//show the Control form.
        //	WSControllerForm controlForm = new WSControllerForm();
        //	controlForm.Show();
        //}


        public void ExitControlForm(object sender, EventArgs e)
        {
            //Hide the NotifyIcon.
            WSNotifyIcon.Visible = false;

            this.Close();

        }

        private void InitializeServiceController()
        {

            this.WSController = new System.ServiceProcess.ServiceController();

            ServiceController[] AvailableServices = ServiceController.GetServices(".");

            foreach (ServiceController AvailableService in AvailableServices)
            {
                //Check the service name for IIS.
                if (AvailableService.ServiceName == "Focusdata Service")
                {
                    this.WSController.ServiceName = "Focusdata Service";
                    WSController.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                    SetButtonStatus();


                    return;
                }
            }


        }


        private void InitializeNotifyIcon()
        {
            //setup the Icon
            NotifyIcon WSNotifyIcon = new NotifyIcon();
            WSNotifyIcon.Icon = mDirIcon;
            WSNotifyIcon.Text = "Focusdata Bridge";
            WSNotifyIcon.Visible = true;

            //Create the MenuItem objects and add them to
            //the context menu of the NotifyIcon.


            //create the menu items array
            mnuItems[0] = new MenuItem("Log...", new EventHandler(this.Log_Click));
            mnuItems[0].DefaultItem = true;
            mnuItems[1] = new MenuItem("-");
            mnuItems[2] = new MenuItem("Start service", new EventHandler(this.Start_Click));
            mnuItems[2].Enabled = false;
            mnuItems[3] = new MenuItem("Stop service", new EventHandler(this.Stop_Click));
            mnuItems[3].Enabled = false;
            mnuItems[4] = new MenuItem("Pause service", new EventHandler(this.Pause_Click));
            mnuItems[4].Enabled = false;
            mnuItems[5] = new MenuItem("-");
            mnuItems[6] = new MenuItem("Exit", new EventHandler(this.ExitControlForm));

            //add the menu items to the context menu of the NotifyIcon
            ContextMenu notifyIconMenu = new ContextMenu(mnuItems);
            WSNotifyIcon.ContextMenu = notifyIconMenu;
        }


        private void SetButtonStatus()
        {
            //get the status of the service.
            string strServerStatus = WSController.Status.ToString();

            //check the status of the service and enable the 
            //command buttons accordingly.
            //MessageBox.Show(strServerStatus);

            if (strServerStatus == "Running")
            {
                //check to see if the service can be paused
                if (WSController.CanPauseAndContinue == true)
                {

                    mnuItems[4].Enabled = true;
                }
                else
                {

                    mnuItems[4].Enabled = false;
                }

                mnuItems[3].Enabled = true;

                mnuItems[2].Enabled = false;
            }
            else if (strServerStatus == "Paused")
            {
                mnuItems[2].Enabled = true;
                mnuItems[4].Enabled = false;
                mnuItems[3].Enabled = true;
            }
            else if (strServerStatus == "Stopped")
            {
                mnuItems[2].Enabled = true;
                mnuItems[4].Enabled = false;
                mnuItems[3].Enabled = false;
            }

        }

        private void Log_Click(object sender, System.EventArgs e)
        {

            
            //logForm.SetLog();
            logForm.Show();

        }


        private void Start_Click(object sender, System.EventArgs e)
        {
            //check the status of the service
            if (WSController.Status.ToString() == "Paused")
            {
                WSController.Continue();
            }
            else if (WSController.Status.ToString() == "Stopped")
            {

                //get an array of services this service depends upon, loop through 
                //the array and prompt the user to start all required services.
                ServiceController[] ParentServices = WSController.ServicesDependedOn;

                //if the length of the array is greater than or equal to 1.
                if (ParentServices.Length >= 1)
                {
                    foreach (ServiceController ParentService in ParentServices)
                    {
                        //make sure the parent service is running or at least paused.
                        if (ParentService.Status.ToString() != "Running" || ParentService.Status.ToString() != "Paused")
                        {

                            if (MessageBox.Show("This service is required. Would you like to also start this service?\n" + ParentService.DisplayName, "Required Service", MessageBoxButtons.YesNo).ToString() == "Yes")
                            {
                                //if the user chooses to start the service

                                ParentService.Start();
                                ParentService.WaitForStatus(ServiceControllerStatus.Running);
                            }
                            else
                            {
                                //otherwise just return.
                                return;
                            }
                        }
                    }
                }

                WSController.Start();
            }

            WSController.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
            SetButtonStatus();
        }

        private void Stop_Click(object sender, System.EventArgs e)
        {
            //check to see if the service can be stopped.
            if (WSController.CanStop == true)
            {

                //get an array of dependent services, loop through the array and 
                //prompt the user to stop all dependent services.
                ServiceController[] DependentServices = WSController.DependentServices;

                //if the length of the array is greater than or equal to 1.
                if (DependentServices.Length >= 1)
                {
                    foreach (ServiceController DependentService in DependentServices)
                    {
                        //make sure the dependent service is not already stopped.
                        if (DependentService.Status.ToString() != "Stopped")
                        {
                            if (MessageBox.Show("Would you like to also stop this dependent service?\n" + DependentService.DisplayName, "Dependent Service", MessageBoxButtons.YesNo).ToString() == "Yes")
                            {
                                // not checking at this point whether the dependent service can be stopped.
                                // developer may want to include this check to avoid exception.
                                DependentService.Stop();
                                DependentService.WaitForStatus(ServiceControllerStatus.Stopped);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }

                //check the status of the service
                if (WSController.Status.ToString() == "Running" || WSController.Status.ToString() == "Paused")
                {
                    WSController.Stop();
                }
                WSController.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                SetButtonStatus();
            }
        }

        private void Pause_Click(object sender, System.EventArgs e)
        {
            //check to see if the service can be paused and continue
            if (WSController.CanPauseAndContinue == true)
            {
                //check the status of the service
                if (WSController.Status.ToString() == "Running")
                {
                    WSController.Pause();
                }

                WSController.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Paused);
                SetButtonStatus();
            }

        }


    }
}