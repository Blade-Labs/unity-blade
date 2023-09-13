using NUnit.Framework;
using BladeLabs.UnitySDK;
using System;
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace BladeLabs.UnitySDK.Tests.Runtime
{
    /// <summary>
    /// Tests for BladeSDK 
    /// </summary>
    public class BladeSDKTests
    {
        string apiKey = "Rww3x27z3Q9rrIvRQ6qGgIRppxz5e5HHPWdARyxnMXpe77WD5MW39REBXXvRZsZE";
        Network network = Network.Testnet;
        string dAppCode = "unitysdktest";
        SdkEnvironment sdkEnvironment = SdkEnvironment.CI;
        string accountId0 = "0.0.346533";
        string accountId0Private = "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b";
        string accountId0Public = "302d300706052b8104000a032200029dc73991b0d9cdbb59b2cd0a97a0eaff6de801726cb39804ea9461df6be2dd30";

        string accountId1 = "0.0.346530";

        string token0DApp = "0.0.433870";
        string token1 = "0.0.416487";

        string contractId = "0.0.416245";

        // Assert.AreEqual("", info.sdkVersion, "OMG! Hack instead of Debug.Log" );

        [UnityTest]
        public IEnumerator GetInfo() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task =  bladeSdk.getInfo();
            while (!task.IsCompleted) {
                yield return null; // Wait for the next frame
            }
            InfoData info = task.Result;

            Assert.AreEqual(info.apiKey, apiKey);
            Assert.AreEqual(info.dAppCode, dAppCode);
            Assert.AreEqual(info.network, network);
            Assert.AreEqual(info.sdkEnvironment, sdkEnvironment);
            Assert.AreEqual(string.IsNullOrEmpty(info.sdkVersion), false);        
        }

        [UnityTest]
        public IEnumerator GetAccountInfo() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.getAccountInfo(accountId0);
            while (!task.IsCompleted) {
                yield return null;
            }
            AccountInfoData accountInfo = task.Result;

            Assert.AreEqual(accountInfo.accountId, accountId0);
            Assert.AreEqual(string.IsNullOrEmpty(accountInfo.evmAddress), false);
            Assert.AreEqual(string.IsNullOrEmpty(accountInfo.calculatedEvmAddress), false);
        } 

        [UnityTest]
        public IEnumerator GetBalance() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.getBalance(accountId0);
            while (!task.IsCompleted) {
                yield return null;
            }
            AccountBalanceData accountBalance = task.Result;

            Assert.AreEqual(accountBalance.balance > 0, true);
            Assert.AreEqual(accountBalance.tokens.Count > 0, true);
        }

        [UnityTest]
        public IEnumerator TransferHbars() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.transferHbars(
                accountId0,
                accountId0Private,
                accountId1,
                "15",
                "unity-sdk-tests"
            );

            while (!task.IsCompleted) {
                yield return null;
            }
            ExecuteTxReceipt receipt = task.Result;
            Assert.AreEqual(receipt.status, "SUCCESS");
        }  

        [UnityTest]
        public IEnumerator TransferTokens() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.transferTokens(
                token1,
                accountId0,
                accountId0Private,
                accountId1,
                "0.12345678",
                "unity-sdk-tests-paid-token-transfer",
                false
            );

            while (!task.IsCompleted) {
                yield return null;
            }
            ExecuteTxReceipt receipt = task.Result;

            Assert.AreEqual(receipt.status, "SUCCESS");
        }

        [UnityTest]
        public IEnumerator TransferTokensNoFee() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.transferTokens(
                token0DApp, /// token id assigned on server side for dAppCode
                accountId0,
                accountId0Private,
                accountId1,
                "1",
                "unity-sdk-tests-free-token-transfer",
                true
            );

            while (!task.IsCompleted) {
                yield return null;
            }
            ExecuteTxReceipt receipt = task.Result;

            Assert.AreEqual(receipt.status, "SUCCESS");
        }

        [UnityTest]
        public IEnumerator CreateAndDeleteAccount() {
            bool status = true;
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.createAccount("some device id, if configured");

            while (!task.IsCompleted) {
                yield return null;
            }
            CreateAccountData createAccountData = task.Result;

            if (createAccountData.status != "SUCCESS") {
                Assert.AreEqual(createAccountData.status, "SUCCESS");
                throw new Exception("failed to create account");
            }

            var taskDelete = bladeSdk.deleteAccount(
                createAccountData.accountId,
                createAccountData.privateKey,
                accountId0, // transferAccountId
                accountId0,
                accountId0Private
            );
            while (!taskDelete.IsCompleted) {
                yield return null;
            }
            ExecuteTxReceipt receipt = taskDelete.Result;

            Assert.AreEqual(receipt.status, "SUCCESS");            
        }

        [UnityTest]
        public IEnumerator ContractCallFunction() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);

            ContractFunctionParameters parameters = new ContractFunctionParameters().addString("Hello Unity SDK [tests]");
            var task = bladeSdk.contractCallFunction(
                contractId,
                "set_message", 
                parameters,
                accountId0,
                accountId0Private,
                150000,
                false
            );

            while (!task.IsCompleted) {
                yield return null;
            }
            ExecuteTxReceipt receipt = task.Result;
            Assert.AreEqual(receipt.status, "SUCCESS");
        } 

        [UnityTest]
        public IEnumerator ContractCallFunctionNoFee() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);

            ContractFunctionParameters parameters = new ContractFunctionParameters().addString("Hello Unity SDK [tests no fee]");
            var task = bladeSdk.contractCallFunction(
                contractId,
                "set_message", 
                parameters,
                accountId0,
                accountId0Private,
                150000,
                true
            );

            while (!task.IsCompleted) {
                yield return null;
            }
            ExecuteTxReceipt receipt = task.Result;
            Assert.AreEqual(receipt.status, "SUCCESS");
        }


        [UnityTest]
        public IEnumerator ContractCallQueryFunction() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.contractCallQueryFunction(
                contractId,
                "get_message", 
                new ContractFunctionParameters(),
                accountId0,
                accountId0Private,
                150000,
                70000000,
                new List<string> {"string", "int32"}
            );

            while (!task.IsCompleted) {
                yield return null;
            }
            ContractQueryData contractQueryData = task.Result;
            Assert.AreEqual(contractQueryData.gasUsed > 100000 && contractQueryData.values.Count == 2, true);
        }

        [UnityTest]
        public IEnumerator GetC14Url() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.getC14url(
                "HBAR",
                accountId0, 
                "200"
            );
            while (!task.IsCompleted) {
                yield return null;
            }
            string url = task.Result;
            Assert.AreEqual(url.Contains("targetAssetId") && url.Contains("sourceAmount") && url.Contains("targetAddress"), true);
        }

        [UnityTest]
        public IEnumerator SignAndVerify() {
            var signature = "27cb9d51434cf1e76d7ac515b19442c619f641e6fccddbf4a3756b14466becb6992dc1d2a82268018147141fc8d66ff9ade43b7f78c176d070a66372d655f942";


            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var taskSign1 = bladeSdk.sign("hello", accountId0Private, "utf8");
            while (!taskSign1.IsCompleted) {
                yield return null;
            }
            string signedMessage1 = taskSign1.Result.signedMessage;

            var taskSign2 = bladeSdk.sign("aGVsbG8=", accountId0Private, "base64");
            while (!taskSign2.IsCompleted) {
                yield return null;
            }
            string signedMessage2 = taskSign2.Result.signedMessage;


            var taskVerify1 = bladeSdk.signVerify("hello", signature, accountId0Public, "utf8");
            while (!taskVerify1.IsCompleted) {
                yield return null;
            }
            bool verification1 = taskVerify1.Result;

            var taskVerify2 = bladeSdk.signVerify("aGVsbG8=", signature, accountId0Public, "base64");
            while (!taskVerify2.IsCompleted) {
                yield return null;
            }
            bool verification2 = taskVerify2.Result;

            var taskVerify3 = bladeSdk.signVerify("signature will not match", signature, accountId0Public, "utf8");
            while (!taskVerify3.IsCompleted) {
                yield return null;
            }
            bool verification3 = taskVerify3.Result;

            Assert.AreEqual(true, signature == signedMessage1 && signedMessage1 ==  signedMessage2 && verification1 && verification2 && !verification3);
        }

        [UnityTest]
        public IEnumerator HethersSign() {
            var signature = "0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c11217b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df3741561b";

            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var taskSign1 = bladeSdk.hethersSign("hello", accountId0Private, "utf8");
            while (!taskSign1.IsCompleted) {
                yield return null;
            }
            string signedMessage1 = taskSign1.Result.signedMessage;

            Assert.AreEqual(true, signature == signedMessage1);
        }

        [UnityTest]
        public IEnumerator SplitSignature() {
            var signature = "0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c11217b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df3741561b";

            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);
            var task = bladeSdk.splitSignature(signature);
            while (!task.IsCompleted) {
                yield return null;
            }
            SplitSignatureData splitSignatureData = task.Result;

            Assert.AreEqual(true, splitSignatureData.v == 27
                                    && splitSignatureData.r == "0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c1121"
                                    && splitSignatureData.s == "0x7b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df374156"
            );
        }

        [UnityTest]
        public IEnumerator GetParamsSignature() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);

            ContractFunctionParameters parameters = new ContractFunctionParameters()
                .addAddress(accountId0)
                .addUInt64Array(new List<ulong> {300000, 300000})
                .addUInt64Array(new List<ulong> {6})
                .addUInt64Array(new List<ulong> {2})
            ;

            var task = bladeSdk.getParamsSignature(parameters, accountId0Private);
            while (!task.IsCompleted) {
                yield return null;
            }
            SplitSignatureData splitSignatureData = task.Result;

            Assert.AreEqual(true, splitSignatureData.v == 28
                                    && splitSignatureData.r == "0xe5e662d0564828fd18b2b5b228ade288ad063fadca76812f7902f56cae3e678e"
                                    && splitSignatureData.s == "0x61b7ceb82dc6695872289b697a1bca73b81c494288abda29fa022bb7b80c84b5"
            );
        }

        [UnityTest]
        public IEnumerator GetTransactions() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment);

            var task = bladeSdk.getTransactions(accountId0, "CRYPTOTRANSFER", "", 5);
            while (!task.IsCompleted) {
                yield return null;
            }
            TransactionsHistoryData transactionsHistoryData = task.Result;
            Assert.AreEqual(true, !string.IsNullOrEmpty(transactionsHistoryData.nextPage) && transactionsHistoryData.transactions.Count > 0);
        }
    }
}
