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
    public partial class MachineLotRequestDelete: Form
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

        public MachineLotRequestDelete(string releaseNo, WebViewFunctions mainWebViewFunctions)
        {
            InitializeComponent();
            webViewFunctions = new WebViewFunctions(webView21);
            httpClient = new HttpClient();

            this.releaseNo = releaseNo;

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;

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
            await InjectDeleteInterceptor();

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
                    await webViewFunctions.ClickButtonAsync("name", "gridWebGrid$ctl02$Check");

                    await webViewFunctions.ClickButtonAsync("id", "cmdDelete");

                });
            }
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

            if (message == "DELETE_CONFIRMED")
            {
                // 1. Call YOUR API to delete from local DB
                // Ensure you have the 'releaseNo' available in this context. 
                // If releaseNo comes from the selected grid row, you might need to scrape it first.

                string result = await PostRawJsonAsync($"http://apbiphiqcwb01:1116/api/MachineLotRequests/deleteMachineLotRequest?releaseNo={releaseNo}", new { });

                if (result.Contains("Error"))
                {
                    MessageBox.Show("Local DB Delete Failed: " + result);
                    // Optional: Return here if you don't want to proceed with Legacy Delete on error
                }

                // 2. Resume the Legacy WebForms Delete
                await webViewFunctions.ExecuteJavascript("window.finalizeDelete();");

                JObject returnData = new JObject()
                {
                    { "actionName", "DeleteSuccess" },
                    { "message", result }
                };
                mainWebViewFunctions.SendDataToWeb(returnData, "EditSuccess");
                this.Close();
            }
        }

        private async Task InjectDeleteInterceptor()
        {
            string script = @"
    (function() {
        const btn = document.getElementById('cmdDelete');
        if (!btn) return;

        // 1. Remove the original inline 'onclick' (the one that pops up the alert)
        btn.removeAttribute('onclick'); 

        // 2. Add our automated listener
        btn.addEventListener('click', function(e) {
            e.preventDefault();

            // Aiko's Fix: AUTO-CONFIRM for automation. 
            // Removed: const userConfirmed = confirm('Delete(Y/N)?');
            const userConfirmed = true; 

            if (userConfirmed) {
                // Signal C# to sync with the local DB
                window.chrome.webview.postMessage('DELETE_CONFIRMED');
            }
        });

        window.finalizeDelete = function() {
            __doPostBack('cmdDelete', ''); 
        };
    })();
    ";

            await webViewFunctions.ExecuteJavascript(script);
        }
    }
}
