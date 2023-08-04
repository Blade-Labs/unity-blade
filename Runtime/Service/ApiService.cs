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
        private string visitorId;
        private string dAppCode;


        public ApiService(Network network, SdkEnvironment sdkEnvironment, string executeApiEndpoint, string dAppCode) {
            this.network = network;
            this.sdkEnvironment = sdkEnvironment;
            this.executeApiEndpoint = executeApiEndpoint;
            // TODO replace visitorID with proper solution for this platform
            this.visitorId = "O9LAocV5ISChRrBCtvpY";
            this.dAppCode = dAppCode;
        }

        private string getSdkApi(string route) {
            return this.executeApiEndpoint + route;
        }

        private string getApiUrl(string route) {
            string host = this.sdkEnvironment == SdkEnvironment.Prod
                ? "https://rest.prod.bladewallet.io/openapi/v7"
                : "https://api.bld-dev.bladewallet.io/openapi/v7"
                // : "https://rest.ci.bladewallet.io/openapi/v7"
            ;

            return host + route;
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

        public async Task<TokenData> requestTokenInfo(string tokenId) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    HttpResponseMessage response = await httpClient.GetAsync(getMirrorNodeUrl($"/api/v1/tokens/{tokenId}"));
                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        var responseObject = JsonUtility.FromJson<TokenData>(content);
                        return responseObject;
                    } else {
                        throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
        }

        public async Task<FreeTokenTransferResponse> freeTokenTransfer(string senderAccountId, string receiverAccountId, double amount, string memo, string xTvteApiToken) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    FreeTokenTransferRequest request = new FreeTokenTransferRequest {
                        receiverAccountId = receiverAccountId,
                        senderAccountId = senderAccountId,
                        amount = amount,
                        decimals = null,
                        memo = memo
                    };
                    string body = JsonUtility.ToJson(request);
                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    
                    httpClient.DefaultRequestHeaders.Add("X-NETWORK", this.network.ToString().ToUpper());
                    httpClient.DefaultRequestHeaders.Add("X-VISITOR-ID", this.visitorId);
                    httpClient.DefaultRequestHeaders.Add("X-DAPP-CODE", this.dAppCode);
                    httpClient.DefaultRequestHeaders.Add("X-SDK-TVTE-API", xTvteApiToken);
                    
                    HttpResponseMessage response = await httpClient.PostAsync(getApiUrl($"/tokens/transfers"), bodyContent);

                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        var responseObject = JsonUtility.FromJson<FreeTokenTransferResponse>(content);
                        return responseObject;
                    } else {
                        throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
        }

        public async Task<CreateAccountResponse> createAccount(string publicKey, string deviceId, string xTvteApiToken) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    CreateAccountRequest request = new CreateAccountRequest {
                        publicKey = publicKey
                    };
                    string body = JsonUtility.ToJson(request);

                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    
                    httpClient.DefaultRequestHeaders.Add("X-NETWORK", this.network.ToString().ToUpper());
                    httpClient.DefaultRequestHeaders.Add("X-VISITOR-ID", this.visitorId);
                    httpClient.DefaultRequestHeaders.Add("X-DAPP-CODE", this.dAppCode);
                    httpClient.DefaultRequestHeaders.Add("X-SDK-TVTE-API", xTvteApiToken);
                    
                    if (deviceId != "") {
                        httpClient.DefaultRequestHeaders.Add("X-DID-API", deviceId);
                    }

                    HttpResponseMessage response = await httpClient.PostAsync(getApiUrl($"/accounts"), bodyContent);

                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        var responseObject = JsonUtility.FromJson<CreateAccountResponse>(content);
                        return responseObject;
                    } else {
                        throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
        }

        public async Task<bool> confirmAccountUpdate(string accountId, string xTvteApiToken) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    ConfirmAccountRequest request = new ConfirmAccountRequest {
                        id = accountId
                    };
                    string body = JsonUtility.ToJson(request);
                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    
                    httpClient.DefaultRequestHeaders.Add("X-NETWORK", this.network.ToString().ToUpper());
                    httpClient.DefaultRequestHeaders.Add("X-VISITOR-ID", this.visitorId);
                    httpClient.DefaultRequestHeaders.Add("X-DAPP-CODE", this.dAppCode);
                    httpClient.DefaultRequestHeaders.Add("X-SDK-TVTE-API", xTvteApiToken);

                    HttpResponseMessage response = await httpClient.PatchAsync(getApiUrl($"/accounts/confirm"), bodyContent);

                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        return true;
                    } else {
                        throw new BladeSDKException($"HTTP Request Error: {response.StatusCode}", content);
                    }
                } catch (HttpRequestException ex) {
                    throw new BladeSDKException($"HttpRequestException", ex.Message);
                }
            }
        }

        public async Task<SignContractCallResponse> signContractCallTx(
            string contractFunctionParameters,
            string contractId,
            string functionName,
            uint gas,
            string xTvteApiToken,
            bool contractCallQuery = false
        ) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    SignContractCallRequest request = new SignContractCallRequest {
                        functionParametersHash = contractFunctionParameters,
                        contractId = contractId,
                        functionName = functionName,
                        gas = gas
                    };
                    string body = JsonUtility.ToJson(request);
                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    
                    httpClient.DefaultRequestHeaders.Add("X-NETWORK", this.network.ToString().ToUpper());
                    httpClient.DefaultRequestHeaders.Add("X-VISITOR-ID", this.visitorId);
                    httpClient.DefaultRequestHeaders.Add("X-DAPP-CODE", this.dAppCode);
                    httpClient.DefaultRequestHeaders.Add("X-SDK-TVTE-API", xTvteApiToken);
                    
                    HttpResponseMessage response = await httpClient.PostAsync(getApiUrl($"/smart/contract/{(contractCallQuery ? "call" : "sign")}"), bodyContent);

                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {

// TODO REMOVE. Debugging contractCallQuery 
Debug.Log($"isQuery = {contractCallQuery}, rawResponse = {content}");

                        var responseObject = JsonUtility.FromJson<SignContractCallResponse>(content);
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
                    HttpResponseMessage response = await httpClient.PostAsync(getSdkApi("/signer/tx"), bodyContent);
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

        
        public async Task<ExecuteTxReceipt> executeDelayedQueryCall(DelayedQueryCall delayedQueryCall) {
            using (HttpClient httpClient = new HttpClient()) {
                try {
                    string body = JsonUtility.ToJson(delayedQueryCall);
                    HttpContent bodyContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(getSdkApi("/signer/query"), bodyContent);
                    string content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        // TODO describe data class 
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
            // Debug.Log("~ApiService dispose");
        }
    }
}