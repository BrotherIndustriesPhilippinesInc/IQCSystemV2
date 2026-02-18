using IQCSystemV2.Functions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http.Json;

namespace IQCSystemV2.Forms
{


    public partial class MachineLotRequestAutomation : Form
    {
        private WebViewFunctions webViewFunctions;
        private string username = "ZZPDE31G";
        private string password = "ZZPDE31G";
        private MachineLotRequestData machineLotRequestData;
        private HttpClient httpClient = new HttpClient();
        private WebViewFunctions mainWebViewFunctions;
        private MachineLotRequest machineLotRequest;

        private int loadCount = 0;

        public MachineLotRequestAutomation(MachineLotRequestData machineLotRequestData, WebViewFunctions mainWebViewFunctions, MachineLotRequest machineLotRequest)
        {
            InitializeComponent();
            webViewFunctions = new WebViewFunctions(webView21);
            this.mainWebViewFunctions = mainWebViewFunctions;
            this.machineLotRequestData = machineLotRequestData;
            this.httpClient = new HttpClient();
            this.machineLotRequest = machineLotRequest;

            Uri emes_link = new Uri("http://" + username + ":" + password + "@10.248.1.10/BIPHMES/FLoginNew.aspx");
            webView21.Source = emes_link;
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            Uri summonsRelease = new Uri($@"http://" + username + ":" + password + $"@10.248.1.10/BIPHMES/BenQGuru.eMES.Web.RM/FRMSummonsReleaseMP.aspx");
            await Login(username, password, summonsRelease);
        }

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
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
                await InputDetails();
                await webViewFunctions.ClickButtonAsync("id", "cmdAdd");
                await webViewFunctions.WaitForElementToExistAsync("#gridWebGrid", async () => {
                    var releaseNo = await webViewFunctions.ExecuteJavascript("document.querySelector('#gridWebGrid tbody tr td:nth-child(2)').innerText;", true);
                    //Remove "
                    releaseNo = releaseNo.ToString().Replace("\"", "");

                    var postData = new 
                    {
                        checkLot = machineLotRequestData.checkLot, // Maybe from a login cookie?
                        releaseNo = releaseNo
                    };
                    Console.WriteLine(postData);
                    string result = await PostRawJsonAsync("http://apbiphiqcwb01:1116/api/MachineLotRequests/assignReleaseNo", postData);
                    //string result = await PostRawJsonAsync("https://localhost:7246/api/MachineLotRequests/assignReleaseNo", postData);
                    Console.WriteLine(result);

                    //JObject
                    JObject data = new JObject()
                {
                    { "actionName", "machineLotRequestDone" },
                    { "data", "" }
                };
                    mainWebViewFunctions.SendDataToWeb(data, "machineLotRequestDone");
                    // Use a proper delay on the UI thread instead of Task.Run
                    await Task.Delay(2000);

                    // Close the other form first (if it's not this one)
                    machineLotRequest?.Close();

                    // Finally, close this form
                    this.Close();
                });
            }
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

        private async Task InputDetails()
        {
            string releaseReasonCode = machineLotRequestData.releaseReasonCode.ToString();
            string releaseReasonRemarks = machineLotRequestData.remarks.ToString();
            string partCode = machineLotRequestData.partCode.ToString();
            string vendorName = machineLotRequestData.vendorName.ToString();
            string quantity = machineLotRequestData.quantity.ToString();
            string dciOtherNo = machineLotRequestData.dciOtherNo.ToString();
            bool yellowCard = machineLotRequestData.yellowCard;

            await webViewFunctions.SetTextBoxValueAsync("name", "txtMaterialCodeEdit$ctl00", partCode);
            await webViewFunctions.TriggerLostFocusAsync(".shortrequire");


            await webViewFunctions.SetTextBoxValueAsync("name", "txtDesginNoEdit", dciOtherNo);
            await webViewFunctions.SetTextAreaValueAsync("name", "txtReleaseContentEdit", releaseReasonRemarks);

            await webViewFunctions.SetTextBoxValueAsync("name", "txtMaterialQtyEdit", quantity);

            await webViewFunctions.SelectElement("id", "drpReleaseReasonEdit", releaseReasonCode);

            await webViewFunctions.SelectByTextAsync("#drpVendorNameEdit", vendorName);

            await webViewFunctions.SetCheckboxStateAsync("#chbIsYellowCardEdit", yellowCard);

        }

        
        private async Task<string> GetRawJsonAsync(string endpoint)
        {
            try
            {
                // Note: No "using" block needed here if you just want the string content
                var response = await httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
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
    }

    public class MachineLotRequestData
    {
        public string id { get; set; }

        public string checkLot { get; set; }
        public string createdBy { get; set; }
        public string partCode { get; set; }
        public string partName { get; set; }
        public string vendorName { get; set; }
        public int quantity { get; set; }
        public bool yellowCard { get; set; }
        public string dciOtherNo { get; set; }
        public string releaseReasonName { get; set; }
        public string releaseReasonCode { get; set; }
        public string whatForName { get; set; }
        public string whatForCode { get; set; }
        public string remarks { get; set; }

    }

    public class MachineLotRequestDataPost
    {
        public string id { get; set; }

        public string checkLot { get; set; }
        public string createdBy { get; set; }
        public string partCode { get; set; }
        public string partName { get; set; }
        public string vendorName { get; set; }
        public int quantity { get; set; }
        public string releaseNo { get; set; }
        public bool yellowCard { get; set; }
        public string dciOtherNo { get; set; }
        public string releaseReasonId { get; set; }
        public string whatForId { get; set; }
        public string remarks { get; set; }}

    }


    public class MachineLotRequestGetDTO
    {
        public int Id { get; set; }

        public string CheckLot { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string VendorName { get; set; }
        public int Quantity { get; set; }
        public string ReleaseNo { get; set; }
        public bool YellowCard { get; set; }
        public string DCIOtherNo { get; set; }
        public string Remarks { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        // Flattened or nested DTOs
        public string WhatForName { get; set; }
        public string ReleaseReasonName { get; set; }
    }

