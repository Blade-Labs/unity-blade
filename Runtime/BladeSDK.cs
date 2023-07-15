using Jint;
using Jint.Native;
using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Timers;


namespace BladeLabs.UnitySDK
{
    public class BladeSDK
    {
        private Engine engine;
        private string executeApiEndpoint;
        private string network;

        public BladeSDK(string network = "testnet", string executeApiEndpoint = "http://localhost:8443/signer/tx") {
            
            this.network = network;
            this.executeApiEndpoint = executeApiEndpoint;

            engine = new Engine();
            engine.SetValue("console",typeof(Debug));
            engine.Execute("window = {};");
            engine.Execute("global = {};");
            engine.SetValue("setTimeout", new Action<Action<int>, int>(setTimeout));

            string absolutePath = Path.GetFullPath("Packages/io.bladelabs.unity-sdk/Resources/JSUnityWrapper.bundle.js");            
            var source = new StreamReader(absolutePath);
            string script = source.ReadToEnd();
            source.Close();
            engine.Execute(script);
        }
        
        public async Task<bool> transferHbars(string accountId, string accountPrivateKey, string recieverAccount, string amount, string memo) {
            
            string response = engine
                .Evaluate($"window.bladeSdk.transferHbars('{accountId}', '{accountPrivateKey}', '{recieverAccount}', '{amount}', '{memo}')")
                .UnwrapIfPromise()
                .ToString();

            SignedTx signedTx = this.processResponse<SignedTx>(response);
            
            ExecuteTxReceipt responseValue = await executeTx(signedTx.tx, signedTx.network);
            if (responseValue.status != null) {
                Debug.Log(responseValue.status);
            } else {
                Debug.Log("FAIL");
            }
            return responseValue.status == "SUCCESS";
        }


        private T processResponse<T>(string rawJson) {
            Debug.Log(rawJson);

            Response<T> response = JsonUtility.FromJson<Response<T>>(rawJson);
            T data = (T)response.data;
            BladeJSError error = (BladeJSError)response.error;

            if (error.name != null || error.reason != null) {
                Debug.Log($"throwing BladeSDKException({error.name}, {error.reason})");
                throw new BladeSDKException(error.name, error.reason);
            }

            return data;
        }

        async Task<ExecuteTxReceipt> executeTx(string tx, string network)
        {
            using (HttpClient client = new HttpClient())
            {
                try {
                    ExecuteTxRequest request = new ExecuteTxRequest {
                        tx = tx,
                        network = network
                    };
                    string body = JsonUtility.ToJson(request);
                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(this.executeApiEndpoint, bodyContent);

                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        Debug.Log(content);
                        var responseObject = JsonUtility.FromJson<ExecuteTxReceipt>(content);
                        return responseObject;
                    } else {
                        throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
        }


        void setTimeout(Action<int> callback, int delay)
        {
            Debug.Log("Warning setTimeout use. Try to avoid");
            Task.Delay(delay).ContinueWith(_ => callback(delay));
        }
    }
}
