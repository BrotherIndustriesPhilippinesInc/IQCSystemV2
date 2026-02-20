using ClosedXML.Excel;
using IQCSystemV2.Functions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        private readonly string userId;

        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";

        private List<string> releaseNos = new List<string>();

        public MachineLotRequestExport(WebViewFunctions mainWebViewFunctions, string userId)
        {
            InitializeComponent();

            this.mainWebViewFunctions = mainWebViewFunctions;
            webViewFunctions = new WebViewFunctions(webView21);
            httpClient = new HttpClient();
            this.userId = userId;

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;
            this.userId = userId;
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            Uri summonsRelease = new Uri($@"http://" + username + ":" + password + $"@10.248.1.10/BIPHMES/BenQGuru.eMES.Web.RM/FRMSummonsReleaseMP.aspx");
            await Login(username, password, summonsRelease);

            webView21.CoreWebView2.NewWindowRequested += webView21_NewWindowRequested;
            webView21.CoreWebView2.DownloadStarting += webView21_DownloadStarting;

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


            if (message == "EXPORT_CLICKED")
            {
                // 1. Do any C# work you need here (Logging, UI updates, etc.)
                // Example: Update a status label or log the timestamp
                Debug.WriteLine("Export initiated by user at " + DateTime.Now);

                this.releaseNos.Clear();
                this.releaseNos = await GetAllSelectedReleaseNos();

                if (releaseNos.Count > 0)
                {
                    Console.WriteLine($"Found {releaseNos.Count} selected items:");
                }
                else
                {
                    Console.WriteLine("No checkboxes were checked.");
                }

                //await PostRawJsonAsync();

                // 2. Resume the WebForms Submit
                // This calls the JS function we defined in InjectExportInterceptor
                await webViewFunctions.ExecuteJavascript("window.finalizeExport();");
            }
        }

        private async void webView21_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            // A deferral is good practice when handling window events
            var deferral = e.GetDeferral();

            // "Handled = true" tells WebView2: "Don't open that new window, I got this."
            e.Handled = true;

            // NOW, force the download to happen in OUR window.
            // Since it ends in .xlsx, WebView2 will detect it's a file and fire DownloadStarting
            // instead of navigating the page away.
            webView21.CoreWebView2.Navigate(e.Uri);

            deferral.Complete();
        }

        private void webView21_DownloadStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DownloadStartingEventArgs e)
        {
            // This fires for BOTH normal downloads AND the pop-up we just redirected
            var download = e.DownloadOperation;

            // Optional: Hide the default browser download dialog
            e.Handled = true;

            // Subscribe to progress/completion
            download.StateChanged += (s, args) =>
            {
                // Must use Invoke because this runs on a background thread
                this.Invoke(new Action(async () =>
                {
                    if (download.State == Microsoft.Web.WebView2.Core.CoreWebView2DownloadState.Completed)
                    {
                        //MessageBox.Show($"Captured Download!\nPath: {download.ResultFilePath}", "Success");
                        MessageBox.Show($"Loading, please wait for \"OK Window\"", "Success");
                        await BuildYellowCard(download.ResultFilePath.ToString());

                    }
                    else if (download.State == Microsoft.Web.WebView2.Core.CoreWebView2DownloadState.Interrupted)
                    {
                        Debug.WriteLine("Download failed: " + download.InterruptReason);
                    }
                }));
            };
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
        private async Task<List<string>> GetAllSelectedReleaseNos()
        {
            // JavaScript returns the array directly. 
            // Do NOT use JSON.stringify() inside the JS if you want to keep it simple, 
            // but even if you do, the logic below handles it.
            string jsScript = @"
                (function() {
                    var checkboxes = document.querySelectorAll('#gridWebGrid input[type=""checkbox""]:checked');
                    var results = [];
                    for (var i = 0; i < checkboxes.length; i++) {
                        var row = checkboxes[i].closest('tr');
                        if (row && row.cells.length > 1) {
                            results.push(row.cells[1].innerText.trim());
                        }
                    }
                    return results; 
                })();
            ";

            try
            {
                string result = await webView21.CoreWebView2.ExecuteScriptAsync(jsScript);

                if (string.IsNullOrEmpty(result) || result == "null")
                    return new List<string>();

                // === THE HYBRID FIX ===

                // Scenario A: Result is ["PQC-1", "PQC-2"] (Your current case)
                if (result.Trim().StartsWith("["))
                {
                    return JsonConvert.DeserializeObject<List<string>>(result) ?? new List<string>();
                }

                // Scenario B: Result is "[\"PQC-1\", \"PQC-2\"]" (Double encoded)
                else
                {
                    // Unwrap the outer string first
                    string cleanJson = JsonConvert.DeserializeObject<string>(result);
                    return JsonConvert.DeserializeObject<List<string>>(cleanJson) ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing release numbers: {ex.Message}");
                return new List<string>();
            }
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

        public class YellowCardDto
        {
            public string PartCode { get; set; }
            public string PartName { get; set; }     // Example
            public string VendorName { get; set; }      // Example
            public int Quantity { get; set; }      // Example
            public string ReleaseNo { get; set; } // Example
            public string DCIOtherNo { get; set; }
            public string Remarks { get; set; }
            public string LotNumber { get; set; }

            public string ModelCode { get; set; }

            public DateTime? DeliveryDate { get; set; }
        }



        private async Task BuildYellowCard(string targetFilePath)
        {
            // 1. FETCH DATA
            var cardDataList = new List<YellowCardDto>();

            // note: assuming 'releaseNos' and 'userId' are class-level variables since they weren't passed in
            foreach (string releaseNo in releaseNos)
            {
                // ... (Keep your existing API call logic here) ...
                try
                {
                    string jsonResult = await PostRawJsonAsync($@"http://apbiphiqcwb01:1116/api/MachineLotRequests/MachineLotRequestExportation?releaseNo={releaseNo}&exportedBy={userId}", new { });
                    var data = JsonConvert.DeserializeObject<YellowCardDto>(jsonResult);

                    if (data != null)
                        cardDataList.Add(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing JSON for {releaseNo}: {ex.Message}");
                }
            }

            if (cardDataList.Count == 0)
            {
                MessageBox.Show("No data retrieved from API.", "Warning");
                return;
            }

            // 2. PREPARE EXCEL
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templatePath = Path.Combine(baseDir, "Resource", "YellowCardTemplate", "YELLOW CARD.xlsx");

            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Could not find template at:\n{templatePath}", "File Missing");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    // Open the template in memory
                    using (var sourceWorkbook = new XLWorkbook(templatePath))
                    {
                        // We assume the template is on the first sheet
                        var templateSheet = sourceWorkbook.Worksheet(1);

                        // LOOP: One Sheet Per Item
                        for (int i = 0; i < cardDataList.Count; i++)
                        {
                            var item = cardDataList[i];
                            string sheetName = $"YC-{i + 1}";

                            // 1. Clone the template
                            var newSheet = templateSheet.CopyTo(sheetName);

                            // 2. Fill ALL 6 slots on this specific sheet with the SAME item
                            for (int pos = 0; pos < 6; pos++)
                            {
                                FillYellowCard(newSheet, pos, item);
                            }
                        }

                        // 3. Delete the original empty template sheet 
                        // so it doesn't appear in the final output.
                        templateSheet.Delete();

                        // 4. MERGE INTO TARGET FILE
                        // Open the file where you want to save these sheets
                        using (var targetWorkbook = new XLWorkbook(targetFilePath))
                        {
                            foreach (var sheet in sourceWorkbook.Worksheets)
                            {
                                // Copy the filled sheets over
                                sheet.CopyTo(targetWorkbook, sheet.Name);
                            }
                            targetWorkbook.Save();
                        }
                    }

                    this.Invoke(new Action(() => {
                        //MessageBox.Show($"Process Complete!\n{cardDataList.Count} Yellow Cards added to:\n{targetFilePath}", "Success");
                        MessageBox.Show($"OK Window, you can now close this window!", "Success");
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => {
                        MessageBox.Show($"Error merging files: {ex.Message}", "Error");
                    }));
                }
            });
        }
        private void FillYellowCard(IXLWorksheet sheet, int position, YellowCardDto data)
        {
            // --- 1. DEFINE ANCHOR POINTS ---
            // "MODEL CODE" Value is at C5 (Row 5, Col 3)
            int rowBase = 5;
            int colBase = 3;

            // --- 2. CALCULATE TARGET OFFSET ---
            int targetRow = 0;
            int targetCol = 0;

            // Determine Row (Top, Middle, Bottom)
            // 0,1 -> Top (Index 0)
            // 2,3 -> Mid (Index 1)
            // 4,5 -> Bot (Index 2)
            int rowIndex = position / 2;
            targetRow = rowBase + (rowIndex * 16); // 16 rows vertical spacing

            // Determine Column (Left, Right)
            // 0,2,4 -> Left (Index 0)
            // 1,3,5 -> Right (Index 1)
            int colIndex = position % 2;

            // CORRECTION: The horizontal gap is 9 columns (C to L), not 8.
            targetCol = colBase + (colIndex * 9);

            // --- 3. WRITE DATA ---

            // 0. Model Code (Base) -> C5
            sheet.Cell(targetRow, targetCol).Value = data.ModelCode;

            // 1. Part Name (Base) -> C7
            sheet.Cell(targetRow + 2, targetCol).Value = data.PartName;

            // 2. Part Code (2 rows down) -> C9
            sheet.Cell(targetRow + 4, targetCol).Value = data.PartCode;

            // 3. Lot No / Release No (4 rows down) -> D11
            // (You had +1 for Column D)
            sheet.Cell(targetRow + 6, targetCol).Value = data.LotNumber;

            // 4. Lot Size / Quantity (6 rows down) -> C13
            // CORRECTION: Changed from +5 to +6 to hit Row 13
            sheet.Cell(targetRow + 8, targetCol).Value = data.Quantity;

            // 5. DCI Change / Design Change No (2 rows UP) -> G5
            // CORRECTION: Base is Row 7, so 7 - 2 = 5.
            // Column G is 4 columns right of C (3 + 4 = 7)
            sheet.Cell(targetRow, targetCol + 4).Value = data.DCIOtherNo;

            // 6. Supplier / Vendor (7 rows down) -> D14
            sheet.Cell(targetRow + 8, targetCol + 1).Value = data.VendorName;

            // 7. Remarks (7 rows down) -> G14
            sheet.Cell(targetRow + 2, targetCol + 3).Value = data.Remarks;

            // 8. Release No () -> Q3
            sheet.Cell(targetRow - 2, targetCol + 5).Value = data.ReleaseNo;

            // 9. Release Date () -> C31
            //Convert to DateTime to yy/mm/dd
            string formattedDate = DateTimeOffset.Parse(data.DeliveryDate.ToString()).ToString("yy/MM/dd");
            sheet.Cell(targetRow + 10, targetCol).Value = formattedDate;
        }

    }
}
