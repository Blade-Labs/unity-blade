# Public methods 📢

## Get SDK-instance info

### Returns: 

* an instance of `InfoData` containing SDK-related properties.

```cs
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
```

## Get balances by Hedera id (address)

### Parameters:

* `accountId`: Hedera id (address), example: 0.0.112233

### Returns: 

* an instance of `AccountBalanceData` containing account hbar and token balances. 

```cs
public async Task<AccountBalanceData> getBalance(string accountId) {
    return await apiService.getBalance(accountId);
}
```

## Get account evmAddress and calculated evmAddress from public key

### Parameters:

* `accountId`: Hedera id (address), example: 0.0.112233

### Returns: 

* an instance of `AccountInfoData` containing account id, evmAddress, calculatedEvmAddress

```cs
public async Task<AccountInfoData> getAccountInfo(string accountId) {
    var account = await apiService.GET<AccountData>($"/api/v1/accounts/{accountId}");

    string response = engine
        .Evaluate($"window.bladeSdk.getAccountInfo('{accountId}', '{account.evm_address}', '{account.key.key}')")
        .UnwrapIfPromise()
        .ToString();

    return this.processResponse<AccountInfoData>(response);
}
```


## Method to execute Hbar transfers from current account to receiver

### Parameters:

* `accountId`: sender
* `accountPrivateKey`: sender's private key to sign transfer transaction
* `receiverId`: receiver
* `amount`: amount
* `memo`: memo (limited to 100 characters)

### Returns: 

* an instance of `ExecuteTxReceipt` containing transaction receipt

```cs
public async Task<ExecuteTxReceipt> transferHbars(string accountId, string accountPrivateKey, string recieverAccount, string amount, string memo) {
    string response = engine
        .Evaluate($"window.bladeSdk.transferHbars('{accountId}', '{accountPrivateKey}', '{recieverAccount}', '{amount}', '{memo}')")
        .UnwrapIfPromise()
        .ToString();

    SignedTx signedTx = this.processResponse<SignedTx>(response);
    return await apiService.executeTx(signedTx.tx, signedTx.network);
}
```


## Method to execute token transfers from current account to receiver

### Parameters:

* `tokenId`: token
* `accountId`: sender
* `accountPrivateKey`: sender's private key to sign transfer transaction
* `receiverId`: receiver
* `amount`: amount
* `memo`: memo (limited to 100 characters)
* `freeTransfer`: for tokens configured for this dAppCode on Blade backend

### Returns: 

* an instance of `ExecuteTxReceipt` containing transaction receipt

```cs
public async Task<ExecuteTxReceipt> transferTokens(string tokenId, string accountId, string accountPrivateKey, string recieverAccount, string amount, string memo, bool freeTransfer = false) {
    TokenData meta = await apiService.GET<TokenData>($"/api/v1/tokens/{tokenId}");
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
```


## Method to create Hedera account

### Parameters:

* `deviceId`: unique device id (advanced security feature, required only for some dApps)

### Returns: 

* an instance of `CreateAccountData` containing new account data

```cs
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
```


## Method to delete Hedera account

### Parameters:

* `deleteAccountId`: account to delete - id
* `deletePrivateKey`: account to delete - private key
* `transferAccountId`: The ID of the account to transfer the remaining funds to.
* `operatorAccountId`: operator account Id
* `operatorPrivateKey`: operator account private key

### Returns: 

* an instance of `ExecuteTxReceipt` containing transaction receipt

```cs
public async Task<ExecuteTxReceipt> deleteAccount(string deleteAccountId, string deletePrivateKey, string transferAccountId, string operatorAccountId, string operatorPrivateKey) {
    string signedTxResponse = engine
        .Evaluate($"window.bladeSdk.deleteAccount('{deleteAccountId}', '{deletePrivateKey}', '{transferAccountId}', '{operatorAccountId}', '{operatorPrivateKey}')")
        .UnwrapIfPromise()
        .ToString();
    SignedTx signedTx = this.processResponse<SignedTx>(signedTxResponse);
    return await apiService.executeTx(signedTx.tx, signedTx.network);
}
```


## Method to call smart-contract function

### Parameters:

* `contractId`: contract id
* `functionName`: contract function name
* `params`: function arguments (instance of ContractFunctionParameters)
* `accountId`: sender account id
* `accountPrivateKey`: sender's private key to sign transfer transaction
* `gas`: gas amount for transaction (default 100000)
* `bladePayFee`: blade pay fee, otherwise fee will be pay from sender accountId

### Returns: 

* an instance of `ExecuteTxReceipt` containing transaction receipt

