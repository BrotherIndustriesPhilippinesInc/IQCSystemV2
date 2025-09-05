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

        public async Task<JObject> APIPostCall(string url, Dictionary<string, string> postData)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var content = new FormUrlEncodedContent(postData);
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Request error:");
                    Console.WriteLine(e.Message);
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error:");
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }
    }
}
