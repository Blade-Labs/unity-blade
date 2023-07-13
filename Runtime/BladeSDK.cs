using Jint;
using Jint.Native;
using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
// using System.Text.Json;
// using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Timers;


namespace BladeLabs.UnitySDK
{
    public class BladeSDK
    {
        private static Engine engine;

        public BladeSDK() {
            engine = new Engine();
            engine.SetValue("console",typeof(Debug));
            engine.Execute("window = {};");
            engine.Execute("global = {};");
            engine.SetValue("setTimeout", new Action<Action<int>, int>(setTimeout));
            engine.SetValue("CXMLHttpRequest", typeof(CXMLHTTPRequest));

// "Packages/com.unity.images-library/Example/Images/image.png"

// ...
string absolutePath =   Path.GetFullPath("Packages/io.bladelabs.unity-sdk/Resources/JSWrapper.bundle.js");
            
            
Debug.Log(absolutePath);
Debug.Log(Application.dataPath);



            // var source = new StreamReader(Application.dataPath + "/Resources/" + "JSWrapper.bundle.js");
            var source = new StreamReader(absolutePath);
            string script = source.ReadToEnd();
            source.Close();
            engine.Execute(script);
        }
        
        public async Task<bool> transferHbars(string accountId, string accountPrivateKey, string recieverAccount, string amount, string memo) {
            
            string tx = engine
                .Evaluate($"window.bladeSdk.transferHbars('{accountId}', '{accountPrivateKey}', '{recieverAccount}', '{amount}', '{memo}', 'transferHbars1')")
                .UnwrapIfPromise()
                .ToString();
            
            // send tx

            Debug.Log(tx);

            // var responseValue = await executeTx(tx, "testnet");
            // if (responseValue.status != null) {
            //     Debug.Log(responseValue.status);
            // } else {
            //     Debug.Log("FAIL");
            // }
            // return responseValue.status == "SUCCESS";

            return false;
        }

        static async Task<ResponseObject> executeTx(string tx, string network)
        {
            using (HttpClient client = new HttpClient())
            {
                string jsonPayload = "{\"tx\": \"" + tx + "\", \"network\": \"" + network + "\"}";
                HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:8443/signer/tx", content);


                if (response.IsSuccessStatusCode) {

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    // var responseObject = JsonSerializer.Deserialize<ResponseObject>(jsonResponse);
                    var responseObject = JsonUtility.FromJson<ResponseObject>(jsonResponse);
                    return responseObject;
                } else {
                    Console.WriteLine($"HTTP Request Error: {response.StatusCode}");

                    var responseObject = JsonUtility.FromJson<ResponseObject>("{}");
                    return responseObject;
                }
            }
        }


        void setTimeout(Action<int> callback, int delay)
        {
            Task.Delay(delay).ContinueWith(_ => callback(delay));
        }
    }

    public class CXMLHTTPRequest
    {
        private HttpClient client;
        private HttpRequestMessage request;
        private HttpResponseMessage response;
        private Action onReadyStateChange;
        private Action<string> onLoad;
        private Action onError;

        private bool isPolling;
        private string requestId = "";

        public CXMLHTTPRequest()
        {
            client = new HttpClient();
            request = new HttpRequestMessage();
            isPolling = false;
        }

        public void open(string method, string url, bool async = true)
        {
            request.Method = new HttpMethod(method);
            request.RequestUri = new Uri(url);
        }

        public void send(string bodyInit = "", string _requestId = "")
        {
            requestId = _requestId;
            response = client.SendAsync(request).GetAwaiter().GetResult();
            StartPolling();
        }

        private async void StartPolling()
        {
            int delay = 100;
            int timeout = 30000;
            isPolling = true;
            while (isPolling)
            {
                await Task.Delay(delay); // Adjust the polling interval as needed
                timeout -= delay;
                if ((int)(response?.StatusCode ?? 0) > 0) {

                    Debug.Log("response code: " + response?.StatusCode);

                    isPolling = false;

                    Debug.Log("onLoad?: " + onLoad);

                    onReadyStateChange?.Invoke();
                    onLoad?.Invoke(requestId);
                }

                // TODO on error

                if (timeout <= 0) {
                    // onTimeout?.Invoke();
                    // onAbort?.Invoke();
                }
                
            }
        }

        public void abort()
        {
            isPolling = false;
            request.Dispose();
            // onAbort?.Invoke();
        }

        public string getAllResponseHeaders() {
            return "";
        }

        public string responseText
        {
            get
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        public int status
        {
            get
            {
                return (int)(response?.StatusCode ?? 0);
            }
        }

        public int readyState
        {
            get
            {
                return isPolling ? 3 : 4;
            }
        }

        public Action onreadystatechange
        {
            get
            {
                return onReadyStateChange;
            }
            set
            {
                Debug.Log("onreadystatechange set");
                onReadyStateChange = value;
            }
        }

        public Action<string> onload
        {
            get
            {
                return onLoad;
            }
            set
            {
                Debug.Log("onload set");
                onLoad = value;
            }
        }

        public Action onerror
        {
            get
            {
                return onError;
            }
            set
            {
                Debug.Log("onerror set");
                onError = value;
            }
        }
    }

    [Serializable]
    public struct ResponseObject
    {
        public string status;
    }
}
