using NUnit.Framework;
using BladeLabs.UnitySDK;
using System;
using System.IO;


namespace BladeLabs.UnitySDK.Tests.Runtime
{
    /// <summary>
    /// Tests for BladeSDK 
    /// </summary>
    public class BladeSDKTests
    {
        string apiKey = "ygUgCzRrsvhWmb3dsLcDpGnJpSZ4tk8hACmZqg9WngpuQYKdnD5m8FjfPV3XVUeB";
        Network network = Network.Testnet;
        string dAppCode = "unitysdktest";
        SdkEnvironment sdkEnvironment = SdkEnvironment.CI;
        string executeApiEndpoint = "http://localhost:8443";
        string accountId0 = "0.0.346533";

        // Assert.AreEqual("", info.sdkVersion, "OMG! Hack instead of Debug.Log" );


        // [UnitySetUp]
        // public void SetUp()
        // {
        //     bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment, executeApiEndpoint);
        // }


        [Test]
        public async void GetInfo() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment, executeApiEndpoint);
            InfoData info = await bladeSdk.getInfo();

            Assert.AreEqual(info.apiKey, apiKey);
            Assert.AreEqual(info.dAppCode, dAppCode);
            Assert.AreEqual(info.network, network);
            Assert.AreEqual(info.sdkEnvironment, sdkEnvironment);
            Assert.AreEqual(string.IsNullOrEmpty(info.sdkVersion), false);        
        }

        [Test]
        public async void GetAccountInfo() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment, executeApiEndpoint);
            AccountInfoData accountInfo = await bladeSdk.getAccountInfo(accountId0);

            Assert.AreEqual(accountInfo.accountId, accountId0);
            Assert.AreEqual(string.IsNullOrEmpty(accountInfo.evmAddress), false);
            Assert.AreEqual(string.IsNullOrEmpty(accountInfo.calculatedEvmAddress), false);
        } 

        [Test]
        public async void GetBalance() {
            BladeSDK bladeSdk = new BladeSDK(apiKey, network, dAppCode, sdkEnvironment, executeApiEndpoint);
            AccountBalanceData accountBalance = await bladeSdk.getBalance(accountId0);

            Assert.AreEqual(accountBalance.balance > 0, true);
            Assert.AreEqual(accountBalance.tokens.Count > 0, true);
        }        
    }
}
