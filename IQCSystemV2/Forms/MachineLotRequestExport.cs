using IQCSystemV2.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Forms
{
    public partial class MachineLotRequestExport: Form
    {
        private readonly WebViewFunctions mainWebViewFunctions;
        private readonly WebViewFunctions webViewFunctions;
        private readonly HttpClient httpClient;

        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";

        public MachineLotRequestExport(WebViewFunctions mainWebViewFunctions)
        {
            InitializeComponent();

            this.mainWebViewFunctions = mainWebViewFunctions;
            webViewFunctions = new WebViewFunctions(webView21);
            httpClient = new HttpClient();

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

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            await InjectExportInterceptor();

        }

        private async void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            // ... your existing DELETE_CONFIRMED logic ...

            if (message == "EXPORT_CLICKED")
            {
                // 1. Do any C# work you need here (Logging, UI updates, etc.)
                // Example: Update a status label or log the timestamp
                Debug.WriteLine("Export initiated by user at " + DateTime.Now);



                // 2. Resume the WebForms Submit
                // This calls the JS function we defined in InjectExportInterceptor
                await webViewFunctions.ExecuteJavascript("window.finalizeExport();");
            }
        }

        private async Task InjectExportInterceptor()
        {
            string script = @"
    (function() {
        const btn = document.getElementById('cmdComBineExport');
        // Safety check: Exit if button not found or already hooked
        if (!btn || btn.dataset.hasExportListener) return;

        // 1. Remove inline onclick if it exists (Clean slate)
        btn.removeAttribute('onclick'); 

        // 2. Add our automated listener
        btn.addEventListener('click', function(e) {
            // STOP the immediate submit
            e.preventDefault();

            // Signal C# that Export was clicked
            if(window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage('EXPORT_CLICKED');
            }
        });

        // 3. Define the Resume function that C# will call later
        window.finalizeExport = function() {
            const form = btn.form;
            if (form) {
                // ASP.NET Core/WebForms Requirement: 
                // We must manually inject the button's name/value because 
                // calling form.submit() programmatically usually omits the submit button itself.
                const hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = btn.name;
                hiddenInput.value = btn.value;
                form.appendChild(hiddenInput);

                // Resume intended purpose
                form.submit();
            }
        };

        // Mark as attached so we don't attach twice
        btn.dataset.hasExportListener = 'true';
    })();
    ";

            await webViewFunctions.ExecuteJavascript(script);
        }

        private async Task<string> PostRawJsonAsync<T>(string endpoint, T data)
        {
            try
            {
                // C# 7.3 requires the full block syntax
                using (HttpResponseMessage response = await httpClient.PostAsJsonAsync(endpoint, data))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return $"Error: {(int)response.StatusCode} ({response.ReasonPhrase})";
                    }

                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                return $"System Error: {ex.Message}";
            }
        }
    }
}
