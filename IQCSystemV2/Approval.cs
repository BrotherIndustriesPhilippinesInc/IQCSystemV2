using IQCSystemV2.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2
{
    public partial class Approval: Form
    {
        private WebViewFunctions webViewFunctions;
        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";
        private string userID = "";
        private string checklot = "";

        private APIHandler apiHandler = new APIHandler();

        public Approval(string userID, string checklot)
        {
            InitializeComponent();
            webViewFunctions = new WebViewFunctions(webView21);

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;
            this.userID = userID;
            this.checklot = checklot;
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

        private async Task SetStorage()
        {
            Dictionary<string, string> post = new Dictionary<string, string> {
                { "id_number", this.userID.ToString() }
            };
            JObject data = await apiHandler.APIPostCall("http://apbiphbpswb02/homs/api/user/getUser.php", post);

            await webViewFunctions.ExecuteJavascript($"localStorage.setItem(\"user\", JSON.stringify({data["data"]}));");
        }

        private void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            //LOGIN
            Uri ordersCheckTable = new Uri($@"http://" + username + ":" + password + $"@10.248.1.10/BIPHMES/BenQGuru.eMES.Web.IQC/FIQCCheckResultMpNew.aspx?IQCLOT={this.checklot}");
            Login(username, password, ordersCheckTable);

            SetStorage();
        }

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            string isLoaded = await webViewFunctions.GetElementText("id", "lblTitle");
            if (isLoaded == "\"IQC Check Result Maint\"")
            {
                Console.WriteLine("Loaded");

                await webViewFunctions.ExecuteJavascript(@"
                    let link = document.createElement('link');
                    link.rel = 'stylesheet';
                    link.href = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css';
                    document.head.appendChild(link);

                    // Inject Bootstrap JS (optional if you need dropdowns/modals/etc.)
                    let script = document.createElement('script');
                    script.src = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js';
                    document.head.appendChild(script);

                    //SweetAlert
                    let script2 = document.createElement('script');
                    script2.src = 'https://cdn.jsdelivr.net/npm/sweetalert2@11';
                    document.head.appendChild(script2);
                ");

                await webViewFunctions.ExecuteJavascript(@"
                    // Wait until DOM is ready
                    (async () => {
                        console.log('IQC Check Result Maint loaded');
                        let employeeNumber = JSON.parse(localStorage.getItem(""user"")).EmpNo;

                        let btn = document.getElementById('cmdSubmit');
                        let lotValue = document.getElementById('txtIqcLotQuery').value;

                        // Auto-approve if button is disabled (approved already)
                        await updateApproval(lotValue);

                        // Attach click event for manual approval
                        btn.addEventListener('click', async function () {
                            await approveInspectionData();
                        });

                        
                        async function approveInspectionData() {
                            try {
                                
                                let data = { checklot: document.getElementById('txtIqcLotQuery').value, approver: employeeNumber};

                                const res = await fetch('http://apbiphiqcwb01:1116/API/InspectionDetails/ApproveInspection/', {
                                    method: 'POST',
                                    headers: { 'Content-Type': 'application/json' },
                                    body: JSON.stringify(data)
                                });

                                if (!res.ok) throw new Error('Server responded with ' + res.status);
                                console.log('Manual approval done ✅');
                                sendClosing();
                            } catch (err) {
                                alert('Approval failed: ' + err.message);
                            }
                        }

                        function sendClosing() {
                            window.chrome.webview.postMessage('close');
                        }

                        async function updateApproval(checklot) {
                            let isApproved = document.getElementById('cmdSubmit').disabled;
                            
                            if (isApproved) {
                                try {
                                    let data = { checklot: document.getElementById('txtIqcLotQuery').value, approver: employeeNumber};

                                    const res = await fetch('http://apbiphiqcwb01:1116/API/InspectionDetails/ApproveInspection/', {
                                        method: 'POST',
                                        headers: { 'Content-Type': 'application/json' },
                                        body: JSON.stringify(data)
                                    });

                                    if (!res.ok) throw new Error('Server responded with ' + res.status);

                                    console.log('Auto-approved status updated ✅');

                                    // Show SweetAlert confirmation
                                    Swal.fire({
                                        title: 'Approval Already Completed! 🎉',
                                        text: 'The inspection status has been successfully updated.',
                                        icon: 'success',
                                        confirmButtonText: 'Close'
                                    }).then((result) => {
                                        if (result.isConfirmed) {
                                            sendClosing();
                                        }
                                    });

                                } catch (err) {
                                    console.error('Auto-approval failed:', err);

                                    Swal.fire({
                                        title: 'Error 😖',
                                        text: 'Auto-approval failed: ' + err.message,
                                        icon: 'error'
                                    });
                                }
                            } else {
                                console.log('Button enabled → not approved yet.');
                            }
                        }
                    })();
                ");

            }
        }

        private async void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();
            if (message == "close")
            {
                // ⏳ wait 1 second before closing
                await Task.Delay(1000);
                this.Close();
            }
        }

    }
}