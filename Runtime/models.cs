using System;

namespace BladeLabs.UnitySDK
{
    [Serializable]
    public struct ExecuteTxReceipt
    {
        public string status;
        public string accountId;
        public string fileId;
        public string contractId;
        public string topicId;
        public string tokenId;
        public string scheduleId;
        public ExchangeRate exchangeRate;
        public Long topicSequenceNumber;
        //   public topicRunningHash: Uint8Array(0) [],
        public Long totalSupply;
        public string scheduledTransactionId;
        public string[] serials;
        public string[] duplicates;
        public string[] children;

        public override string ToString() {
            return $"{{status = {status}, accountId = {accountId}, fileId = {fileId}, contractId = {contractId}, " +
                   $"topicId = {topicId}, tokenId = {tokenId}, scheduleId = {scheduleId}, " +
                   $"exchangeRate = {exchangeRate}, topicSequenceNumber = {topicSequenceNumber}, " +
                   $"totalSupply = {totalSupply}, scheduledTransactionId = {scheduledTransactionId}, " +
                   $"serials = [{string.Join(", ", serials)}], duplicates = [{string.Join(", ", duplicates)}], " +
                   $"children = [{string.Join(", ", children)}]}}";
        }
    }

    [Serializable]
    public class Long
    {
        public uint low;
        public uint high;
        public bool unsigned;

        public override string ToString() {
            return $@"{{low = {low}, high = {high}, unsigned = {unsigned}}}";
        }
    }

    [Serializable]
    public class ExchangeRate
    {
        public int hbars;
        public int cents;
        public string expirationTime;
        public double exchangeRateInCents;

        public override string ToString() {
            return $@"{{hbars = {hbars}, cents = {cents}, expirationTime = {expirationTime}, exchangeRateInCents = {exchangeRateInCents}}}";
        }
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
    public class ExecuteTxRequest
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
