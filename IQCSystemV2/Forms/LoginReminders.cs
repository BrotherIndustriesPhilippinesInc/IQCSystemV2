using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Forms
{
    public partial class LoginReminders: Form
    {
        public LoginReminders()
        {
            InitializeComponent();
        }

        private void OpenPortalBtn_Click(object sender, EventArgs e)
        {
            Process.Start(@"\\apbiphsh07\D0_ShareBrotherGroup\19_BPS\Installer\BPS Centralized Login\setup.exe");
        }

        private void LoginReminders_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
