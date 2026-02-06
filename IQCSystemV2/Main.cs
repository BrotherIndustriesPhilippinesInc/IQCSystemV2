using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using IQCSystemV2.Forms;
using IQCSystemV2.Functions;

namespace IQCSystemV2
{
    public partial class Main : Form
    {
        // Constants
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        // Import user32.dll for sending messages
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public static string localIP;
        public static string UserIdNumber = "";

        private string centralizedLoginConnString = "Data Source=APBIPHBPSDB02;Initial Catalog=Centralized_LOGIN_DB;Persist Security Info=True;User ID=CAS_access;Password=@BIPH2024";
        public static string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void GetIPAddressFromCentralizedLogin()
        {
            SqlConnection CentralizedLogin = new SqlConnection(centralizedLoginConnString);

            CentralizedLogin.Open();
            SqlCommand SelectUserAccount = new SqlCommand("SP_SelectLoginRequestFromCentralizedLogin", CentralizedLogin);
            SelectUserAccount.CommandType = CommandType.StoredProcedure;
            SelectUserAccount.Parameters.AddWithValue("@IPAddress", localIP);
            SelectUserAccount.Parameters.AddWithValue("@SystemID", "73"); //palitan ng assigned system id
            SqlDataAdapter da = new SqlDataAdapter(SelectUserAccount);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                SqlDataReader reader = SelectUserAccount.ExecuteReader();

                if (reader.Read())
                {
                    UserIdNumber = reader["USERNAME"].ToString(); //ito ay depende kong anong gamit nyo na user name (ADID/ID number)
                }
            }
            else
            {

                LoginReminders loginReminders = new LoginReminders();
                loginReminders.ShowDialog();
                return;
            }
        }

        private APIHandler apiHandler;

        private WebViewFunctions webViewFunctions;

        public Main()
        {
            localIP = GetLocalIPAddress();
            GetIPAddressFromCentralizedLogin();

            InitializeComponent();

            apiHandler = new APIHandler();
            webViewFunctions = new WebViewFunctions(webView21);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //private void button1_MouseHover(object sender, EventArgs e)
        //{
        //    exit.ForeColor = Color.Red;
        //}

        //private void button1_MouseLeave(object sender, EventArgs e)
        //{
        //    exit.ForeColor = Color.White;
        //}

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 2) // Detect double-click
                {
                    this.WindowState = this.WindowState == FormWindowState.Normal
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;
                }
                else
                {
                    // Handle dragging the form
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            }
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            Dictionary<string, string> post = new Dictionary<string, string> {
                { "id_number", UserIdNumber.ToString() }
            };

            JObject data = await apiHandler.APIPostCall("http://apbiphbpswb02/homs/api/user/getUser.php", post);

            await webViewFunctions.ExecuteJavascript($"localStorage.setItem(\"user\", JSON.stringify({data["data"]}));");

            //JObject auth = await apiHandler.APIGetCall($"http://apbiphiqcwb01:1116/api/MachineLotRequests/ViaEmployeeNumber/{UserIdNumber}");
            //await webViewFunctions.ExecuteJavascript($"localStorage.setItem(\"auth\", JSON.stringify({auth}));");
        }

        private void maximize_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Normal
               ? FormWindowState.Maximized
               : FormWindowState.Normal;
        }

        private void minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            JObject data = webViewFunctions.CapturedMessage(e);
            string jsonString = data.ToString(Newtonsoft.Json.Formatting.Indented);

            if (data["actionName"].ToString() == "openInspectionWinform")
            {
                Inspection inspectionForm = new Inspection(UserIdNumber);
                inspectionForm.Show();
            }
            if (data["actionName"].ToString() == "openApprovalWinform")
            {
                Approval approvalForm = new Approval(UserIdNumber, data["data"]["checkLot"].ToString());
                approvalForm.Show();
            }
            if (data["actionName"].ToString() == "openHistoryWinform")
            {
                History approvalForm = new History(UserIdNumber, data["data"]["checkLot"].ToString());
                approvalForm.Show();
            }
            if (data["actionName"].ToString() == "openMachineLotRequestEditWinform")
            {

                MachineLotRequestEdit machineLotRequest = new MachineLotRequestEdit(data["data"]["releaseNo"].ToString(), data["data"]["modifiedBy"].ToString(), webViewFunctions);
                machineLotRequest.Show();
            }
            if (data["actionName"].ToString() == "openMachineLotRequestDeleteWinform")
            {
                MachineLotRequestDelete machineLotRequest = new MachineLotRequestDelete(data["data"]["releaseNo"].ToString(), webViewFunctions);
                machineLotRequest.Show();
            }
        }
    }
}
