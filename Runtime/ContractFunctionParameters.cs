using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Collections.Generic;

namespace BladeLabs.UnitySDK
{
    [Serializable]
    public class ContractFunctionParameters
    {
        [SerializeField]
        private List<ContractFunctionParameter> paramsList = new List<ContractFunctionParameter>();


        public ContractFunctionParameters addAddress(string value) {
            paramsList.Add(new ContractFunctionParameter("address", new List<string> { value }));
            return this;
        }

        public ContractFunctionParameters addAddressArray(List<string> value) {
            paramsList.Add(new ContractFunctionParameter("address[]", value));
            return this;
        }

        public ContractFunctionParameters addBytes32(List<uint> value) {
            try {
                string jsonBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"[{string.Join(",", value.Select(v => (long)v).ToList())}]"));
                paramsList.Add(new ContractFunctionParameter("bytes32", new List<string> { jsonBase64 }));
            } catch (Exception error) {
                Debug.Log(error);
            }
            return this;
        }

        public ContractFunctionParameters addUInt8(uint value) {
            paramsList.Add(new ContractFunctionParameter("uint8", new List<string> { value.ToString() }));
            return this;
        }

        public ContractFunctionParameters addUInt64(ulong value) {
            paramsList.Add(new ContractFunctionParameter("uint64", new List<string> { value.ToString() }));
            return this;
        }

        public ContractFunctionParameters addUInt64Array(List<ulong> value) {
            paramsList.Add(new ContractFunctionParameter("uint64[]", value.Select(v => v.ToString()).ToList()));
            return this;
        }

        public ContractFunctionParameters addInt64(long value) {
            paramsList.Add(new ContractFunctionParameter("int64", new List<string> { value.ToString() }));
            return this;
        }

        public ContractFunctionParameters addUInt256(BigInteger value) {
            paramsList.Add(new ContractFunctionParameter("uint256", new List<string> { value.ToString() }));
            return this;
        }

        public ContractFunctionParameters addUInt256Array(List<BigInteger> value) {
            paramsList.Add(new ContractFunctionParameter("uint256[]", value.Select(v => v.ToString()).ToList()));
            return this;
        }

        public ContractFunctionParameters addTuple(ContractFunctionParameters value) {
            paramsList.Add(new ContractFunctionParameter("tuple", new List<string> { value.encode() }));
            return this;
        }

        public ContractFunctionParameters addTupleArray(List<ContractFunctionParameters> value) {
            paramsList.Add(new ContractFunctionParameter("tuple[]", value.Select(v => v.encode()).ToList()));
            return this;
        }

        public ContractFunctionParameters addString(string value) {
            paramsList.Add(new ContractFunctionParameter("string", new List<string> { value }));
            return this;
        }

        public ContractFunctionParameters addStringArray(List<string> value) {
            paramsList.Add(new ContractFunctionParameter("string[]", value));
            return this;
        }

        public string encode() {
            try {
                string result = JsonUtility.ToJson(this);

                // Make JSON without root key `paramsList`, just for compatibility
                // WRONG: {"paramsList":[{"type":"string","value":["Hello Unity SDK"]}]}
                // GOOD: [{"type":"string","value":["Hello Unity SDK"]}]
                string removeString = "{\"paramsList\":";
                int index = result.IndexOf(removeString, StringComparison.Ordinal);
                result = (index < 0)
                    ? result
                    : result.Remove(index, removeString.Length);
                result = result.Remove(result.Length - 1, 1);

                // return base64 encoded result. To make it compatible with Swift, Kotlin, JS SDK
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(result));
            } catch (Exception error) {
                Debug.Log(error);
            }
            return "";
        }
    }
}
