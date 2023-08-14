using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BladeLabs.UnitySDK {
    public class TransactionUtils
    {
        public static List<TransactionData> formatTransactionData(TransactionsHistoryRaw transactionsHistoryRaw, string accountId = "") {
            List<TransactionData> result = transactionsHistoryRaw.transactions.Select(t => new TransactionData {
                time = DateTimeOffset.FromUnixTimeMilliseconds((long)(double.Parse(t.consensus_timestamp) * 1000)).DateTime,
                transfers = (t.token_transfers ?? Enumerable.Empty<TransfersRaw>())
                    .Concat(t.transfers ?? Enumerable.Empty<TransfersRaw>())
                    .Where(tt => tt.account != accountId)
                    .Select(tt => new TransferData {
                        amount = string.IsNullOrEmpty(tt.token_id) ? tt.amount / Math.Pow(10, 8) : tt.amount,
                        account = tt.account,
                        token_id = tt.token_id
                    })
                    .ToList(),
                nftTransfers = null, // TODO fix after creating valid models: t.nft_transfers ?? null,
                memo = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(t.memo_base64)),
                transactionId = t.transaction_id,
                fee = t.charged_tx_fee,
                type = t.name,
                consensusTimestamp = t.consensus_timestamp
            }).ToList();
            return result;
        }


        public static List<TransactionData> filterAndFormatTransactions(List<TransactionData> transactions, string transactionType) {
            if (string.IsNullOrEmpty(transactionType)) {
                return transactions;
            }
            return transactions
                .Where(tx => tx.type == transactionType)
                .ToList();
        }
    }
}
