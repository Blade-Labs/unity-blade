using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BladeLabs.UnitySDK {
    public class StringUtils
    {
        public static string readNodeIdFromRawJson(string rawJson, string network) {
            try {
                // {"mainnet":{"https://node01-00-grpc.swirlds.com:443":"0.0.4"},"testnet":{"https://testnet-node00-00-grpc.hedera.com:443":"0.0.3"}}
                rawJson = rawJson.Trim('{', '}');
                string[] networkSections = rawJson.Split(',');

                foreach (string item in networkSections) {
                    string networkSection = item.Trim('{', '}');
                    // "mainnet":{"https://node01-00-grpc.swirlds.com:443":"0.0.4"  
                    
                    string[] keyValues = networkSection.Split(":{" , StringSplitOptions.RemoveEmptyEntries);

                    if (keyValues.Length > 1) {
                        if (keyValues[0].Trim('"','"').ToUpper() == network.ToUpper()) {
                            string[] nodeInfo = keyValues[1].Split("\":\"" , StringSplitOptions.RemoveEmptyEntries);

                            if (nodeInfo.Length > 1) {
                                return nodeInfo[1].Trim('"','"');
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                return "0.0.3"; // fallback node id
            }
            return "0.0.3"; // fallback node id
        }
    }
}
