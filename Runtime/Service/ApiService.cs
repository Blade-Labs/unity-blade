using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BladeLabs.UnitySDK
{
    public class ApiService {

        private Network network;
        private SdkEnvironment sdkEnvironment;
        private string executeApiEndpoint;


        public ApiService(Network network, SdkEnvironment sdkEnvironment, string executeApiEndpoint) {
            this.network = network;
            this.sdkEnvironment = sdkEnvironment;
            this.executeApiEndpoint = executeApiEndpoint;
        }

        private string getApiUrl() {
            return this.sdkEnvironment == SdkEnvironment.Prod
                ? "https://rest.prod.bladewallet.io/openapi/v7"
                : "https://rest.ci.bladewallet.io/openapi/v7";
        }

        private string getMirrorNodeUrl(string route) {
            string host = this.network == Network.Mainnet 
                ? "https://mainnet-public.mirrornode.hedera.com"
                : "https://testnet.mirrornode.hedera.com";
            return host + route;
        }

        public async Task<AccountData> getAccount(string accountId) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    HttpResponseMessage response = await httpClient.GetAsync(getMirrorNodeUrl($"/api/v1/accounts/{accountId}"));
                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        var responseObject = JsonUtility.FromJson<AccountData>(content);
                        return responseObject;
                    } else {
                        throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
        }

        public async Task<List<TokenBalance>> getAccountTokens(string accountId) {
            List<TokenBalance> result = new List<TokenBalance>();

            using (HttpClient httpClient = new HttpClient()) {
                try {
                    string nextPage = getMirrorNodeUrl($"/api/v1/accounts/{accountId}/tokens");

                    while (nextPage != null) {
                        HttpResponseMessage response = await httpClient.GetAsync(nextPage);
                        string content = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode) {
                            var balanceDataResponse = JsonUtility.FromJson<BalanceDataResponse>(content);                            
                            nextPage = string.IsNullOrEmpty(balanceDataResponse.links.next) ? null : getMirrorNodeUrl(balanceDataResponse.links.next);
                            result.AddRange(balanceDataResponse.tokens);
                        } else {
                            throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                        }
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
            return result;
        }

        public async Task<AccountBalanceData> getBalance(string accountId) {
            var account = await this.getAccount(accountId);
            var tokens = await this.getAccountTokens(accountId);            
            
            return new AccountBalanceData {
                balance = account.balance.balance,
                timestamp = account.balance.timestamp,
                tokens = tokens
            };
        }

        public async Task<ExecuteTxReceipt> executeTx(string tx, string network) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    ExecuteTxRequest request = new ExecuteTxRequest {
                        tx = tx,
                        network = network
                    };
                    string body = JsonUtility.ToJson(request);
                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(this.executeApiEndpoint, bodyContent);

                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
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

        ~ApiService() {
            Debug.Log("~ApiService dispose");
        }
    }
}