```cs
public async Task<ExecuteTxReceipt> contractCallFunction(string contractId, string functionName, ContractFunctionParameters parameters, string accountId, string accountPrivateKey, uint gas, bool bladePayFee = false) {
    if (bladePayFee) {
        string contractCallBytecodeResponse = engine
            .Evaluate($"window.bladeSdk.getContractCallBytecode('{functionName}', '{parameters.encode()}')")
            .UnwrapIfPromise()
            .ToString();
        ContractCallBytecode contractCallBytecode = this.processResponse<ContractCallBytecode>(contractCallBytecodeResponse);

        SignContractCallResponse signContractCallResponse = await apiService.signContractCallTx(
            contractCallBytecode.contractFunctionParameters,
            contractId,
            functionName,
            gas,
            this.getTvteToken(),
            false
        );
        SignedTx signedTx = this.signTransaction(signContractCallResponse.transactionBytes, "base64", accountPrivateKey);
        return await apiService.executeTx(signedTx.tx, signedTx.network);
    } else {
        string signedTxResponse = engine
            .Evaluate($"window.bladeSdk.contractCallFunctionTransaction('{contractId}', '{functionName}', '{parameters.encode()}', '{accountId}', '{accountPrivateKey}', {gas})")
            .UnwrapIfPromise()
            .ToString();
        SignedTx signedTx = this.processResponse<SignedTx>(signedTxResponse);
        return await apiService.executeTx(signedTx.tx, signedTx.network);
    }
}
```


## Method to call smart-contract query

### Parameters:

* `contractId`: contract id
* `functionName`: contract function name
* `params`: function arguments (instance of ContractFunctionParameters)
* `accountId`: sender account id
* `accountPrivateKey`: sender's private key to sign transfer transaction
* `gas`: gas amount for transaction (default 100000)
* `bladePayFee`: blade pay fee, otherwise fee will be pay from sender accountId
* `returnTypes`: array of return types, e.g. ["string", "int32"]

### Returns: 

* an instance of `ContractQueryData` containing result from query with types

```cs
public async Task<ContractQueryData> contractCallQueryFunction(
    string contractId, 
    string functionName, 
    ContractFunctionParameters parameters,
    string accountId,
    string accountPrivateKey, 
    uint gas, 
    uint fee,
    List<string> returnTypes
) {
    if (fee > 0) {
        string nodeAccountId = "0.0.3";            
        string delayedQueryCallResponse = engine
            .Evaluate($"window.bladeSdk.contractCallQueryFunction('{contractId}', '{functionName}', '{parameters.encode()}', '{accountId}', '{accountPrivateKey}', {gas}, {fee}, '{nodeAccountId}')")
            .UnwrapIfPromise()
            .ToString();
        DelayedQueryCall delayedQueryCall = this.processResponse<DelayedQueryCall>(delayedQueryCallResponse);
        
        DelayedQueryCallResult delayedQueryCallResult = await apiService.executeDelayedQueryCall(delayedQueryCall);
        
        string contractCallQueryResponse = engine
            .Evaluate($"window.bladeSdk.parseContractCallQueryResponse('{contractId}', '{delayedQueryCallResult.contractFunctionResult.gasUsed}', '{delayedQueryCallResult.rawResult}', ['{string.Join("', '", returnTypes)}'])")
            .UnwrapIfPromise()
            .ToString();
        ContractQueryData contractQueryData = this.processResponse<ContractQueryData>(contractCallQueryResponse);
        return contractQueryData;
    } else {
        // blade pay fee 
        throw new BladeSDKException("Error", "free queries not implemented yet");
    }
}
```


## Method to get C14 url for payment

### Parameters:

* `asset`: USDC, HBAR, KARATE or C14 asset uuid
* `account`: receiver account id
* `amount`: amount to buy

### Returns: 

* string with C14 url with preseted paramsbalances. 

```cs
public async Task<string> getC14url(
    string asset,
    string account,
    string amount
) {
    string tvteToken = this.getTvteToken();
    string clientId = await apiService.getC14token(tvteToken);
    
    var purchaseParams = new Dictionary<string, object> {
        { "clientId", clientId }
    };

    switch (asset.ToUpper()) {
        case "USDC":
            purchaseParams["targetAssetId"] = "b0694345-1eb4-4bc4-b340-f389a58ee4f3";
            purchaseParams["targetAssetIdLock"] = "true";
            break;
        case "HBAR":
            purchaseParams["targetAssetId"] = "d9b45743-e712-4088-8a31-65ee6f371022";
            purchaseParams["targetAssetIdLock"] = "true";
            break;
        case "KARATE":
            purchaseParams["targetAssetId"] = "057d6b35-1af5-4827-bee2-c12842faa49e";
            purchaseParams["targetAssetIdLock"] = "true";
            break;
        default:
            if (asset.Split('-').Length == 5) {
                purchaseParams["targetAssetId"] = asset;
                purchaseParams["targetAssetIdLock"] = "true";
            }
            break;
    }

    if (!string.IsNullOrEmpty(amount)) {
        purchaseParams["sourceAmount"] = amount;
        purchaseParams["quoteAmountLock"] = "true";
    }

    if (!string.IsNullOrEmpty(account)) {
        purchaseParams["targetAddress"] = account;
        purchaseParams["targetAddressLock"] = "true";
    }

    string queryString = "";
    foreach (var kv in purchaseParams) {
        queryString = queryString + $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value.ToString())}&";
    }

    var uriBuilder = new UriBuilder("https://pay.c14.money/");
    uriBuilder.Query = queryString.ToString();
    return uriBuilder.Uri.ToString();
}
```


