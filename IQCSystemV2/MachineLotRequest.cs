using IQCSystemV2.Forms;
using IQCSystemV2.Functions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace IQCSystemV2
{
    public partial class MachineLotRequest: Form
    {
        public readonly string partCode;
        public readonly string checkLot;
        private readonly WebViewFunctions webViewFunctions;


        public MachineLotRequest(string partCode, string checkLot)
        {
            InitializeComponent();
            this.partCode = partCode;
            this.checkLot = checkLot;

            webViewFunctions = new WebViewFunctions(machineLotRequestWebView);
        }

        private void machineLotRequestWebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {

        }

        private async void machineLotRequestWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            await InitializeData();
        }

        private async Task InitializeData()
        {
            await webViewFunctions.ExecuteJavascript($"document.getElementById('partCode').textContent = '{partCode}';");
            await webViewFunctions.ExecuteJavascript($"document.getElementById('checkLot').textContent = '{checkLot}';");
        }

        private void machineLotRequestWebView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                JObject action = webViewFunctions.CapturedMessage(e);

                if (action["actionName"].ToString() == "SubmitMachineLotRequest")
                {
                    Console.WriteLine(action["data"]["data"]);
                    MachineLotRequestData machineLotRequestData = JsonConvert.DeserializeObject<MachineLotRequestData>(action["data"]["data"].ToString());
                    MachineLotRequestAutomation machineLotRequest = new MachineLotRequestAutomation(machineLotRequestData, webViewFunctions, this);
                    machineLotRequest.Show();
                }
            }
            catch (Exception ex)
            {
                JObject templateGenerationError = new JObject()
                {
                    { "actionName", "error" },
                    { "data", new JObject { { "message", ex.Message } } }
                };
                webViewFunctions.SendDataToWeb(templateGenerationError, "mainWebView_WebMessageReceived");
                MessageBox.Show("mainWebView_WebMessageReceived: " + ex.Message);
            }
        }
    }
}
