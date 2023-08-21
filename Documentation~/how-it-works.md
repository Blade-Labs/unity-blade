# How it works

Thing to keep in mind:

* It's early prototype
* It's not fully native c#
* Most of it runs on javascript 
* JS part runs on [Jint](https://github.com/sebastienros/jint)
* It requires external server enpoint to execute signed transactions due network limitaions of Jint
* There are some limitations
* Some hacks here too

# Example flow of transfer transaction 

* Create instance of BladeSDK
  * Create instance of Jint Engine
  * Load and execute `Packages/io.bladelabs.unity-sdk/Resources/JSUnityWrapper.bundle.js`
  * Init JS version of BladeSDK: `window.bladeSdk.init('{apiKey}', '{network}', '{dAppCode}', '{sdkEnvironment}', '{sdkVersion}')`
  * Call JS `window.bladeSdk.transferHbars('{accountId}', '{accountPrivateKey}', '{recieverAccount}', '{amount}', '{memo}')`
  * Process response and extract signed transaction (hex encoded)
  * Make network call to external endpoint to execute signed transaction
  * Parse response and return ExecuteTxReceipt object

In this prototype Jint engine loaded from local file Jint.dll.
As for now Jint isn't support threading, so there is no possibility to mock XMLHttpRequest or fetch to execute transaction inside JS environment. The only solution is to execute transactions on external server. 