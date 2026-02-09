using IQCSystemV2.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Forms
{
    public partial class MachineLotRequestExport: Form
    {
        private readonly WebViewFunctions mainWebViewFunctions;
        private readonly WebViewFunctions webViewFunctions;

        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";

        public MachineLotRequestExport(WebViewFunctions mainWebViewFunctions)
        {
            InitializeComponent();

            this.mainWebViewFunctions = mainWebViewFunctions;
            webViewFunctions = new WebViewFunctions(webView21);

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            Uri summonsRelease = new Uri($@"http://" + username + ":" + password + $"@10.248.1.10/BIPHMES/BenQGuru.eMES.Web.RM/FRMSummonsReleaseMP.aspx");
            await Login(username, password, summonsRelease);
        }

        private async Task Login(string username, string password, Uri link)
        {
            await webViewFunctions.SetTextBoxValueAsync("id", "txtUserCode", username);
            await Task.Delay(100);
            await webViewFunctions.SetTextBoxValueAsync("id", "txtPassword", password);
            await Task.Delay(100);
            await webViewFunctions.ClickButtonAsync("id", "cmdSubmit");

            webView21.Source = link;
        }

        private void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {

        }

        private void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {

        }
    }
}
