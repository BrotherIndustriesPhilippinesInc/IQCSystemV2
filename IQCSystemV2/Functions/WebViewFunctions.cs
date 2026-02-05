using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Functions
{
    public class WebViewFunctions
    {
        private WebView2 _webView;
        private TaskCompletionSource<bool> navigationCompletionSource;
        public event EventHandler<CoreWebView2NavigationStartingEventArgs> NavigationStarting;
        public event EventHandler<CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;
        private readonly List<EventHandler<CoreWebView2NavigationStartingEventArgs>> _navigationStartingHandlers = new List<EventHandler<CoreWebView2NavigationStartingEventArgs>>();

        private event EventHandler<CoreWebView2DownloadStartingEventArgs> DownloadStarting;

        public event EventHandler<CoreWebView2ScriptDialogOpeningEventArgs> ScriptDialogOpening;
        public WebViewFunctions(WebView2 _webView, bool CustomEnvironment = false)
        {
            this._webView = _webView;

            if (CustomEnvironment)
            {
                // Explicitly await the initialization
                _ = InitializeAsync(true); // Fire-and-forget for now, or handle it with an async factory pattern
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _webView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during WebView2 initialization: {ex.Message}");
                MessageBox.Show($"Error during WebView2 initialization: {ex.Message}");
            }
        }

        public async Task InitializeAsync(bool CustomEnvironment = false)
        {
            try
            {
                if (CustomEnvironment)
                {
                    // Define the custom cookies directory
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string cookiesPath = Path.Combine(documentsPath, "WebView2Cookies");

                    // Ensure the directory exists
                    if (!Directory.Exists(cookiesPath))
                    {
                        Directory.CreateDirectory(cookiesPath);
                        Console.WriteLine("Directory created.");
                    }
                    else
                    {
                        Console.WriteLine("Directory already exists. Proceeding...");
                    }

                    // Create the WebView2 environment
                    var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: cookiesPath);

                    // Initialize WebView2 with the environment
                    await _webView.EnsureCoreWebView2Async(environment);
                    Console.WriteLine($"WebView2 initialized with User Data Folder: {cookiesPath}");
                }
                else
                {
                    await _webView.EnsureCoreWebView2Async(null);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during WebView2 initialization: {ex.Message}");
                MessageBox.Show($"Error during WebView2 initialization: {ex.Message}");
            }
        }

        public async Task InitializeInMemoryAsync()
        {
            try
            {
                // Create an in-memory user data folder
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: null);

                await _webView.EnsureCoreWebView2Async(environment);

                Console.WriteLine("WebView2 initialized with an in-memory user data folder.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during WebView2 initialization: {ex.Message}");
            }
        }

        public async Task LoadPageAsync(string url, string element = "", int timeout = 5000, bool AreDefaultScriptDialogsEnabled = true)
        {
            await _webView.EnsureCoreWebView2Async(null);
            //DISABLE ALL ALERTS
            _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = AreDefaultScriptDialogsEnabled;

            navigationCompletionSource = new TaskCompletionSource<bool>();
            _webView.CoreWebView2.Navigate(url);

            bool isLoaded = false;
            if (element != "")
            {
                isLoaded = await WaitForElementToExistAsync(element, timeout);
            }


            if (isLoaded == false)
            {
                Debug.WriteLine(url + " Not Loaded");
            }
            await Task.Delay(100);
        }

        public async Task SetTextBoxValueAsync(string searchBy, string name, string inputText, int classIndex = 0, int timeoutMilliseconds = 5000)
        {
            searchBy = searchBy.ToLower();
            string script;
            int elapsedTime = 0;
            const int delayInterval = 100;

            while (elapsedTime < timeoutMilliseconds)
            {
                switch (searchBy)
                {
                    case "id":
                        script = $"document.getElementById('{name}').value = `{inputText}`;";
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                        await Task.Delay(100);
                        break;

                    case "class":
                        script = $"document.getElementsByClassName('{name}')[{classIndex}].value = `{inputText}`;";
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                        await Task.Delay(100);
                        break;

                    case "name":
                        script = $"document.querySelector(\"[name='{name}']\").value = `{inputText}`;";
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                        await Task.Delay(100);
                        break;

                    default:
                        throw new ArgumentException($"Invalid searchBy parameter: {searchBy}");
                }

                // Check if the value has been set successfully
                if (await WaitForContentToExistAsync(searchBy, name, inputText))
                {
                    return; // Exit the method if the value is set
                }

                // Wait and increment elapsed time
                await Task.Delay(delayInterval);
                elapsedTime += delayInterval;
            }

            // Throw a timeout exception if the value couldn't be set within the time limit
            throw new TimeoutException($"Setting the value of the element '{name}' by '{searchBy}' timed out after {timeoutMilliseconds} milliseconds.");
        }

        public async Task SetTextAreaValueAsync(string searchBy, string name, string inputText, int classIndex = 0, int timeoutMilliseconds = 5000)
        {
            searchBy = searchBy.ToLower();
            string script;
            int elapsedTime = 0;
            const int delayInterval = 100;

            while (elapsedTime < timeoutMilliseconds)
            {
                switch (searchBy)
                {
                    case "id":
                        script = $"document.getElementById('{name}').value = `{inputText}`;";
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                        await Task.Delay(100);
                        break;

                    case "class":
                        script = $"document.getElementsByClassName('{name}')[{classIndex}].value = `{inputText}`;";
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                        await Task.Delay(100);
                        break;

                    case "name":
                        script = $"document.querySelector(\"[name='{name}']\").value = `{inputText}`;";
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                        await Task.Delay(100);
                        break;
                }
                // Check if the value has been set successfully
                if (await WaitForContentToExistAsync(searchBy, name, inputText))
                {
                    return; // Exit the method if the value is set
                }

                // Wait and increment elapsed time
                await Task.Delay(delayInterval);
                elapsedTime += delayInterval;
            }
            throw new TimeoutException($"Setting the value of the element '{name}' by '{searchBy}' timed out after {timeoutMilliseconds} milliseconds.");
        }

        public async Task<string> GetTextBoxValue(string searchBy, string name)
        {
            searchBy = searchBy.ToLower();
            string script;
            string result;

            switch (searchBy)
            {
                case "id":
                    script = $"document.getElementById(\"{name}\").value;";
                    result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    result = result.Trim('"');
                    result = System.Text.RegularExpressions.Regex.Unescape(result);
                    return result;

                case "class":
                    script = $"document.getElementsByClassName(\"{name}\").value";
                    result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    result = result.Trim('"');
                    result = System.Text.RegularExpressions.Regex.Unescape(result);
                    return result;

                case "name":
                    script = $"document.querySelector(\"[name='{name}']\").value;";
                    result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    result = result.Trim('"');
                    result = System.Text.RegularExpressions.Regex.Unescape(result);
                    return result;
            }
            return "Get TextBox Value ERROR";
        }

        public async Task SelectElement(string searchBy, string name, string valueName, int classIndex = 0)
        {
            searchBy = searchBy.ToLower();
            string script;
            switch (searchBy)
            {
                case "id":
                    script = $"document.getElementById('{name}').value = '{valueName}';";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "class":
                    script = $"document.getElementsByClassName('{name}')[{classIndex}].value = '{valueName}';";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "name":
                    script = $"document.querySelector(\"[name='{name}']\").value = '{valueName}';";

                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;
            }
        }

        public async Task ClickButtonAsync(string searchBy, string name, int index = 0)
        {
            searchBy = searchBy.ToLower();
            string script;

            switch (searchBy)
            {
                case "id":
                    script = $"document.getElementById('{name}').click();";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "class":
                    script = $"document.getElementsByClassName('{name}')[{index}].click();";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "name":
                    script = $"document.querySelector(\"[name='{name}']\").click();";

                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "content":
                    script = $@"
                    var elements = document.querySelectorAll('*');
                    for (var i = 0; i < elements.length; i++) {{
                        if (elements[i].textContent.trim() === '{name}') {{
                            elements[i].click();
                            break;
                        }}
                    }}";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;
            }
        }

        public void AddNavigationStartingHandler(EventHandler<CoreWebView2NavigationStartingEventArgs> handler)
        {
            // Ensure we're adding to CoreWebView2's NavigationStarting event
            _webView.CoreWebView2.NavigationStarting += handler;
            NavigationStarting += handler;
            Debug.WriteLine("Starting handler added");
        }

        public void RemoveNavigationStartingHandler(EventHandler<CoreWebView2NavigationStartingEventArgs> handler)
        {
            if (_webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.NavigationStarting -= handler;
                NavigationStarting -= handler;
                Debug.WriteLine("Starting handler removed");
            }
        }

        public void AddNavigationCompletedHandler(EventHandler<CoreWebView2NavigationCompletedEventArgs> handler)
        {
            _webView.CoreWebView2.NavigationCompleted += handler;
            _webView.NavigationCompleted += handler;
            Debug.WriteLine("Completed handler added");
        }

        public void RemoveNavigationCompletedHandler(EventHandler<CoreWebView2NavigationCompletedEventArgs> handler)
        {
            if (_webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.NavigationCompleted -= handler;
                NavigationCompleted -= handler;
                Debug.WriteLine("Completed handler removed");
            }
        }

        public bool HasNavigationStartingHandlers()
        {
            return _navigationStartingHandlers.Count > 0; // Check if there are any attached handlers
        }

        // Example method to raise the event
        protected virtual void OnNavigationStarting(CoreWebView2NavigationStartingEventArgs e)
        {
            NavigationStarting?.Invoke(this, e);
        }

        public async Task<bool> WaitForElementToExistAsync(string cssSelector, Action callback, int timeoutMilliseconds = 5000, int maxRetries = 50)
        {
            try
            {
                var startTime = DateTime.Now;
                var retryCount = 0;
                var scriptCheckElement = $@"
                    (function waitElement() {{
                        var element = document.querySelector(""{cssSelector}"");
                        return element !== null;
                    }})();
                ";

                while (retryCount < maxRetries)
                {


                    // Execute the JavaScript to check for the element
                    var result = await _webView.ExecuteScriptAsync(scriptCheckElement);
                    await Task.Delay(100);

                    // If the element is found
                    if (result == "true")
                    {
                        callback?.Invoke(); // Invoke the callback if provided
                        return true; // Exit successfully
                    }

                    // Check for timeout
                    if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                    {
                        Debug.WriteLine($"Timeout reached while waiting for element '{cssSelector}' to exist.");
                        return false; // Exit if timeout is reached
                    }

                    // Increment the retry counter
                    retryCount++;

                    // Wait for a short period before the next retry
                    await Task.Delay(100); // Adjust delay as needed
                }

                // Log if max retries are reached without success
                Debug.WriteLine($"Max retries reached ({maxRetries}) for element '{cssSelector}'.");
                return false; // Exit if retries are exhausted
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in WaitForElementToExistAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> WaitForElementToExistAsync(string cssSelector, int timeoutMilliseconds = 5000, int maxRetries = 50)
        {
            var startTime = DateTime.Now;
            var retryCount = 0;
            var script = $@"
                (function waitElement() {{
                    var element = document.querySelector(""{cssSelector}"");
                    return element !== null;
                }})();
            ";

            while (retryCount < maxRetries)
            {
                var result = await _webView.ExecuteScriptAsync(script);
                await Task.Delay(100);

                if (result == "true") // The element exists
                {
                    return true; // Return true if the element exists
                }

                // Check for timeout
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                {
                    Debug.WriteLine("Waiting for element timed out");
                    return false; // Return false if the element does not exist within the timeout
                }

                // Increment the retry count
                retryCount++;

                // Wait for a short period before checking again
                await Task.Delay(100); // Wait 100 milliseconds before retrying
            }

            Debug.WriteLine($@"Maximum retries reached without finding the element. - {cssSelector}");
            return false; // Return false if max retries are exceeded
        }

        // Function to wait until the navigation is completed
        public Task WaitForNavigationCompleted(WebViewFunctions webFunctions)
        {
            navigationCompletionSource = new TaskCompletionSource<bool>();

            webFunctions.AddNavigationCompletedHandler((sender, e) =>
            {
                if (e.IsSuccess)
                {
                    navigationCompletionSource.SetResult(true);
                }
                else
                {
                    navigationCompletionSource.SetResult(false);
                }
            });

            return navigationCompletionSource.Task;
        }

        public async Task DropdownSelectAsync(string searchBy, string name, string optionValue)
        {
            searchBy = searchBy.ToLower();
            string script;

            switch (searchBy)
            {
                case "id":

                    script = $@"
                        var selectElement = document.getElementById(""{name}"");
                        selectElement.value = '{optionValue}';
                        var event = new Event('change');
                        selectElement.dispatchEvent(event);";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "class":
                    script = $@"
                        var selectElement = document.getElementsByClassName(""{name}"");
                        selectElement.value = '{optionValue}';
                        var event = new Event('change');
                        selectElement.dispatchEvent(event);";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "name":
                    script = $@"
                        var selectElement = document.querySelector(""[name='{name}']"");
                        selectElement.value = '{optionValue}';
                        var event = new Event('change');
                        selectElement.dispatchEvent(event);";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;
            }
        }

        public async Task<string> GetElementText(string searchBy, string name, int index = 0)
        {
            searchBy = searchBy.ToLower();
            string script;

            switch (searchBy)
            {
                case "id":
                    script = $@"
                        (function extractContent(){{
                            var element = document.getElementById(""{name}"");
                            var text = element.textContent;
                            return text;
                        }})()
                    ";
                    return await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    ;

                case "class":
                    script = $@"
                        var selectElement = document.getElementsByClassName(""{name}"");
                        const textContent = selectElement ? selectElement.textContent : null;

                        (function extractContent(){{
                            var element = document.getElementsByClassName(""{name}"");
                            var text = element{index}.textContent;
                            return text;
                        }})()
                        ";


                    return await _webView.CoreWebView2.ExecuteScriptAsync(script);

                case "name":
                    script = $@"
                        (function extractContent(){{
                            var element = document.querySelector(""[name='{name}']"");
                            var text = element{index}.textContent;
                            return text;
                        }})()
                    ";
                    return await _webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            return null;
        }

        public async Task<object> ExecuteJavascript(string script, bool withReturn = false)
        {
            try
            {
                if (withReturn)
                {
                    return await _webView.CoreWebView2.ExecuteScriptAsync(script);
                }
                else
                {
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing JavaScript: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
        }

        public async Task CheckBoxAsync(string searchBy, string name)
        {
            searchBy = searchBy.ToLower();
            string script;

            switch (searchBy)
            {
                case "id":
                    script = $@"
                var selectElement = document.getElementById(""{name}"");
                if (selectElement && selectElement.type === 'checkbox') {{
                    selectElement.checked = true;
                }}";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "class":
                    script = $@"
                var selectElements = document.getElementsByClassName(""{name}"");
                if (selectElements.length > 0) {{
                    for (let element of selectElements) {{
                        if (element.type === 'checkbox') {{
                            element.checked = true;
                        }}
                    }}
                }}";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;

                case "name":
                    script = $@"
                var selectElement = document.querySelector(""[name='{name}']"");
                if (selectElement && selectElement.type === 'checkbox') {{
                    selectElement.checked = true;
                }}";
                    await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    await Task.Delay(100);
                    break;
            }
        }

        public async Task<string> FindCheckBoxNameByText(string searchText)
        {
            // JavaScript script to find the checkbox name based on the search text
            string script = $@"
                (function findCheckBoxName() {{
                    const targetCell = Array.from(document.querySelectorAll('td')).find(cell => cell.textContent.trim() === '{searchText}');
                    const parentRow = targetCell ? targetCell.parentElement : null;

                    if (parentRow) {{
                        const checkbox = parentRow.querySelector('input[type=""checkbox""]');
                        return checkbox ? checkbox.name : null;
                    }} else {{
                        return null;
                    }}
                }})();
            ";

            // Execute the JavaScript and get the result
            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
            await Task.Delay(100);
            // Deserialize the result to get the checkbox name (or null if not found)
            try
            {
                return JsonConvert.DeserializeObject<string>(result);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null; // Return null or handle the error as needed
            }
        }

        public JObject CapturedMessage(CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                JObject jsonMessage = JObject.Parse(message);

                //var data = jsonMessage["data"];
                //Console.WriteLine($"{jsonMessage}");
                return jsonMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving WebView2 message: {ex.Message}");
                throw ex;
            }
        }

        public void SendDataToWeb(JObject data, string functionName = "")
        {
            try
            {
                // Convert JObject to string (JSON format)
                string jsonData = data.ToString();

                // Send the JSON string to the web page (JavaScript)
                //MessageBox.Show(data.ToString());

                _webView.CoreWebView2.PostWebMessageAsString(jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SendDataToWeb - {functionName}: " + ex.Message);
                Console.WriteLine($"Error sending data to web: {ex.Message}");
            }
        }

        public async Task SearchToLastNode(string selector, string action)
        {
            string script = $@"
                var parentElement = document.querySelector('{selector}');
                var lastElementInside = parentElement.lastElementChild;
                lastElementInside.{action};
            ";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
            await Task.Delay(100);
        }

        public async Task<bool> WaitForContentToExistAsync(string searchBy, string name, string inputText, int classIndex = 0)
        {
            searchBy = searchBy.ToLower();
            string script;
            bool result = false;

            switch (searchBy)
            {
                case "id":
                    script = $@"
                (function() {{
                    let element = document.getElementById('{name}');
                    if (element) {{
                        element.value = `{inputText}`;
                        return true;
                    }}
                    return false;
                }})();";
                    result = Convert.ToBoolean(await _webView.CoreWebView2.ExecuteScriptAsync(script));
                    break;

                case "class":
                    script = $@"
                (function() {{
                    let elements = document.getElementsByClassName('{name}');
                    if (elements && elements[{classIndex}]) {{
                        elements[{classIndex}].value = `{inputText}`;
                        return true;
                    }}
                    return false;
                }})();";
                    result = Convert.ToBoolean(await _webView.CoreWebView2.ExecuteScriptAsync(script));
                    break;

                case "name":
                    script = $@"
                (function() {{
                    let element = document.querySelector('[name=""{name}""]');
                    if (element) {{
                        element.value = `{inputText}`;
                        return true;
                    }}
                    return false;
                }})();";
                    result = Convert.ToBoolean(await _webView.CoreWebView2.ExecuteScriptAsync(script));
                    break;
            }
            return result;
        }

        public async Task PressKey(string key)
        {
            string script = $@"
                var event = new KeyboardEvent('keydown', {{
                    key: '{key}',
                    code: '{key}',
                    keyCode: {key.GetHashCode()}, // You can customize this if needed
                    bubbles: true
                }});
                document.dispatchEvent(event);
            ";

            await ExecuteJavascript(script);
            await Task.Delay(100);
        }

        public async Task<bool> CheckForAlerts(int timeoutMilliseconds = 5000)
        {
            var startTime = DateTime.Now;
            string script = $@"
            function isAlertOpen() {{
                return window.alert && window.alert.toString().indexOf('native code') !== -1;
            }}
            isAlertOpen();
            ";
            while (true)
            {
                var result = await _webView.ExecuteScriptAsync(script);
                await Task.Delay(100);

                if (result == "true") // The element exists
                {
                    return true; // Return true if the element exists
                }

                // Check for timeout
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                {
                    Debug.WriteLine("Waiting for element timed out");
                    return false; // Return false if the element does not exist within the timeout
                }

                // Wait for a short period before checking again
                await Task.Delay(100); // Wait 100 milliseconds before retrying
            }
        }

        public async Task<bool> IsPageLoadingAsync()
        {
            try
            {
                var script = "document.readyState"; // JavaScript to check page state
                var result = await _webView.ExecuteScriptAsync(script);
                await Task.Delay(100);

                // Trim quotes and check the state
                result = result.Trim('"');
                if (result == "complete")
                {
                    Debug.WriteLine("Page is fully loaded.");
                    return false; // Page is not loading
                }
                else
                {
                    Debug.WriteLine($"Page is still loading. Current state: {result}");
                    return true; // Page is still loading
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in IsPageLoadingAsync: {ex.Message}");
                return true; // Default to true to ensure it waits if an error occurs
            }
        }

        public void AddScriptDialogOpening(EventHandler<CoreWebView2ScriptDialogOpeningEventArgs> handler)
        {
            _webView.CoreWebView2.ScriptDialogOpening += handler;
            ScriptDialogOpening += handler;
            Debug.WriteLine("ScriptDialogOpening handler added");
        }

        public void RemoveScriptDialogOpening(EventHandler<CoreWebView2ScriptDialogOpeningEventArgs> handler = null)
        {
            _webView.CoreWebView2.ScriptDialogOpening -= handler;
            ScriptDialogOpening -= handler;
            Debug.WriteLine("ScriptDialogOpening handler removed");

        }

        //SAMPLE USAGE
        //LoadPageAsync AreDefaultScriptDialogsEnabled must be false
        private void SampleScriptDialogHandling()
        {
            string errorMessage;
            AddScriptDialogOpening((sender, args) =>
            {
                Console.WriteLine($"Dialog message: {args.Message}");
                errorMessage = args.Message;
                RemoveScriptDialogOpening();
            });
        }

        public void AddDownloadStartingHandler(EventHandler<CoreWebView2DownloadStartingEventArgs> handler)
        {
            _webView.CoreWebView2.DownloadStarting += handler;
            DownloadStarting += handler;
            Debug.WriteLine("Download Starting handler added");
        }

        public void RemoveDownloadStartingHandler(EventHandler<CoreWebView2DownloadStartingEventArgs> handler)
        {
            _webView.CoreWebView2.DownloadStarting -= handler;
            DownloadStarting -= handler;
            Debug.WriteLine("Download Starting handler removed");
        }

        public async Task HardRefresh()
        {
            await _webView.ExecuteScriptAsync("window.location.reload(true)");
        }

        public async Task ClearAllBrowsingDataAsync()
        {
            await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                "Network.setCacheDisabled",
                "{\"cacheDisabled\":true}"
            );

            _webView.CoreWebView2.CookieManager.DeleteAllCookies();
            await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
        }

        public void BackFunction()
        {
            _webView.GoBack();
        }

        public void DisposeWebView()
        {
            _webView.CoreWebView2.Stop();
            _webView.Dispose();
        }

        public async Task<bool> HasInputAsync(string cssSelector)
        {
            // We use an IIFE (Immediately Invoked Function Expression) to keep variables isolated
            // We check 3 things:
            // 1. Does the element exist?
            // 2. Is the value not null?
            // 3. Is the trimmed length > 0?
            string script = $@"
                (function() {{
                    var element = document.querySelector('{cssSelector}');
                    if (!element) return false; 
                    return element.value && element.value.trim().length > 0;
                }})();
            ";

            // WebView2 ALWAYS returns the result as a JSON String.
            // If JS returns boolean true, C# gets the string "true".
            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            // Parse the JSON result
            return result == "true";
        }

        public async Task TriggerLostFocusAsync(string selector = null)
        {
            // If no selector provided, we assume we are blurring the CURRENTLY focused element.
            string targetJs = string.IsNullOrEmpty(selector)
                ? "document.activeElement"
                : $"document.querySelector('{selector}')";

            string script = $@"
                (function() {{
                    var element = {targetJs};
                    if (!element) return;

                    // 1. Fire the standard 'blur' event (what most inputs listen to)
                    element.dispatchEvent(new Event('blur', {{ bubbles: false, cancelable: true }}));

                    // 2. Fire 'focusout' (bubbles up - frameworks often listen to this)
                    element.dispatchEvent(new Event('focusout', {{ bubbles: true, cancelable: true }}));

                    // 3. Fire 'change' (CRITICAL: Many validators only run if they think data changed)
                    element.dispatchEvent(new Event('change', {{ bubbles: true, cancelable: true }}));

                    // 4. Actually remove focus natively
                    element.blur();
                }})();
            ";

            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task<bool> SelectByTextAsync(string cssSelector, string textToSelect)
        {
            // We iterate the options, find the match, set it, and fire events.
            // 'trim()' is used to ignore accidental spaces in the HTML.
            string script = $@"
            (function() {{
                var select = document.querySelector('{cssSelector}');
                if (!select) return 'Error: Select element not found';

                var found = false;
                for (var i = 0; i < select.options.length; i++) {{
                    // Compare the visible text (ignoring case and whitespace is safer)
                    if (select.options[i].text.trim() === '{textToSelect}') {{
                        select.selectedIndex = i;
                        found = true;
                        break;
                    }}
                }}

                if (found) {{
                    // Fire events so the website knows something changed
                    // (Required for React/Angular/Vue or dependent dropdowns)
                    select.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    select.dispatchEvent(new Event('input', {{ bubbles: true }}));
                    return 'true';
                }}
            
                    return 'false';
                }})();
            ";

            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            // Remember: WebView2 returns JSON strings (e.g. "\"true\"" or "\"false\"")
            return result.Contains("true");
        }

        public async Task<string> GetSelectTextAsync(string cssSelector)
        {
            // Build the script dynamically
            string script = $@"
                (function() {{
                    const element = document.querySelector('{cssSelector}');
                    if (!element) return null;
                    if (element.selectedIndex === -1) return '';
                    return element.options[element.selectedIndex].text;
                }})();";

            // Execute
            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            // WebView2 returns the result as a JSON string (e.g., ""Selected Text""). 
            // You must deserialize it or trim the quotes.
            if (result == "null") return null;

            // System.Text.Json is cleaner than string replacement
            return System.Text.Json.JsonSerializer.Deserialize<string>(result);
        }

        public async Task<bool> GetCheckboxStateAsync(string cssSelector)
        {
            string script = $@"
                (function() {{
                    const element = document.querySelector('{cssSelector}');
                    if (!element) return false; // Default to false if missing
                    return element.checked;
                }})();";

            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            // WebView2 returns JSON: "true" or "false" (or "null")
            // We can use bool.Parse, but we must clean the quotes first just in case
            // explicitly handling JSON is safer.
            if (bool.TryParse(result, out bool isChecked))
            {
                return isChecked;
            }

            // Fallback if result is weird (like "null")
            return false;
        }

        public async Task<string> GetSelectValueAsync(string cssSelector)
        {
            string script = $@"
                (function() {{
                    const element = document.querySelector('{cssSelector}');
                    if (!element) return null;
                    return element.value;
                }})();";

            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            // If the result is "null" (string), return actual null
            if (result == "null") return null;

            // Cleanly remove the JSON quotes (e.g., ""101"" -> "101")
            return System.Text.Json.JsonSerializer.Deserialize<string>(result);
        }

        public async Task<string> GetTextareaValueAsync(string cssSelector)
        {
            string script = $@"
                (function() {{
                    const element = document.querySelector('{cssSelector}');
                    if (!element) return null;
                    return element.value;
                }})();";

            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            if (result == "null") return null;

            // Deserialize to handle newlines (\n) and special characters correctly
            return System.Text.Json.JsonSerializer.Deserialize<string>(result);
        }

        public async Task<bool?> IsElementDisabledAsync(string cssSelector)
        {
            // Aiko's Note: This checks the standard HTML 'disabled' attribute.
            string script = $@"
                (function() {{
                    const element = document.querySelector('{cssSelector}');
                    if (!element) return null;
                    return element.disabled; 
                }})();";

            string result = await _webView.CoreWebView2.ExecuteScriptAsync(script);

            // If element wasn't found, JS returns null, which comes back as string "null"
            if (result == "null") return null;

            // Deserialize to bool (true/false)
            return System.Text.Json.JsonSerializer.Deserialize<bool>(result);
        }

        public async Task SetCheckboxStateAsync(string cssSelector, bool shouldBeChecked)
        {
            // We compare current state vs desired state.
            // If they differ, we .click() to trigger any attached events/PostBacks.
            string script = $@"
        (function() {{
            const checkbox = document.querySelector('{cssSelector}');
            if (!checkbox) return;

            // Only act if the state is different
            if (checkbox.checked !== {shouldBeChecked.ToString().ToLower()}) {{
                checkbox.click();
            }}
        }})();";

            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
