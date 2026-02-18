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
        private string userID = "";

        private APIHandler apiHandler = new APIHandler();

        public Inspection(string userID)
        {
            InitializeComponent();
            webViewFunctions = new WebViewFunctions(webView21);

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;
            this.userID = userID;
        }

        private async Task SetStorage()
        {
            Dictionary<string, string> post = new Dictionary<string, string> {
                { "id_number", this.userID.ToString() }
            };
            JObject data = await apiHandler.APIPostCall("http://apbiphiqcwb01:1116/api/Accounts/getUser", post);

            await webViewFunctions.ExecuteJavascript($"localStorage.setItem(\"user\", JSON.stringify({data}));");
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

            SetStorage();
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

                await webViewFunctions.ExecuteJavascript(@"
                    // Inject SweetAlert2 CSS
                    let swalLink = document.createElement('link');
                    swalLink.rel = 'stylesheet';
                    swalLink.href = 'https://cdn.jsdelivr.net/npm/sweetalert2@11.25.1/dist/sweetalert2.min.css';
                    document.head.appendChild(swalLink);

                    // Inject SweetAlert2 JS
                    let swalScript = document.createElement('script');
                    swalScript.src = 'https://cdn.jsdelivr.net/npm/sweetalert2@11.25.1/dist/sweetalert2.all.min.js';
                    document.head.appendChild(swalScript);
                ");

                await webViewFunctions.ExecuteJavascript(@"
                    // Call machine lot request function

                    function machineLotRequest() {
                        let partCode = document.getElementById(""txtMCodeQuery"");
                        let checkLot = document.getElementById(""txtIqcLotQuery"");
                        console.log(partCode.value);
                        sendToWebView('machineLotRequest', {partCode: partCode.value, checkLot: checkLot.value});
                    }
                    
                ");


                //LOAD PANELS
                await webViewFunctions.ExecuteJavascript(@"
                    let div = document.createElement('div');
                    div.innerHTML = `
                        <div class=""ms-2"">
                            <div class=""mt-2"">
                                <button id=""back"" class=""btn btn-warning"" onclick=""back()"">Back</button>
                            </div>

                            <div class=""d-flex gap-2 mt-2 justify-content-around"">
                                <div> 
                                    <button id=""threeD"" class=""btn btn-primary"">3D</button>
                                    <button id=""twoD"" class=""btn btn-primary"">2D</button>
                                    <button id=""wi"" class=""btn btn-primary"">Work Instructions</button>
                                    <button id=""artwork"" class=""btn btn-primary"">Artwork</button>
                                    <button id=""dci"" class=""btn btn-primary"">DCI</button>
                                    <button id=""ng"" class=""btn btn-primary"">NG Illustration</button>
                                    <button id=""qhc"" class=""btn btn-primary"">Quality History Card</button>
                                    <button id=""generalWI"" class=""btn btn-primary"">General Work Instructions</button>
                                </div>
                                <div>
                                    <button id=""machineLotRequest"" 
                                            class=""btn btn-primary"" 
                                            onclick=""machineLotRequest()"" >
                                        Machine Lot Request
                                    </button>
                                </div>
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
                    
                    ////////////////////////////// MAIN FUNCTION ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    
                    (async () => {
                        disableElements();
                        await getMesName();

                        const btn = document.getElementById(""cmdSave"");
                        if (btn) {
                            btn.onclick = null; // clears any JS-bound handler
                            btn.removeAttribute(""onclick""); // removes inline one too
                        }
                        
                        // Keep original postback if needed
                        const originalDoPostBack = window.__doPostBack;

                        // Temporarily disable postback during script setup
                        window.__doPostBack = function() {
                            console.log(""Postback blocked intentionally by custom script."");
                            return false;
                        };


                        //Attach events to ALL new buttons
                        document.getElementById(""cmdSave"").addEventListener(""click"", async function (e) {
                            e.preventDefault(); // stop normal submission first

                            try {
                                let spvString = document.getElementsByName(""txtSupervisor$ctl00"")[0].value;
                                if (!spvString) {
                                    await Swal.fire({
                                        title: ""Error 😢"",
                                        text: ""Please input supervisor name."",
                                        icon: ""error"",
                                        confirmButtonText: ""OK""
                                    });
                                    return;
                                }

                                const result = await insertInspectionData();

                                await Swal.fire({
                                    title: ""Inspection Saved! 🎉"",
                                    text: ""The inspection data has been successfully inserted."",
                                    icon: ""success"",
                                    timer: 1000,
                                    showConfirmButton: false
                                });

                                console.log(""result"", result);

                                // ✅ restore postback before submission
                                if (originalDoPostBack) window.__doPostBack = originalDoPostBack;

                                // ✅ now trigger postback manually (ASP.NET-compatible)
                                __doPostBack('cmdSave', '');

                            } catch (error) {
                                console.error(""Insert failed:"", error);
                                Swal.fire({
                                    title: ""Error 😢"",
                                    text: ""Failed to insert inspection data. Please try again."",
                                    icon: ""error"",
                                    confirmButtonText: ""OK""
                                });
                            }
                        });
                        




                        items = await getAllItems();
                        console.log(""Got items:"", items);

                        // Map your button IDs to category keys
                        const categoryMap = {
                            threeD: ""threeD"",
                            twoD: ""twoD"",
                            wi: ""wi"",
                            artwork: ""artwork"",
                            dci: ""dci"",
                            ng: ""ng"",
                            qhc: ""qhc"",
                            generalWI: ""generalWI""
                        };

                        Object.entries(categoryMap).forEach(([btnId, category]) => {
                            let btn = document.getElementById(btnId);
                            if (!items[category] || !items[category][category] || items[category][category].length === 0) {
                                btn.setAttribute(""disabled"", ""true"");
                                btn.classList.add(""btn-secondary"");  // make it grey
                                btn.classList.remove(""btn-primary""); // remove active color
                            }
                        });

                        startTimer();

                    })();

                    //////////////////// FUNCTIONS ///////////////////////////////////////////////////////////////////////////////

                    function generateItems(category) {
                    console.log('trial');
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
                        let pdfUrl = `http://apbiphiqcwb01:8080/iqcv2/resources/open_tool/${category}/${fileName}`;
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
                    document.getElementById('generalWI').addEventListener('click', () => generateItems('generalWI'));

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
                            const elapsedMinutes = Math.floor((Date.now() - startTime) / 60000); // whole minutes
                            const txt = document.getElementById(""txtCheckTime"");
                            if (txt) txt.value = elapsedMinutes; // update with whole minutes
                        }, 1000);
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    function disableElements(){
                        document.getElementById(""cmdSubmit"").disabled = true;
                        document.getElementById(""txtCheckUser"").readOnly = true;
                    }

                    async function getInspectionData() {
                        try {
                            const response = await fetch('http://apbiphiqcwb01/IQCSystemAPI/API/InspectionDetails', {
                                method: 'GET',
                                headers: {
                                    'Content-Type': 'application/json'
                                }
                            });

                            if (!response.ok) {
                                console.error(""HTTP error:"", response.status);
                                return;
                            }

                            const data = await response.json(); // parse JSON response
                            console.log(""Received data:"", data);

                        } catch (err) {
                            console.error(""Fetch error:"", err);
                        }
                    }

                    async function insertInspectionData(){
                        let data = {
                            ""checkLot"": document.getElementById(""txtIqcLotQuery"").value,
                            ""dimenstionsMaxSamplingCheckQty"": document.getElementById(""txtMaxNumEdit"").value,
                            ""continuedEligibility"": document.getElementById(""txtQualifiedLotNo"").value,
                            ""relatedCheckLot"": document.getElementById(""txtUnitIQCLot"").value,

                            ""stockInCollectDate"": document.getElementById(""txtInDateQuery"").value,
                            ""partCode"": document.getElementById(""txtMCodeQuery"").value,
                            ""samplingCheckQty"": document.getElementById(""txtCheckQty"").value,

                            ""factoryCode"": document.getElementById(""txtFacCodeQuery"").value,
                            ""partName"": document.getElementById(""txtMNameQuery"").value,
                            ""allowQty"": document.getElementById(""txtAllowQty"").value,    

                            ""standard"": document.getElementById(""txtStandard"").value,
                            ""totalLotQty"": document.getElementById(""txtLotinQty"").value,
                            ""samplingRejectQty"": document.getElementById(""txtRejectSize"").value,

                            ""iqcCheckDate"": document.getElementById(""txtIQCCheckDate_GuruDate"").value,
                            ""classOne"": document.getElementById(""ddlFirstClass"").value,
                            ""samplingCheckDefectiveQty"": document.getElementById(""txtNgQty"").value,
                            ""lotJudge"": document.getElementById(""ddlLotResult"").value,
                            ""occuredEngineer"": document.getElementById(""ddlOccEng"").value,
                            ""checkMonitor"": document.getElementById(""ddlMonitorCheckEdit"").value,

                            ""lotNo"": document.getElementById(""txtLotNo"").value,
                            ""classTwo"": document.getElementById(""ddlSecondClass"").value,
                            ""rejectQty"": document.getElementById(""txtRejectQty"").value,
                            ""processMethod"": document.getElementById(""txtTreatmentMeas"").value,
                            ""checkUser"": document.getElementById(""txtCheckUser"").value,

                            ""proficienceLevel"": document.getElementById(""ddlProLevel"").value,
                            ""firstSize"": document.getElementById(""txtFirstSize"").value,
                            ""secondSize"": document.getElementById(""txtSecondSize"").value,
                            ""supervisor"": document.getElementsByName(""txtSupervisor$ctl00"")[0].value,
                            ""modelNo"": document.getElementById(""txtModelNo"").value,
                            ""designNoticeNo"": document.getElementById(""txtDESIGNNOTICE"").value,

                            ""firstAppearance"": document.getElementById(""txtFirstAppear"").value,
                            ""secondAppearance"": document.getElementById(""txtSecondAppear"").value,
                            ""actualCheckTime"": document.getElementById(""txtCheckTime"").value,
                            ""fourMNumber"": document.getElementById(""txtPowerofatt"").value,
                            ""remarks"": document.getElementById(""txtMemoEdit"").value,

                            ""outgoingInspectionReport"": document.getElementById(""ddlIndustry"").value,
                            ""threeCDataConfirm"": document.getElementById(""ddlValidity"").value,

                            
                            ""createdBy"": '" + this.userID+ @"',

                            ""visualCheckItems"": document.getElementById(""gridWebGridDiv"").outerHTML,
                            ""dimensionCheckItems"": document.getElementById(""gridWebGrid1Div"").outerHTML
                        };

                    await fetch('http://apbiphiqcwb01:1116/API/InspectionDetails', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify(data)
                        }).catch(err => alert(err));
                    }


                    function getCheckItems(){
                        let outer = document.getElementById(""checkResultBodyTable"").outerHTML;
                        return outer;

                    }

                   async function getMesName() {
                        try {
                            const employeeNumber = JSON.parse(localStorage.getItem(""user"")).EmpNo;

                            const res = await fetch(`http://apbiphiqcwb01:1116/API/SystemApproverLists/MesName/${employeeNumber}`, {
                                method: 'GET',
                                headers: { 'Content-Type': 'application/json' }
                            });

                            const mesName = await res.text(); // plain string response

                            if (!mesName) {
                                // empty response → just clear things out
                                localStorage.setItem(""mesName"", """");
                                document.getElementById(""txtCheckUser"").value = """";
                                return;
                            }

                            localStorage.setItem(""mesName"", mesName);

                            // only parse if there's actually something
                            document.getElementById(""txtCheckUser"").value = JSON.parse(mesName).mesName;

                        } catch (err) {
                            console.error(""Error fetching MesName:"", err);
                            alert(err);
                        }
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
                }else if (action["actionName"].ToString() == "machineLotRequest")
                {
                    string partCode = action["data"]["partCode"].ToString();
                    string checkLot = action["data"]["checkLot"].ToString();
                    MachineLotRequest machineLotRequest = new MachineLotRequest(partCode, checkLot);

                    //Check if the form is already open, if not, open it
                    if (Application.OpenForms["MachineLotRequest"] == null)
                    {
                        machineLotRequest.Show();
                    }
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

        private void Inspection_Load(object sender, EventArgs e)
        {

        }
    }
}