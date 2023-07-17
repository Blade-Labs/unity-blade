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
        ApiService apiService;
        string apiKey;
        private Network network = Network.Testnet;
        string dAppCode;
        private SdkEnvironment sdkEnvironment;
        string sdkVersion = "Unity@0.6.0";
        private string executeApiEndpoint;
        
        public BladeSDK(string apiKey, Network network, string dAppCode, SdkEnvironment sdkEnvironment, string executeApiEndpoint = "http://localhost:8443/signer/tx") {
            this.apiKey = apiKey;
            this.network = network;
            this.dAppCode = dAppCode;
            this.sdkEnvironment = sdkEnvironment;
            this.executeApiEndpoint = executeApiEndpoint;

            this.apiService = new ApiService(network, sdkEnvironment, executeApiEndpoint);

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

        public async Task<InfoData> getInfo() {
            return new InfoData {
                apiKey = this.apiKey,
                dAppCode = this.dAppCode,
                network = this.network,
                visitorId = "[not implemented yet]",
                sdkEnvironment = this.sdkEnvironment,
                sdkVersion = this.sdkVersion
            };
        }

        // TODO: contractCallFunction(contractId: string, functionName: string, paramsEncoded: string | ParametersBuilder, accountId: string, accountPrivateKey: string, gas: number = 100000, bladePayFee: boolean = false, completionKey?: string): Promise<Partial<TransactionReceipt>>
        // TODO: contractCallQueryFunction(contractId: string, functionName: string, paramsEncoded: string | ParametersBuilder, accountId: string, accountPrivateKey: string, gas: number = 100000, bladePayFee: boolean = false, resultTypes: string[]): Promise<ContractCallQueryRecord[]>
        // TODO: transferTokens(tokenId: string, accountId: string, accountPrivateKey: string, receiverID: string, amount: string, memo: string, freeTransfer: boolean = false, completionKey?: string): Promise<TransactionResponse> {
        // TODO: createAccount(deviceId?: string, completionKey?: string): Promise<CreateAccountData>
        // TODO: getPendingAccount(transactionId: string, mnemonic: string, completionKey?: string): Promise<CreateAccountData> {
        // TODO: deleteAccount(deleteAccountId: string, deletePrivateKey: string, transferAccountId: string, operatorAccountId: string, operatorPrivateKey: string, completionKey?: string): Promise<TransactionReceipt>

        public async Task<AccountInfoData> getAccountInfo(string accountId) {
            var account = await apiService.getAccount(accountId);
            string response = engine
                .Evaluate($"window.bladeSdk.getAccountInfo('{accountId}', '{account.evm_address}', '{account.key.key}')")
                .UnwrapIfPromise()
                .ToString();

            return this.processResponse<AccountInfoData>(response);
        }

        // TODO: getKeysFromMnemonic(mnemonicRaw: string, lookupNames: boolean, completionKey?: string): Promise<PrivateKeyData> {
        // TODO: sign(messageString: string, privateKey: string, completionKey?: string): Promise<SignMessageData> {
        // TODO: signVerify(messageString: string, signature: string, publicKey: string, completionKey?: string): Promise<SignVerifyMessageData> {
        // TODO: hethersSign(messageString: string, privateKey: string, completionKey?: string): Promise<SignMessageData>
        // TODO: splitSignature(signature: string, completionKey?: string): Promise<SplitSignatureData> {
        // TODO: async getParamsSignature(paramsEncoded: string | ParametersBuilder, privateKey: string, completionKey?: string): Promise<SplitSignatureData> {
        // TODO: getTransactions(accountId: string, transactionType: string = "", nextPage: string, transactionsLimit: string = "10", completionKey?: string): Promise<TransactionsHistoryData> {
        // TODO: getC14url(asset: string, account: string, amount: string, completionKey?: string): Promise<IntegrationUrlData> {



        public async Task<AccountBalanceData> getBalance(string accountId) {
            return await apiService.getBalance(accountId);
        }
        
        public async Task<ExecuteTxReceipt> transferHbars(string accountId, string accountPrivateKey, string recieverAccount, string amount, string memo) {
            string response = engine
                .Evaluate($"window.bladeSdk.transferHbars('{accountId}', '{accountPrivateKey}', '{recieverAccount}', '{amount}', '{memo}')")
                .UnwrapIfPromise()
                .ToString();

            SignedTx signedTx = this.processResponse<SignedTx>(response);
            return await apiService.executeTx(signedTx.tx, signedTx.network);
        }


        private T processResponse<T>(string rawJson) {
            Response<T> response = JsonUtility.FromJson<Response<T>>(rawJson);
            T data = (T)response.data;
            BladeJSError error = (BladeJSError)response.error;

            if (error.name != null || error.reason != null) {
                Debug.Log($"throwing BladeSDKException({error.name}, {error.reason})");
                throw new BladeSDKException(error.name, error.reason);
            }

            return data;
        }

        


        void setTimeout(Action<int> callback, int delay)
        {
            Debug.Log("Warning setTimeout use. Try to avoid");
            Task.Delay(delay).ContinueWith(_ => callback(delay));
        }

        ~BladeSDK() {
            Debug.Log("~BladeSDK() dispose");
        }
    }
}
