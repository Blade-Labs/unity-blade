using BladeLabs.UnitySDK;
using UnityEngine;
using System.Collections.Generic;

namespace BladeLabs.UnitySDK.Samples
{
    class UsageBladeSDKSample : MonoBehaviour
    {
        async void Start()
        {
            BladeSDK bladeSdk = new BladeSDK("ygUgCzRrsvhWmb3dsLcDpGnJpSZ4tk8hACmZqg9WngpuQYKdnD5m8FjfPV3XVUeB", Network.Testnet, "unitysdktest", SdkEnvironment.CI, "http://localhost:8443");

            // get info
            // Debug.Log(await bladeSdk.getInfo());

            // get account info
            // Debug.Log(await bladeSdk.getAccountInfo("0.0.346533"));

            // get balance
            // Debug.Log(await bladeSdk.getBalance("0.0.346533"));

            // transfer hbars
            // Debug.Log(
            //     await bladeSdk.transferHbars(
            //         "0.0.8172",
            //         "3030020100300706052b8104000a042204200ce23c3a1cc9b7cd85db5fdd039491cab3d95c0065ac18f77867d33eaff5c050",
            //         "0.0.14943230",
            //         "15",
            //         "unity-sdk-test-ok"
            //     )
            // );
            
            // transfer tokens
            // Debug.Log(
            //     await bladeSdk.transferTokens(
            //         "0.0.416487",
            //         "0.0.346533",
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b",
            //         "0.0.346530",
            //          "0.12345678",
            //         "unity-sdk-paid-token-transfer",
            //         false
            //     )
            // );

            // free transfer tokens
            // Debug.Log(
            //     await bladeSdk.transferTokens(
            //         "0.0.416487",
            //         "0.0.346533",
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b",
            //         "0.0.346530",
            //          "0.12345678",
            //         "unity-sdk-free-token-transfer",
            //         true
            //     )
            // );
            
            // create account (without mnemonic)
            //  Debug.Log(
                // await bladeSdk.createAccount("some device id string, if required")
            // );

            // contract call
            // ContractFunctionParameters parameters = new ContractFunctionParameters();
            // parameters.addString("Hello Unity SDK");
            // Debug.Log(
            //     await bladeSdk.contractCallFunction(
            //         "0.0.416245", 
            //         "set_message", 
            //         parameters, 
            //         "0.0.346533", 
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", 
            //         150000,
            //         false
            //     )
            // );


            // contract call (Blade pay fee)
            // ContractFunctionParameters parameters = new ContractFunctionParameters();
            // parameters.addString("Hello Unity SDK");
            // Debug.Log(
            //     await bladeSdk.contractCallFunction(
            //         "0.0.416245", 
            //         "set_message", 
            //         parameters, 
            //         "0.0.346533", 
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", 
            //         150000,
            //         true
            //     )
            // );


            // contract call query 
            Debug.Log(
                await bladeSdk.contractCallQueryFunction(
                    "0.0.416245", 
                    "get_message", 
                    new ContractFunctionParameters(), 
                    "0.0.346533", 
                    "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", 
                    150000, // gas
                    70000000, // tinybars
                    new List<string> {"string", "int32"}
                )
            );




        }
    }
}