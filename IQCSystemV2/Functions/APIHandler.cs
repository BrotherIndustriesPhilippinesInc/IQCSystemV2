using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IQCSystemV2.Functions
{
    class APIHandler
    {
        public APIHandler() { }

        public async Task<JObject> APIGetCall(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send GET request
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Parse the response to JObject and return
                    JObject json = JObject.Parse(responseBody);
                    return json;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Request error:");
                    Console.WriteLine(e.Message);
                    return null; // Return null or throw an exception depending on your error handling strategy
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error:");
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        // Use a static client to prevent Socket Exhaustion
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<JObject> APIPostCall(string url, Dictionary<string, string> postData)
        {
            try
            {
                // 1. Serialize the dictionary to a JSON string
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);

                // 2. Wrap it in StringContent with the "application/json" header
                using (var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"))
                {
                    using (var response = await _httpClient.PostAsync(url, content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("API Error: " + response.StatusCode + " - " + error);
                            return null;
                        }

                        string responseBody = await response.Content.ReadAsStringAsync();
                        return JObject.Parse(responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                return null;
            }
        }
    }
}
