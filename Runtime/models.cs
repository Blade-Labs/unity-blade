using System;
using System.Collections.Generic;

namespace BladeLabs.UnitySDK {
    [Serializable]
    public class InfoData {
        public string apiKey;
        public string dAppCode;
        public Network network;
        public string visitorId;
        public SdkEnvironment sdkEnvironment;
        public string sdkVersion;
        
        public override string ToString() {
            return $@"{{apiKey = {apiKey}, dAppCode = {dAppCode}, network = {network}, visitorId = {visitorId}, sdkEnvironment = {sdkEnvironment}, sdkVersion = {sdkVersion}}}";
        }
    }

    [Serializable]
    public class AccountInfoData {
        public string accountId;
        public string evmAddress;
        public string calculatedEvmAddress;
                
        public override string ToString() {
            return $@"{{accountId = {accountId}, evmAddress = {evmAddress}, calculatedEvmAddress = {calculatedEvmAddress}}}";
        }
    }


    [Serializable]
    public struct ExecuteTxReceipt {
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
    public class Long {
        public uint low;
        public uint high;
        public bool unsigned;

        public override string ToString() {
            return $@"{{low = {low}, high = {high}, unsigned = {unsigned}}}";
        }
    }

    [Serializable]
    public class ExchangeRate {
        public int hbars;
        public int cents;
        public string expirationTime;
        public double exchangeRateInCents;

        public override string ToString() {
            return $@"{{hbars = {hbars}, cents = {cents}, expirationTime = {expirationTime}, exchangeRateInCents = {exchangeRateInCents}}}";
        }
    }

    [Serializable]
    public class BladeJSError {
        public string name;
        public string reason;
    }

    [Serializable]
    public class Response<T> {
        // public string completionKey;
        public T data;
        public BladeJSError error;
    }

    [Serializable]
    public class SignedTx {
        public string tx;
        public string network;
    }

    [Serializable]
    public class ExecuteTxRequest {
        public string tx;
        public string network;
    }

    [Serializable]
    public class FreeTokenTransferRequest {
        public string receiverAccountId;
        public string senderAccountId;
        public double amount;
        public int? decimals;
        public string memo;

        public override string ToString() {
            return $@"{{receiverAccountId = {receiverAccountId}, senderAccountId = {senderAccountId}, amount = {amount}, decimals = {decimals}, memo = {memo}}}";
        }
    };

    [Serializable]
    public class CreateAccountRequest {
        public string publicKey;
    }

    [Serializable] 
    public class ConfirmAccountRequest {
        public string id;
    }

    [Serializable]
    public class FreeTokenTransferResponse {
        public string transactionBytes;

        public override string ToString() {
            return $@"{{transactionBytes = {transactionBytes}}}";
        }
    }

    [Serializable]
    public class CreateAccountResponse {
        public string id;
        public string network;
        public int maxAutoTokenAssociation;
        public string associationPresetTokenStatus;
        public string transactionBytes;
        public string updateAccountTransactionBytes;
        public string transactionId;

        public override string ToString() {
            return $@"{{id = {id}, network = {network}, maxAutoTokenAssociation = {maxAutoTokenAssociation}, associationPresetTokenStatus = {associationPresetTokenStatus}, transactionBytes = {transactionBytes}, updateAccountTransactionBytes = {updateAccountTransactionBytes}, transactionId = {transactionId}}}";
        }
    }

    [Serializable]
    public class CreateAccountData {
        public string transactionId;
        public string status;
        public string seedPhrase;
        public string publicKey;
        public string privateKey;
        public string accountId;
        public string evmAddress;

        public override string ToString() {
            return $@"{{transactionId = {transactionId}, status = {status}, seedPhrase = {seedPhrase}, publicKey = {publicKey}, privateKey = {privateKey}, accountId = {accountId}, evmAddress = {evmAddress}}}";
        }
    }
    
    [Serializable]
    public class TVTEResponse {
        public string tvte;

        public override string ToString() {
            return $@"{{tvte = {tvte}}}";
        }
    }

    [Serializable]
    public class AccountData {
        public string account;
        public string alias;
        public ulong auto_renew_period;
        public AccountBalanceData balance;
        public string created_timestamp;
        public bool decline_reward;
        public bool deleted;
        public ulong ethereum_nonce;
        public string evm_address;
        public string expiry_timestamp;
        public KeyData key;
        public PaginationLink links;
        public ulong max_automatic_token_associations;
        public string memo;
        public ulong pending_reward;
        public bool receiver_sig_required;
        public string stake_period_start;
        public string staked_account_id;
        public string staked_node_id;
        public string[] transactions; /////

        public override string ToString() {
            return $@"{{account = {account}, alias = {alias}, balance = {balance}, evm_address = {evm_address}, key = {key}, memo = {memo}, transactions = {transactions}}}";
        }
    }

    [Serializable]
    public class TokenData {
        public string name;
        public string token_id;
        public string symbol;
        public string decimals;
        public string memo;
        public string auto_renew_account;
        public ulong auto_renew_period;
        public string created_timestamp;
        // custom_fees":{
        //     "created_timestamp":"1678732798.485340406",
        //     "fixed_fees":[],
        //     "fractional_fees":[]
        // },
        
        public bool deleted;
        public ulong expiry_timestamp;
        public KeyData admin_key;
        public KeyData fee_schedule_key;
        public KeyData freeze_key;
        public KeyData kyc_key;
        public KeyData pause_key;
        public KeyData supply_key;
        public KeyData wipe_key;
        public bool freeze_default;
        public string initial_supply;
        public string max_supply;
        public string modified_timestamp;
        public string pause_status;
        public string supply_type;
        public string total_supply;
        public string treasury_account_id;
        public string type;

        public override string ToString() {
            return $@"{{token_id = {token_id}, name = {name}, symbol = {symbol}, decimals = {decimals}}}";
        }
    }

    [Serializable]
    public class KeyData {
        public string key;
        public string _type;
    
        public override string ToString() {
            return $@"{{key = {key}, _type = {_type}}}";
        }
    }

    [Serializable]
    public class KeyPairData {
        public string privateKey;
        public string publicKey;
        public string evmAddress;
    
        public override string ToString() {
            return $@"{{privateKey = {privateKey}, publicKey = {publicKey}, evmAddress = {evmAddress}}}";
        }
    }

    [Serializable]
    public class AccountBalanceData {
        public long balance;
        public string timestamp;
        public List<TokenBalance> tokens;

        public override string ToString() {
            return $@"{{balance = {balance}, timestamp = {timestamp}, tokens = [{string.Join<TokenBalance>(", ", tokens)}]}}";
        }
    }

    [Serializable]
    public class TokenBalance {
        public string token_id;
        public ulong balance;

        public override string ToString() {
            return $@"{{token_id = {token_id}, balance = {balance}}}";
        }
    }

    [Serializable]
    public class PaginationLink {
        public string next;

        public override string ToString() {
            return $@"{{next = {next}}}";
        }
    }

    [Serializable]
    public class BalanceDataResponse {
        public List<TokenBalance> tokens;
        public PaginationLink links;

        public override string ToString() {
            return $@"{{tokens = [{string.Join(", ", tokens)}], links = {links}}}";
        }
    }

    [Serializable]
    public class ContractFunctionParameter {
        public string type;
        public List<string> value;

        public ContractFunctionParameter(string type, List<string> value) {
            this.type = type;
            this.value = value;
        }

        public override string ToString() {
            return $@"{{type = {type}, value = [{string.Join(", ", value)}]}}";
        }
    }
}
