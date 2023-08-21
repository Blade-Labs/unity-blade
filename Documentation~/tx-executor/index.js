const express = require('express')
const cors = require('cors')
const bodyParser = require('body-parser')
const {Client, Transaction, Query, AccountId, Hbar, TransactionId, Timestamp} = require("@hashgraph/sdk");
const {Buffer} = require("buffer");

const app = express()
app.use(cors())
app.use(bodyParser.json())

app.post('/signer/tx', (req, res) => {
    console.log(JSON.stringify(req.body));
    const {tx, network} = req.body;
    const client = getClient(network);
    const transaction = Transaction.fromBytes(Buffer.from(tx, "hex"));
    transaction
        .execute(client)
        .then(executedTx => {
            return executedTx.getReceipt(client);
        })
        .then(txReceipt => {
            const result = {
                ...txReceipt,
                status: txReceipt.status?.toString(),
            }; 
            console.log(result);
            res.send(result);
        })
        .catch((err) => {
            console.error(JSON.stringify(err));
            res.status(400).json({ 
                error: err.message
            });
        });
});



app.post('/signer/query', async (req, res) => {
    const {
        network,
        queryHex,
        signedBuffers,
        sharedTimestamp,
        nodeAccountId,
        publicKey,
        accountId,
        fee
    } = req.body;
    
    console.log(JSON.stringify(req.body));
    
    try {
        const clientFakeSign = getClient(network);
        clientFakeSign.setOperatorWith(
            accountId,
            publicKey,
            async buf => {
                const hex = signedBuffers.shift();
                return Buffer.from(hex, "hex");
            }
        );

        console.log(`TS = ${Timestamp.fromDate(new Date(sharedTimestamp))}, orig = ${sharedTimestamp}`);


        const q = Query
            .fromBytes(Buffer.from(queryHex, "hex"))
            .setNodeAccountIds([AccountId.fromString(nodeAccountId)])
            .setQueryPayment(Hbar.fromTinybars(fee))
            .setPaymentTransactionId(new TransactionId(AccountId.fromString(accountId), Timestamp.fromDate(new Date(sharedTimestamp))))

        const response = await q.execute(clientFakeSign);

        const result = {
            rawResult: Buffer.from(response.bytes).toString("base64"),
            contractFunctionResult: {
                contractId: response.contractId.toString(),
                gasUsed: response.gasUsed.toNumber()
            }
        };
        res.send(result);
    } catch (error) {
        console.log(error);
        res.send(error);
    }
});

function getClient(network) {
    return network.toUpperCase() === "MAINNET" ? Client.forMainnet() : Client.forTestnet()
}

app.listen(8443, () => {
    console.log('Running...');
})
