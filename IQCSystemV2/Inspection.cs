using IQCSystemV2.Functions;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace IQCSystemV2
{
    public partial class Inspection: Form
    {
        private WebViewFunctions webViewFunctions;
        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";
        public Inspection()
        {
            InitializeComponent();
            webViewFunctions = new WebViewFunctions(webView21);

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;

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

        private void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            //LOGIN
            Uri ordersCheckTable = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/BenQGuru.eMES.Web.IQC/FIQCCheckTableMP.aspx");
            Login(username, password, ordersCheckTable);
        }

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            string isLoaded = await webViewFunctions.GetElementText("id", "lblTitle");
            if (isLoaded == "\"IQC Check Result Maint\"")
            {
                Console.WriteLine("Loaded");

                //INJECT BOOTSTRAP
                await webViewFunctions.ExecuteJavascript(@"
                    let link = document.createElement('link');
                    link.rel = 'stylesheet';
                    link.href = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css';
                    document.head.appendChild(link);

                    // Inject Bootstrap JS (optional if you need dropdowns/modals/etc.)
                    let script = document.createElement('script');
                    script.src = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js';
                    document.head.appendChild(script);
                ");

                //LOAD PANELS
                await webViewFunctions.ExecuteJavascript(@"
                    let div = document.createElement('div');
                    div.innerHTML = `
                        <div class=""ms-2"">
                            <div class=""mt-2"">
                                <button id=""back"" class=""btn btn-warning"" onclick=""back()"">Back</button>
                            </div>

                            <div class=""d-flex gap-2 mt-2"">
                                <button id=""threeD"" class=""btn btn-primary"">3D</button>
                                <button id=""twoD"" class=""btn btn-primary"">2D</button>
                                <button id=""wi"" class=""btn btn-primary"">Work Instructions</button>
                                <button id=""artwork"" class=""btn btn-primary"">Artwork</button>
                                <button id=""dci"" class=""btn btn-primary"">DCI</button>
                                <button id=""ng"" class=""btn btn-primary"">NG Illustration</button>
                                <button id=""qhc"" class=""btn btn-primary"">Quality History Card</button>
                            </div>
                            <div id=""items-container""></div>
                            
                        </div>
                    `;
                    document.body.prepend(div);
                ");


                //LOAD SCRIPTS
                await webViewFunctions.ExecuteJavascript(@"
                    //Receive from webview
                    function receiveFromWebView(callback, executeOnce = false) {
                        return new Promise((resolve, reject) => {
                            window.chrome.webview.addEventListener(
                                'message',
                                function (event) {
                                    console.log(""Received message from C#: "", event.data);
                                    let data = JSON.parse(event.data);

                                    if (data[""actionName""] === ""error"") {
                                        errorTracker(data[""actionName""], data.data.message);
                                    } else {
                                        // Execute the provided callback function with the received data
                                        if (typeof callback === 'function') {
                                            callback(data);
                                        }
                                    }

                                    resolve(data);
                                },
                                { once: executeOnce } // Automatically removes the listener after being called once
                            );
                        });
                    }

                    //Send to webview
                    function sendToWebView(actionName,data = {}) {
                        if (window.chrome && window.chrome.webview) {
                            let payload = {
                                actionName: actionName,
                                data
                            }
                            let payloadString = JSON.stringify(payload);
                            window.chrome.webview.postMessage(payloadString);
                        } else {
                            console.warn(""WebView2 not available. Ensure you're running in a WebView2 environment."");
                        }
                    }


                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //GENERAL VARIABLES
                    let startTime = 0;
                    let timerInterval = null;

                    // Get item list
                    let part_code = document.getElementById(""txtMCodeQuery"").value
                    let items;
                    async function getAllItems(){
                        let data;
                        sendToWebView(""GetList"", {part_code});
                        await receiveFromWebView((e)=>{
                            data = e;
                        });
                        return data;
                    }

                    (async () => {
                        items = await getAllItems();
                        console.log(""Got items:"", items);

                        startTimer();

                        // Map your button IDs to category keys
                        const categoryMap = {
                            threeD: ""threeD"",
                            twoD: ""2d"",
                            wi: ""wi"",
                            artwork: ""artwork"",
                            dci: ""dci"",
                            ng: ""ng"",
                            qhc: ""qhc""
                        };

                        Object.entries(categoryMap).forEach(([btnId, category]) => {
                            let btn = document.getElementById(btnId);
                            if (!items[category] || !items[category][category] || items[category][category].length === 0) {
                                btn.setAttribute(""disabled"", ""true"");
                                btn.classList.add(""btn-secondary"");  // make it grey
                                btn.classList.remove(""btn-primary""); // remove active color
                            }
                        });
                    })();

                    function generateItems(category) {
                        let container = document.getElementById('items-container');
                        container.innerHTML = ''; 
                        container.className = ""d-flex gap-2 flex-wrap mt-2"";

                        items[category][category].forEach(item => {
                            let div = document.createElement('div');
                            div.innerHTML = `
                                <button class=""iqc-items btn btn-info"" 
                                        data-category=""${category}"" 
                                        data-item=""${item.fileName}"">
                                    ${item.fileName}
                                </button>
                            `;
                            container.appendChild(div);
                        });

                        // Attach events to ALL new buttons
                        document.querySelectorAll('.iqc-items').forEach(btn => {
                            btn.addEventListener('click', function () {
                                let category = this.getAttribute('data-category');
                                let fileName = this.getAttribute('data-item');
                                console.log(`${category} & ${fileName} clicked`);
                                if (category == ""threeD"") 
                                {
                                    openThreeD(fileName);      
                                }else{
                                    openItems(category, fileName);
                                }
                            });
                        });
                    }

                    //Open 3D
                    function openThreeD(fileName) {
                        sendToWebView(""OpenThreeD"", {fileName});
                    }


                    //Open Items
                    function openItems(category, fileName) {
                        let container = document.getElementById('items-container');

                        // Build the file path (adjust if using http or file://)
                        let pdfUrl = `http://apbiphbpsts01:8080/iqc/resources/${category}/${fileName}`;
                        pdfUrl = pdfUrl.replace(""xlsm"", ""pdf"");
                        pdfUrl = pdfUrl.replace(""xlsx"", ""pdf"");

                        // Clear old content
                        container.innerHTML = '';

                        // Embed the PDF viewer
                        container.innerHTML = `
                            <div class=""w-100"" style=""height:50vh;"">
                                <embed src=""${pdfUrl}"" type=""application/pdf"" width=""100%"" height=""100%"" />
                            </div>
                        `;
                    }

                    // Hook buttons to generator
                    document.getElementById('threeD').addEventListener('click', () => generateItems('threeD'));
                    document.getElementById('twoD').addEventListener('click', () => generateItems('twoD'));
                    document.getElementById('wi').addEventListener('click', () => generateItems('wi'));
                    document.getElementById('artwork').addEventListener('click', () => generateItems('artwork'));
                    document.getElementById('dci').addEventListener('click', () => generateItems('dci'));
                    document.getElementById('ng').addEventListener('click', () => generateItems('ng'));
                    document.getElementById('qhc').addEventListener('click', () => generateItems('qhc'));

                    document.getElementsByClassName('iqc-items')[0].addEventListener('click', function () {
                        
                        let category = this.getAttribute('data-category');
                        let fileName = this.getAttribute('data-item');
                        console.log(`${category} & ${fileName} clicked`);
                        openItems(category, fileName);
                    });

                    function back(){
                        window.history.go(-2);
                    }

                    //Timer Function
                    function startTimer() {
                        startTime = Date.now();

                        // Clear any existing interval
                        if (timerInterval) clearInterval(timerInterval);

                        // Update every second
                        timerInterval = setInterval(() => {
                            const elapsed = Math.floor((Date.now() - startTime) / 1000); // seconds
                            const txt = document.getElementById(""txtCheckTime"");
                            if (txt) txt.value = elapsed; // update the input value
                        }, 1000);
                    }

                ");

            }
        }

        private async void webView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            OpenToolHandler openTool;

            try
            {
                JObject action = webViewFunctions.CapturedMessage(e);

                if (action["actionName"].ToString() == "GetList")
                {
                    openTool = new OpenToolHandler(action["data"]["part_code"].ToString());
                    webViewFunctions.SendDataToWeb(await openTool.ReturnAllAsync(), "ReturnAll");
                }
                else if (action["actionName"].ToString() == "OpenThreeD")
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\01 3D Drawing\\" + action["data"]["fileName"].ToString(),
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
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