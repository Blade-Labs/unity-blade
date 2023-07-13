using BladeLabs.UnitySDK;
using UnityEngine;

namespace BladeLabs.UnitySDK.Samples
{
    class UsageBladeSDKSample : MonoBehaviour
    {
        async void Start()
        {
            BladeSDK bladeSdk = new BladeSDK();

            Debug.Log("BladeSDK()");

            var res = await bladeSdk.transferHbars(
                "0.0.8172",
                "3030020100300706052b8104000a042204200ce23c3a1cc9b7cd85db5fdd039491cab3d95c0065ac18f77867d33eaff5c050",
                "0.0.14943230",
                "15",
                "unity-sdk-test-ok"
            );

            Debug.Log(res);

        }
    }
}