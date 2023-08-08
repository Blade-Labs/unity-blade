using Jint;
using Jint.Native;
using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Timers;
using System.Web;

/**
    Mnemonic not supported:
        - no `getKeysFromMnemonic` method
        - no `hethersSign(messageEncoded, accountPrivateKey, encoding)` method, only `sign(messageEncoded, accountPrivateKey, encoding)`
        - `createAccount` not returning mnemonic

    TODO: handle errors from remote signer server 

    TODO: get from BladeConfig nodeAccountId = "0.0.3";

    // TODO: splitSignature(signature: string, completionKey?: string): Promise<SplitSignatureData> {
    // TODO: async getParamsSignature(paramsEncoded: string | ParametersBuilder, privateKey: string, completionKey?: string): Promise<SplitSignatureData> {
    // TODO: getTransactions(accountId: string, transactionType: string = "", nextPage: string, transactionsLimit: string = "10", completionKey?: string): Promise<TransactionsHistoryData> {
    // TODO: getPendingAccount(transactionId: string, mnemonic: string, completionKey?: string): Promise<CreateAccountData> {
    // TODO: deleteAccount(deleteAccountId: string, deletePrivateKey: string, transferAccountId: string, operatorAccountId: string, operatorPrivateKey: string, completionKey?: string): Promise<TransactionReceipt>
*/
        
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
        string sdkVersion = "Swift@0.6.0"; // "Unity@0.6.0";
        private string executeApiEndpoint;
        
        public BladeSDK(string apiKey, Network network, string dAppCode, SdkEnvironment sdkEnvironment, string executeApiEndpoint = "http://localhost:8443") {
            this.apiKey = apiKey;
            this.network = network;
            this.dAppCode = dAppCode;
            this.sdkEnvironment = sdkEnvironment;
            this.executeApiEndpoint = executeApiEndpoint;

            this.apiService = new ApiService(network, sdkEnvironment, executeApiEndpoint, dAppCode);

            engine = new Engine();
            engine.SetValue("console",typeof(Debug));
            engine.Execute("window = {};");

            string absolutePath = Path.GetFullPath("Packages/io.bladelabs.unity-sdk/Resources/JSUnityWrapper.bundle.js");            
            var source = new StreamReader(absolutePath);
            string script = source.ReadToEnd();
            source.Close();
            engine.Execute(script);
            engine.Execute($"window.bladeSdk.init('{apiKey}', '{network}', '{dAppCode}', '{sdkEnvironment}', '{sdkVersion}')");
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

        public async Task<AccountInfoData> getAccountInfo(string accountId) {
            var account = await apiService.getAccount(accountId);
            string response = engine
                .Evaluate($"window.bladeSdk.getAccountInfo('{accountId}', '{account.evm_address}', '{account.key.key}')")
                .UnwrapIfPromise()
                .ToString();

            return this.processResponse<AccountInfoData>(response);
        }

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

        public async Task<ExecuteTxReceipt> transferTokens(string tokenId, string accountId, string accountPrivateKey, string recieverAccount, string amount, string memo, bool freeTransfer = false) {
            TokenData meta = await apiService.requestTokenInfo(tokenId);
            double correctedAmount = double.Parse(amount) * Math.Pow(10, int.Parse(meta.decimals));
            
            if (freeTransfer == true) {
                // for free transfer BladeApi need to create TX, from our initial data.
                // to call BladeApi we need TVTE-token                
                // after we get transactionBytes need to sign it with sender private key
                // after signing - need to execute this TX

                // generating TVTE token
                string response = engine
                    .Evaluate($"window.bladeSdk.getTvteValue()")
                    .UnwrapIfPromise()
                    .ToString();
                TVTEResponse tvteResponse = this.processResponse<TVTEResponse>(response);

                // bladeApi create and sign TX, and return base64-encoded transactionBytes
                FreeTokenTransferResponse freeTokenTransferResponse = await apiService.freeTokenTransfer(accountId, recieverAccount, correctedAmount, memo, tvteResponse.tvte);

                // sign with sender private key
                SignedTx signedTx = this.signTransaction(freeTokenTransferResponse.transactionBytes, "base64", accountPrivateKey);

                //send tx to execution
                return await apiService.executeTx(signedTx.tx, signedTx.network);
            } else {
                string response = engine
                    .Evaluate($"window.bladeSdk.transferTokens('{tokenId}', '{accountId}', '{accountPrivateKey}', '{recieverAccount}', {correctedAmount}, '{memo}')")
                    .UnwrapIfPromise()
                    .ToString();

                SignedTx signedTx = this.processResponse<SignedTx>(response);
                return await apiService.executeTx(signedTx.tx, signedTx.network);
            }
        }

        public async Task<CreateAccountData> createAccount(string deviceId) {
            // generateKeys
            string keyResponse = engine
                .Evaluate($"window.bladeSdk.generateKeys()")
                .UnwrapIfPromise()
                .ToString();
            KeyPairData keyPairData = this.processResponse<KeyPairData>(keyResponse);
            string tvteToken = this.getTvteToken();

            CreateAccountResponse createAccountResponse = await apiService.createAccount(keyPairData.publicKey, deviceId, tvteToken);

            // sign and execute transactions if any
            if (createAccountResponse.updateAccountTransactionBytes != null) {
                SignedTx signedTx = this.signTransaction(createAccountResponse.updateAccountTransactionBytes, "base64", keyPairData.privateKey);
                ExecuteTxReceipt executeTxReceipt = await apiService.executeTx(signedTx.tx, signedTx.network);
                // confirm account key update
                if (executeTxReceipt.status == "SUCCESS") {
                    await apiService.confirmAccountUpdate(createAccountResponse.id, tvteToken);
                }
            }

            if (createAccountResponse.transactionBytes != null) {
                SignedTx signedTx = this.signTransaction(createAccountResponse.transactionBytes, "base64", keyPairData.privateKey);
                ExecuteTxReceipt executeTxReceipt = await apiService.executeTx(signedTx.tx, signedTx.network);
            }

            CreateAccountData createAccountData = new CreateAccountData {
                transactionId = createAccountResponse.transactionId,
                status = string.IsNullOrEmpty(createAccountResponse.transactionId) ? "SUCCESS" : "PENDING",
                seedPhrase = "[not supported in now]",
                publicKey = keyPairData.publicKey,
                privateKey = keyPairData.privateKey,
                accountId = createAccountResponse.id,
                evmAddress = keyPairData.evmAddress
            };
            return createAccountData;
        }

        public async Task<ExecuteTxReceipt> contractCallFunction(string contractId, string functionName, ContractFunctionParameters parameters, string accountId, string accountPrivateKey, uint gas, bool bladePayFee = false) {
            if (bladePayFee) {
                string contractCallBytecodeResponse = engine
                    .Evaluate($"window.bladeSdk.getContractCallBytecode('{functionName}', '{parameters.encode()}')")
                    .UnwrapIfPromise()
                    .ToString();
                ContractCallBytecode contractCallBytecode = this.processResponse<ContractCallBytecode>(contractCallBytecodeResponse);

                SignContractCallResponse signContractCallResponse = await apiService.signContractCallTx(
                    contractCallBytecode.contractFunctionParameters,
                    contractId,
                    functionName,
                    gas,
                    this.getTvteToken(),
                    false
                );
                SignedTx signedTx = this.signTransaction(signContractCallResponse.transactionBytes, "base64", accountPrivateKey);
                return await apiService.executeTx(signedTx.tx, signedTx.network);
            } else {
                string signedTxResponse = engine
                    .Evaluate($"window.bladeSdk.contractCallFunctionTransaction('{contractId}', '{functionName}', '{parameters.encode()}', '{accountId}', '{accountPrivateKey}', {gas})")
                    .UnwrapIfPromise()
                    .ToString();
                SignedTx signedTx = this.processResponse<SignedTx>(signedTxResponse);
                return await apiService.executeTx(signedTx.tx, signedTx.network);
            }
        }

        public async Task<ContractQueryData> contractCallQueryFunction(
            string contractId, 
            string functionName, 
            ContractFunctionParameters parameters,
            string accountId,
            string accountPrivateKey, 
            uint gas, 
            uint fee,
            List<string> returnTypes
        ) {
            if (fee > 0) {
                string nodeAccountId = "0.0.3";            
                string delayedQueryCallResponse = engine
                    .Evaluate($"window.bladeSdk.contractCallQueryFunction('{contractId}', '{functionName}', '{parameters.encode()}', '{accountId}', '{accountPrivateKey}', {gas}, {fee}, '{nodeAccountId}')")
                    .UnwrapIfPromise()
                    .ToString();
                DelayedQueryCall delayedQueryCall = this.processResponse<DelayedQueryCall>(delayedQueryCallResponse);
                
                DelayedQueryCallResult delayedQueryCallResult = await apiService.executeDelayedQueryCall(delayedQueryCall);
                
                string contractCallQueryResponse = engine
                    .Evaluate($"window.bladeSdk.parseContractCallQueryResponse('{contractId}', '{delayedQueryCallResult.contractFunctionResult.gasUsed}', '{delayedQueryCallResult.rawResult}', ['{string.Join("', '", returnTypes)}'])")
                    .UnwrapIfPromise()
                    .ToString();
                ContractQueryData contractQueryData = this.processResponse<ContractQueryData>(contractCallQueryResponse);
                return contractQueryData;
            } else {
                // blade pay fee 
                throw new BladeSDKException("Error", "free queries not implemented yet");
            }
        }


        public async Task<string> getC14url(
            string asset,
            string account,
            string amount
        ) {
            string tvteToken = this.getTvteToken();
            string clientId = await apiService.getC14token(tvteToken);
            
            var purchaseParams = new Dictionary<string, object> {
                { "clientId", clientId }
            };

            switch (asset.ToUpper()) {
                case "USDC":
                    purchaseParams["targetAssetId"] = "b0694345-1eb4-4bc4-b340-f389a58ee4f3";
                    purchaseParams["targetAssetIdLock"] = "true";
                    break;
                case "HBAR":
                    purchaseParams["targetAssetId"] = "d9b45743-e712-4088-8a31-65ee6f371022";
                    purchaseParams["targetAssetIdLock"] = "true";
                    break;
                case "KARATE":
                    purchaseParams["targetAssetId"] = "057d6b35-1af5-4827-bee2-c12842faa49e";
                    purchaseParams["targetAssetIdLock"] = "true";
                    break;
                default:
                    if (asset.Split('-').Length == 5) {
                        purchaseParams["targetAssetId"] = asset;
                        purchaseParams["targetAssetIdLock"] = "true";
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(amount)) {
                purchaseParams["sourceAmount"] = amount;
                purchaseParams["quoteAmountLock"] = "true";
            }

            if (!string.IsNullOrEmpty(account)) {
                purchaseParams["targetAddress"] = account;
                purchaseParams["targetAddressLock"] = "true";
            }

            string queryString = "";
            foreach (var kv in purchaseParams) {
                queryString = queryString + $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value.ToString())}&";
            }

            var uriBuilder = new UriBuilder("https://pay.c14.money/");
            uriBuilder.Query = queryString.ToString();
            return uriBuilder.Uri.ToString();
        }

        public async Task<SignMessageData> sign(
            string messageEncoded, 
            string accountPrivateKey,
            string encoding // hex|base64|utf8
        ) {
            string signMessageReponse = engine
                .Evaluate($"window.bladeSdk.sign('{messageEncoded}', '{accountPrivateKey}', '{encoding}')")
                .UnwrapIfPromise()
                .ToString();
            SignMessageData signMessageData = this.processResponse<SignMessageData>(signMessageReponse);
            return signMessageData;
        }

        public async Task<bool> signVerify(
            string messageEncoded, 
            string signatureHex,
            string publicKey, // hex-encoded public key with DER header
            string encoding // hex|base64|utf8
        ) {
            string signVerifyMessageResponse = engine
                .Evaluate($"window.bladeSdk.signVerify('{messageEncoded}', '{signatureHex}', '{publicKey}', '{encoding}')")
                .UnwrapIfPromise()
                .ToString();
            SignVerifyMessageData signVerifyMessageData = this.processResponse<SignVerifyMessageData>(signVerifyMessageResponse);
            return signVerifyMessageData.valid;
        }

        // PRIVATE METHODS

        private SignedTx signTransaction(string transactionBytes, string encoding, string accountPrivateKey) {
            string signedTxResponse = engine
                .Evaluate($"window.bladeSdk.signTransaction('{transactionBytes}', '{encoding}', '{accountPrivateKey}')")
                .UnwrapIfPromise()
                .ToString();
            return this.processResponse<SignedTx>(signedTxResponse);
        }

        private string getTvteToken() {
            // generating TVTE token    
            string response = engine
                    .Evaluate($"window.bladeSdk.getTvteValue()")
                    .UnwrapIfPromise()
                    .ToString();
            TVTEResponse tvteResponse = this.processResponse<TVTEResponse>(response);
            return tvteResponse.tvte;
        }

        private T processResponse<T>(string rawJson) {
            Response<T> response = JsonUtility.FromJson<Response<T>>(rawJson);
            T data = (T)response.data;
            BladeJSError error = (BladeJSError)response.error;

            if (error.name != null || error.reason != null) {
                Debug.Log($"processResponse() throwing BladeSDKException({error.name}, {error.reason})");
                throw new BladeSDKException(error.name, error.reason);
            }

            return data;
        }

        ~BladeSDK() {
            // Debug.Log("~BladeSDK() dispose");
        }
    }
}
