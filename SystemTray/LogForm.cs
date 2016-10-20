using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTray
{
    public partial class LogForm : Form
    {

        private String LogMessage="";

        public LogForm()
        {
            InitializeComponent();
            LogTextBox.Dock = DockStyle.Fill;

            var watch = new FileSystemWatcher();
            watch.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            watch.Filter = "log.txt";
            watch.NotifyFilter = NotifyFilters.LastWrite; //more options
            watch.Changed += new FileSystemEventHandler(OnChanged);
            watch.EnableRaisingEvents = true;




        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {

            LogMessage = File.ReadAllText(e.FullPath, Encoding.UTF8);
            
        }

        public void SetLog()
        {
            this.LogTextBox.Clear();
            this.LogTextBox.AppendText(LogMessage);
        }


        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
