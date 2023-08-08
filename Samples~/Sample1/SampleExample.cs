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
            // parameters.addString("Hello Unity SDK [self pay]");
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
            // parameters.addString("Hello Unity SDK [Blade pay]");
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


            // contract call query (self pay)
            // Debug.Log(
            //     await bladeSdk.contractCallQueryFunction(
            //         "0.0.416245", 
            //         "get_message", 
            //         new ContractFunctionParameters(), 
            //         "0.0.346533", 
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", 
            //         150000, // gas
            //         70000000, // tinybars
            //         new List<string> {"string", "int32"}
            //     )
            // );

            // C14 url
            // Debug.Log(
            //     await bladeSdk.getC14url("karate", "0.0.123456", "1234")
            // );
            
            // sign
            // Debug.Log(
            //     // await bladeSdk.sign("aGVsbG8=", "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", "base64")
            //     await bladeSdk.sign("hello", "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", "utf8")
            // );
            // Debug.Log("signedMessage: '27cb9d51434cf1e76d7ac515b19442c619f641e6fccddbf4a3756b14466becb6992dc1d2a82268018147141fc8d66ff9ade43b7f78c176d070a66372d655f942'");


            // sign verify
            // Debug.Log(await bladeSdk.signVerify("hello", "27cb9d51434cf1e76d7ac515b19442c619f641e6fccddbf4a3756b14466becb6992dc1d2a82268018147141fc8d66ff9ade43b7f78c176d070a66372d655f942", "302d300706052b8104000a032200029dc73991b0d9cdbb59b2cd0a97a0eaff6de801726cb39804ea9461df6be2dd30", "utf8"));
            // Debug.Log(await bladeSdk.signVerify("aGVsbG8=", "27cb9d51434cf1e76d7ac515b19442c619f641e6fccddbf4a3756b14466becb6992dc1d2a82268018147141fc8d66ff9ade43b7f78c176d070a66372d655f942", "302d300706052b8104000a032200029dc73991b0d9cdbb59b2cd0a97a0eaff6de801726cb39804ea9461df6be2dd30", "base64"));
            // Debug.Log(await bladeSdk.signVerify("signature will not match", "27cb9d51434cf1e76d7ac515b19442c619f641e6fccddbf4a3756b14466becb6992dc1d2a82268018147141fc8d66ff9ade43b7f78c176d070a66372d655f942", "302d300706052b8104000a032200029dc73991b0d9cdbb59b2cd0a97a0eaff6de801726cb39804ea9461df6be2dd30", "utf8"));

            // splitSignature
            Debug.Log(await bladeSdk.splitSignature("0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c11217b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df3741561b"));
            Debug.Log("v: 27, r: '0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c1121', s: '0x7b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df374156'");
  

        }
    }
}