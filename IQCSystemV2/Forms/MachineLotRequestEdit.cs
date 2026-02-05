using IQCSystemV2.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Forms
{
    public partial class MachineLotRequestEdit: Form
    {
        private WebViewFunctions webViewFunctions;
        private HttpClient httpClient;
        private WebViewFunctions mainWebViewFunctions;

        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";
        private string releaseNo;
        private string modifiedBy;


        private int pageLoadCount = 0;
        private int loadCount = 0;
        private bool isSubmit = false;

        public MachineLotRequestEdit(string releaseNo, string modifiedBy, WebViewFunctions mainWebViewFunctions)
        {
            InitializeComponent();
            webViewFunctions = new WebViewFunctions(webView21);

            this.releaseNo = releaseNo;

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;
            this.modifiedBy = modifiedBy;
            this.mainWebViewFunctions = mainWebViewFunctions;
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

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            await InjectSaveInterceptor();

            pageLoadCount++;

            if (loadCount == 0)
            {
                string title = await webViewFunctions.GetElementText("id", "lblTitle");
                //Sanititize title remove "
                title = title.Replace("\"", "");
                if (title != "Summons Release")
                {
                    return;
                }
                loadCount++;

                //MessageBox.Show(releaseNo);
                await webViewFunctions.SetTextBoxValueAsync("id", "txtReleaseNoQuery", releaseNo);
                await webViewFunctions.ClickButtonAsync("id", "cmdQuery");

                await webViewFunctions.WaitForElementToExistAsync("[name='gridWebGrid$ctl02$ctl17']", async () => {
                    await webViewFunctions.ClickButtonAsync("name", "gridWebGrid$ctl02$ctl17");
                });




            }
        }

        private async Task InjectSaveInterceptor()
        {
            // Aiko's Note: We define a 'continueSave' function in JS that we can call later from C#
            string script = @"
                (function() {
                    const btn = document.getElementById('cmdSave');
                    if (!btn) return;

                    // 1. Remove any existing inline clicks to be safe (optional)
                    // btn.removeAttribute('onclick'); 

                    // 2. Attach our interceptor
                    btn.addEventListener('click', function(e) {
            
                        // CHECK: If we already flagged it as 'safe', let it proceed.
                        if (btn.dataset.processingComplete === 'true') {
                            return; // Allow the default ASP.NET submit
                        }

                        // STOP the default ASP.NET submit
                        e.preventDefault();
                        e.stopPropagation();

                        // SIGNAL C# that we are ready to scrape
                        // Ensure you have: webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                        window.chrome.webview.postMessage('SAVE_INITIATED');
                    });

                    // 3. Helper function C# will call after scraping is done
                    window.continueSave = function() {
                        const btn = document.getElementById('cmdSave');
                        btn.dataset.processingComplete = 'true'; // Set flag to bypass our interceptor
                        btn.click(); // Re-click the button naturally
                    };
                })();
                ";

            await webViewFunctions.ExecuteJavascript(script);
        }

        private async Task<string> PostRawJsonAsync<T>(string endpoint, T data)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    using (HttpResponseMessage response = await client.PostAsJsonAsync(endpoint, data))
                    {
                        // Aiko's Fix: Read the content regardless of success/fail
                        string responseBody = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            // Return the Status Code + The actual JSON error from the server
                            return $"Error {(int)response.StatusCode}: {response.ReasonPhrase} | Details: {responseBody}";
                        }

                        return responseBody;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"System Error: {ex.Message}";
            }
        }

        private async void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            if (message == "SAVE_INITIATED")
            {
                // 1. Scrape the data
                var collectedData = await ScrapeFormDataAsync();

                // 2. (Optional) Post to your API here
                // await PostRawJsonAsync("...", collectedData);

                string res = await PostRawJsonAsync("https://localhost:7246/api/MachineLotRequests/updateMachineLotRequest", collectedData);

                Console.WriteLine(res);

                // 3. RESUME the original form submission
                await webViewFunctions.ExecuteJavascript("window.continueSave();");

                JObject returnData = new JObject()
                {
                    { "actionName", "EditSuccess" },
                    { "message", res }
                };
                mainWebViewFunctions.SendDataToWeb(returnData, "EditSuccess");
                this.Close();

            }
        }

        private async Task<object> ScrapeFormDataAsync(){

            string releaseNo = await webViewFunctions.GetTextBoxValue("name", "txtReleaseNoEdit");
            string partCode = await webViewFunctions.GetTextBoxValue("name", "txtMaterialCodeEdit$ctl00");
            string partName = await webViewFunctions.GetTextBoxValue("id", "txtMaterialNameEdit");
            string vendorName = await webViewFunctions.GetSelectTextAsync("#drpVendorNameEdit");

            int quantity = Convert.ToInt32(await webViewFunctions.GetTextBoxValue("name", "txtMaterialQtyEdit"));

            bool yellowCard = await webViewFunctions.GetCheckboxStateAsync("#chbIsYellowCardEdit");

            string dciOtherNo = await webViewFunctions.GetTextBoxValue("id", "txtDesginNoEdit");

            string releaseReasonId = await webViewFunctions.GetSelectValueAsync("#drpReleaseReasonEdit");

            string remarks = await webViewFunctions.GetTextareaValueAsync("#txtReleaseContentEdit");

            return new
            {
                partCode,
                partName,
                vendorName,
                quantity,
                yellowCard,
                dciOtherNo,
                releaseReasonId,
                remarks,
                releaseNo
            };
        }
    }
}
