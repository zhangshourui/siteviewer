using System;

namespace SiteViewer.WWW.Utils
{
    public static class ThreadHelper
    {
        private static ThreadLocal<string> _lastErrorValue = new();
        private static ThreadLocal<string> _lastErrorCodeValue = new();

        public static string? GetLastError()
        {
          //  var obj = LogicalThreadContext<string>.GetData(METHODHELPER_ERRKEY);
            return _lastErrorValue.Value;
        }

        public static string? GetLastErrorCode()
        {
            return _lastErrorCodeValue.Value;
        }

        public static void SetLastError(string errMsg)
        {
            _lastErrorValue.Value = errMsg;
        }

        public static void SetLastErrorCode(string code)
        {
            _lastErrorCodeValue.Value = code;
        }

        public static void ClearError()
        {
            _lastErrorCodeValue.Value = string.Empty;
            _lastErrorValue.Value = string.Empty;
        }
    }
}