## Sign message with private key

### Parameters:

* `messageEncoded`: encoder message string
* `accountPrivateKey`: private key string
* `encoding`: message encoding. One of: hex, base64, utf8

### Returns: 

* an instance of `SignMessageData` containing signature

```cs
public async Task<SignMessageData> sign(
    string messageEncoded, 
    string accountPrivateKey,
    string encoding // hex|base64|utf8
) {
    string signMessageReponse = engine
        .Evaluate($"window.bladeSdk.sign('{messageEncoded}', '{accountPrivateKey}', '{encoding}')")
        .UnwrapIfPromise()
        .ToString();
    SignMessageData signMessageData = this.processResponse<SignMessageData>(signMessageReponse);
    return signMessageData;
}
```


## Verify message signature with public key

### Parameters:

* `messageEncoded`: message encoded
* `signatureHex`: hex-encoded signature string
* `publicKey`: public key string
* `encoding`: message encoding. One of: hex, base64, utf8

### Returns: 

* boolean. True if signature is valid 

```cs
public async Task<bool> signVerify(
    string messageEncoded, 
    string signatureHex,
    string publicKey, // hex-encoded public key with DER header
    string encoding // hex|base64|utf8
) {
    string signVerifyMessageResponse = engine
        .Evaluate($"window.bladeSdk.signVerify('{messageEncoded}', '{signatureHex}', '{publicKey}', '{encoding}')")
        .UnwrapIfPromise()
        .ToString();
    SignVerifyMessageData signVerifyMessageData = this.processResponse<SignVerifyMessageData>(signVerifyMessageResponse);
    return signVerifyMessageData.valid;
}
```


## Sign message with private key (hethers lib)

### Parameters:

* `messageEncoded`: encoder message string
* `accountPrivateKey`: private key string
* `encoding`: message encoding. One of: hex, base64, utf8

### Returns: 

* an instance of `SignMessageData` containing signature
 

```cs
public async Task<SignMessageData> hethersSign(
    string messageEncoded, 
    string accountPrivateKey,
    string encoding // hex|base64|utf8
) {
    string signMessageReponse = engine
        .Evaluate($"window.bladeSdk.hethersSign('{messageEncoded}', '{accountPrivateKey}', '{encoding}')")
        .UnwrapIfPromise()
        .ToString();
    SignMessageData signMessageData = this.processResponse<SignMessageData>(signMessageReponse);
    return signMessageData;
}
```


## Method to split signature into v-r-s

### Parameters:

* `signature`: signature string "0x21fbf0696......"

### Returns: 

* an instance of `SplitSignatureData` containing v, r, s

```cs
public async Task<SplitSignatureData> splitSignature(string signature) {
    string splitSignatureResponse = engine
        .Evaluate($"window.bladeSdk.splitSignature('{signature}')")
        .UnwrapIfPromise()
        .ToString();
    SplitSignatureData splitSignatureData = this.processResponse<SplitSignatureData>(splitSignatureResponse);
    return splitSignatureData;
}
```


## Get signature for contract params into v-r-s

### Parameters:

* `params`: function arguments (instance of ContractFunctionParameters)
* `accountPrivateKey`: account private key string

### Returns: 

* an instance of `SplitSignatureData` containing v, r, s

```cs
public async Task<SplitSignatureData> getParamsSignature(
    ContractFunctionParameters parameters,
    string accountPrivateKey
) {
    string splitSignatureResponse = engine
        .Evaluate($"window.bladeSdk.getParamsSignature('{parameters.encode()}', '{accountPrivateKey}')")
        .UnwrapIfPromise()
        .ToString();
    SplitSignatureData splitSignatureData = this.processResponse<SplitSignatureData>(splitSignatureResponse);
    return splitSignatureData;
}
```


## Method to get transactions history

### Parameters:

* `accountId`: accountId of history
* `transactionType`: filter by type of transaction
* `nextPage`: link from response to load next page of history
* `transactionsLimit`: limit of transactions to load

### Returns: 

* an instance of `TransactionsHistoryData` containing transaction records, filtered and aggregated

```cs
public async Task<TransactionsHistoryData> getTransactions(string accountId, string transactionType, string nextPage, int transactionsLimit) {
    TransactionsHistoryData transactionsHistoryData = await apiService.getTransactionsFrom(accountId, transactionType, nextPage, transactionsLimit);
    return transactionsHistoryData;
}

```
