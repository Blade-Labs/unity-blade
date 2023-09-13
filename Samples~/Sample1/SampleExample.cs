using BladeLabs.UnitySDK;
using UnityEngine;
using System.Collections.Generic;

namespace BladeLabs.UnitySDK.Samples
{
    class UsageBladeSDKSample : MonoBehaviour
    {
        async void Start()
        {
            BladeSDK bladeSdk = new BladeSDK("Rww3x27z3Q9rrIvRQ6qGgIRppxz5e5HHPWdARyxnMXpe77WD5MW39REBXXvRZsZE", Network.Testnet, "unitysdktest", SdkEnvironment.CI);

            // get info
            // Debug.Log(await bladeSdk.getInfo());

            // get account info
            // Debug.Log(await bladeSdk.getAccountInfo("0.0.346533"));

            // get balance
            // Debug.Log(await bladeSdk.getBalance("0.0.346533"));

            // transfer hbars
            // Debug.Log(
            //     await bladeSdk.transferHbars(
            //         "0.0.346533",
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b",
            //         "0.0.346530",
            //         "15",
            //         "unity-sdk-test-ok"
            //     )
            // );
            
            // transfer tokens
            // Debug.Log(
            //     await bladeSdk.transferTokens(
            //         "0.0.433870",
            //         "0.0.346533",
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b",
            //         "0.0.346530",
            //          "1",
            //         "unity-sdk-paid-token-transfer",
            //         false
            //     )
            // );

            // free transfer tokens
            // Debug.Log(
            //     await bladeSdk.transferTokens(
            //         "0.0.433870", /// token id assigned on server side for dAppCode
            //         "0.0.346533",
            //         "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b",
            //         "0.0.346530",
            //          "1",
            //         "unity-sdk-free-token-transfer",
            //         true
            //     )
            // );
            
            // create account (without mnemonic)
            // Debug.Log(
            //     await bladeSdk.createAccount("some device id string, if required")
            // );

            // delete account
            // CreateAccountData accountOperator = new CreateAccountData {accountId = "0.0.346533", privateKey = "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b"};
            // CreateAccountData accountToDelete = new CreateAccountData {accountId = "0.0.486856", privateKey = "3030020100300706052b8104000a04220420ff3ee87eac8651f1aca58570b7aa33b1809bd956f13734d29767e97385cb9f15"};

            // Debug.Log(
            //     await bladeSdk.deleteAccount(
            //         accountToDelete.accountId,
            //         accountToDelete.privateKey,
            //         accountOperator.accountId, // transferAccountId
            //         accountOperator.accountId,
            //         accountOperator.privateKey
            //     )
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

            // hethersSign
            // Debug.Log(await bladeSdk.hethersSign("hello", "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b", "utf8"));
            // Debug.Log("0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c11217b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df3741561b");

            // splitSignature
            // Debug.Log(await bladeSdk.splitSignature("0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c11217b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df3741561b"));
            // Debug.Log("v: 27, r: '0x25de7c26ecfa4f28d8b96a95cf58ea7088a72a66b311c796090cb4c7d58c1121', s: '0x7b4a7b174b4c31b90c3babb00958b2120274380404c4f1196abe3614df374156'");
  

            // getParamsSignature
            // ContractFunctionParameters parameters = new ContractFunctionParameters();
            // parameters
            //     .addAddress("0.0.346533")
            //     .addUInt64Array(new List<ulong> {300000, 300000})
            //     .addUInt64Array(new List<ulong> {6})
            //     .addUInt64Array(new List<ulong> {2})
            // ;
            // Debug.Log(await bladeSdk.getParamsSignature(parameters, "3030020100300706052b8104000a04220420ebccecef769bb5597d0009123a0fd96d2cdbe041c2a2da937aaf8bdc8731799b"));
            // Debug.Log("v: 28, r: '0xe5e662d0564828fd18b2b5b228ade288ad063fadca76812f7902f56cae3e678e', s: '0x61b7ceb82dc6695872289b697a1bca73b81c494288abda29fa022bb7b80c84b5'");

            //get transaction history
            // Debug.Log(await bladeSdk.getTransactions("0.0.346533", "CRYPTOTRANSFER", "", 5));
            // Debug.Log(await bladeSdk.getTransactions("0.0.346533", "CRYPTOCREATEACCOUNT", "", 20));
            // Debug.Log(await bladeSdk.getTransactions("0.0.346533", "", nextPage, 5));
            
        }
    }
}