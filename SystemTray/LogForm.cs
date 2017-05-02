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

        public LogForm()
        {
            InitializeComponent();
            LogTextBox.Dock = DockStyle.Fill;
        }
  

        public void SetLog(string LogMessage)
        {
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
