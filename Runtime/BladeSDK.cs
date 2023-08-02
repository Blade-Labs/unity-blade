using Jint;
using Jint.Native;
using UnityEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Timers;

using System.Numerics;

/**
    Mnemonic not supported:
        - no `getKeysFromMnemonic` method
        - `createAccount` not returning mnemonic

    // TODO: contractCallFunction(contractId: string, functionName: string, paramsEncoded: string | ParametersBuilder, accountId: string, accountPrivateKey: string, gas: number = 100000, bladePayFee: boolean = false, completionKey?: string): Promise<Partial<TransactionReceipt>>
    // TODO: contractCallQueryFunction(contractId: string, functionName: string, paramsEncoded: string | ParametersBuilder, accountId: string, accountPrivateKey: string, gas: number = 100000, bladePayFee: boolean = false, resultTypes: string[]): Promise<ContractCallQueryRecord[]>
    // TODO: getC14url(asset: string, account: string, amount: string, completionKey?: string): Promise<IntegrationUrlData> {

    // TODO: getPendingAccount(transactionId: string, mnemonic: string, completionKey?: string): Promise<CreateAccountData> {
    // TODO: deleteAccount(deleteAccountId: string, deletePrivateKey: string, transferAccountId: string, operatorAccountId: string, operatorPrivateKey: string, completionKey?: string): Promise<TransactionReceipt>
    // TODO: sign(messageString: string, privateKey: string, completionKey?: string): Promise<SignMessageData> {
    // TODO: signVerify(messageString: string, signature: string, publicKey: string, completionKey?: string): Promise<SignVerifyMessageData> {
    // TODO: hethersSign(messageString: string, privateKey: string, completionKey?: string): Promise<SignMessageData>
    // TODO: splitSignature(signature: string, completionKey?: string): Promise<SplitSignatureData> {
    // TODO: async getParamsSignature(paramsEncoded: string | ParametersBuilder, privateKey: string, completionKey?: string): Promise<SplitSignatureData> {
    // TODO: getTransactions(accountId: string, transactionType: string = "", nextPage: string, transactionsLimit: string = "10", completionKey?: string): Promise<TransactionsHistoryData> {
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
        
        public BladeSDK(string apiKey, Network network, string dAppCode, SdkEnvironment sdkEnvironment, string executeApiEndpoint = "http://localhost:8443/signer/tx") {
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
            
            
            ContractFunctionParameters tuple0 = new ContractFunctionParameters().addInt64(16).addInt64(32);
            ContractFunctionParameters tuple1 = new ContractFunctionParameters().addInt64(5).addInt64(10);
            ContractFunctionParameters tuple2 = new ContractFunctionParameters().addInt64(50).addTupleArray(new List<ContractFunctionParameters> {tuple0, tuple1});

            ContractFunctionParameters parametersTest = new ContractFunctionParameters();
            parametersTest
                .addString("Hello, Backend")
                .addBytes32(new List<uint> { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F})
                .addAddressArray(new List<string> {"0.0.48738539", "0.0.48738538", "0.0.48738537"})
                .addAddress("0.0.48850466")
                .addAddress("0.0.499326")
                .addAddress("0.0.48801688")
                .addInt64(1)
                .addUInt8(123)
                .addUInt64Array(new List<ulong> {1,2,3})
                .addUInt256Array(new List<BigInteger> {1,2,3})
                .addTuple(tuple1)
                .addTuple(tuple2)
                .addTupleArray(new List<ContractFunctionParameters> {tuple0, tuple1})
                .addTupleArray(new List<ContractFunctionParameters> {tuple2, tuple2})
                .addAddress("0.0.12345")
                .addUInt64(56784645645)
                .addUInt256(12345)
            ;            
            
            
            Debug.Log(parametersTest.encode() == "W3sidHlwZSI6InN0cmluZyIsInZhbHVlIjpbIkhlbGxvLCBCYWNrZW5kIl19LHsidHlwZSI6ImJ5dGVzMzIiLCJ2YWx1ZSI6WyJXekFzTVN3eUxETXNOQ3cxTERZc055dzRMRGtzTVRBc01URXNNVElzTVRNc01UUXNNVFVzTVRZc01UY3NNVGdzTVRrc01qQXNNakVzTWpJc01qTXNNalFzTWpVc01qWXNNamNzTWpnc01qa3NNekFzTXpGZCJdfSx7InR5cGUiOiJhZGRyZXNzW10iLCJ2YWx1ZSI6WyIwLjAuNDg3Mzg1MzkiLCIwLjAuNDg3Mzg1MzgiLCIwLjAuNDg3Mzg1MzciXX0seyJ0eXBlIjoiYWRkcmVzcyIsInZhbHVlIjpbIjAuMC40ODg1MDQ2NiJdfSx7InR5cGUiOiJhZGRyZXNzIiwidmFsdWUiOlsiMC4wLjQ5OTMyNiJdfSx7InR5cGUiOiJhZGRyZXNzIiwidmFsdWUiOlsiMC4wLjQ4ODAxNjg4Il19LHsidHlwZSI6ImludDY0IiwidmFsdWUiOlsiMSJdfSx7InR5cGUiOiJ1aW50OCIsInZhbHVlIjpbIjEyMyJdfSx7InR5cGUiOiJ1aW50NjRbXSIsInZhbHVlIjpbIjEiLCIyIiwiMyJdfSx7InR5cGUiOiJ1aW50MjU2W10iLCJ2YWx1ZSI6WyIxIiwiMiIsIjMiXX0seyJ0eXBlIjoidHVwbGUiLCJ2YWx1ZSI6WyJXM3NpZEhsd1pTSTZJbWx1ZERZMElpd2lkbUZzZFdVaU9sc2lOU0pkZlN4N0luUjVjR1VpT2lKcGJuUTJOQ0lzSW5aaGJIVmxJanBiSWpFd0lsMTlYUT09Il19LHsidHlwZSI6InR1cGxlIiwidmFsdWUiOlsiVzNzaWRIbHdaU0k2SW1sdWREWTBJaXdpZG1Gc2RXVWlPbHNpTlRBaVhYMHNleUowZVhCbElqb2lkSFZ3YkdWYlhTSXNJblpoYkhWbElqcGJJbGN6YzJsa1NHeDNXbE5KTmtsdGJIVmtSRmt3U1dsM2FXUnRSbk5rVjFWcFQyeHphVTFVV1dsWVdEQnpaWGxLTUdWWVFteEphbTlwWVZjMU1FNXFVV2xNUTBveVdWZDRNVnBUU1RaWGVVbDZUV2xLWkdaV01EMGlMQ0pYTTNOcFpFaHNkMXBUU1RaSmJXeDFaRVJaTUVscGQybGtiVVp6WkZkVmFVOXNjMmxPVTBwa1psTjROMGx1VWpWalIxVnBUMmxLY0dKdVVUSk9RMGx6U1c1YWFHSklWbXhKYW5CaVNXcEZkMGxzTVRsWVVUMDlJbDE5WFE9PSJdfSx7InR5cGUiOiJ0dXBsZVtdIiwidmFsdWUiOlsiVzNzaWRIbHdaU0k2SW1sdWREWTBJaXdpZG1Gc2RXVWlPbHNpTVRZaVhYMHNleUowZVhCbElqb2lhVzUwTmpRaUxDSjJZV3gxWlNJNld5SXpNaUpkZlYwPSIsIlczc2lkSGx3WlNJNkltbHVkRFkwSWl3aWRtRnNkV1VpT2xzaU5TSmRmU3g3SW5SNWNHVWlPaUpwYm5RMk5DSXNJblpoYkhWbElqcGJJakV3SWwxOVhRPT0iXX0seyJ0eXBlIjoidHVwbGVbXSIsInZhbHVlIjpbIlczc2lkSGx3WlNJNkltbHVkRFkwSWl3aWRtRnNkV1VpT2xzaU5UQWlYWDBzZXlKMGVYQmxJam9pZEhWd2JHVmJYU0lzSW5aaGJIVmxJanBiSWxjemMybGtTR3gzV2xOSk5rbHRiSFZrUkZrd1NXbDNhV1J0Um5Oa1YxVnBUMnh6YVUxVVdXbFlXREJ6WlhsS01HVllRbXhKYW05cFlWYzFNRTVxVVdsTVEwb3lXVmQ0TVZwVFNUWlhlVWw2VFdsS1pHWldNRDBpTENKWE0zTnBaRWhzZDFwVFNUWkpiV3gxWkVSWk1FbHBkMmxrYlVaelpGZFZhVTlzYzJsT1UwcGtabE40TjBsdVVqVmpSMVZwVDJsS2NHSnVVVEpPUTBselNXNWFhR0pJVm14SmFuQmlTV3BGZDBsc01UbFlVVDA5SWwxOVhRPT0iLCJXM3NpZEhsd1pTSTZJbWx1ZERZMElpd2lkbUZzZFdVaU9sc2lOVEFpWFgwc2V5SjBlWEJsSWpvaWRIVndiR1ZiWFNJc0luWmhiSFZsSWpwYklsY3pjMmxrU0d4M1dsTkpOa2x0YkhWa1JGa3dTV2wzYVdSdFJuTmtWMVZwVDJ4emFVMVVXV2xZV0RCelpYbEtNR1ZZUW14SmFtOXBZVmMxTUU1cVVXbE1RMG95V1ZkNE1WcFRTVFpYZVVsNlRXbEtaR1pXTUQwaUxDSlhNM05wWkVoc2QxcFRTVFpKYld4MVpFUlpNRWxwZDJsa2JVWnpaRmRWYVU5c2MybE9VMHBrWmxONE4wbHVValZqUjFWcFQybEtjR0p1VVRKT1EwbHpTVzVhYUdKSVZteEphbkJpU1dwRmQwbHNNVGxZVVQwOUlsMTlYUT09Il19LHsidHlwZSI6ImFkZHJlc3MiLCJ2YWx1ZSI6WyIwLjAuMTIzNDUiXX0seyJ0eXBlIjoidWludDY0IiwidmFsdWUiOlsiNTY3ODQ2NDU2NDUiXX0seyJ0eXBlIjoidWludDI1NiIsInZhbHVlIjpbIjEyMzQ1Il19XQ==");

            Debug.Log($"contractCallFunction(contractId = {contractId}, functionName = {functionName}, params = {parameters.encode()}, accountId = {accountId}, accountPrivateKey = {accountPrivateKey}, gas = {gas}, bladePayFee = {bladePayFee.ToString()})");
            throw new BladeSDKException("Stop", "no return");
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
