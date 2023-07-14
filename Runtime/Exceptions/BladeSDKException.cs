using System;

namespace BladeLabs.UnitySDK {
    public class BladeSDKException : Exception
    {
        private string reason;
        
        public BladeSDKException(string message, string reason) : base(message)
        {
            this.reason = reason;
        }

        public override string Message {
            get {
                return base.Message + ": " + reason;
            }
        }
    }
}

