using System;

namespace BladeLabs.UnitySDK
{
    [Serializable]
    public struct ResponseObject
    {
        public string status;
    }



    [Serializable]
    public class BladeJSError
    {
        public string name;
        public string reason;
    }

    [Serializable]
    public class Response<T>
    {
        // public string completionKey;
        public T data;
        public BladeJSError error;
    }

    [Serializable]
    public class SignedTx
    {
        public string tx;
        public string network;
    }

    [Serializable]
    public class InfoData
    {
        // temporary
        public string apiKey;
        public string dAppCode;
        public string network;
        public string visitorId;
        public string sdkEnvironment;
        public string sdkVersion;
        public int nonce;
    }

    // [Serializable]
    // public class BalanceResponse : Result<BalanceData>
    // {
    //     public string completionKey { get; set; }
    //     public BalanceData data { get; set; }
    // }

    // [Serializable]
    // public class BalanceData
    // {
    //     public double hbars;
    //     public List<BalanceDataToken> tokens;
    // }

    // [Serializable]
    // public class BalanceDataToken
    // {
    //     public double balance;
    //     public string tokenId;
    // }

}
