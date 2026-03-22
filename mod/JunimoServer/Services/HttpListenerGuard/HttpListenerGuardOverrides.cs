using System;
using StardewModdingAPI;

namespace JunimoServer.Services.HttpListenerGuard
{
    public class HttpListenerGuardOverrides
    {
        private static IMonitor _monitor;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Harmony finalizer for System.Net.HttpConnection.OnReadInternal.
        /// Catches exceptions from malformed HTTP requests that would otherwise
        /// crash the process as unhandled threadpool exceptions.
        /// </summary>
        public static Exception OnReadInternal_Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                _monitor?.Log(
                    $"[HttpListenerGuard] Suppressed {__exception.GetType().Name} in HttpConnection.OnReadInternal: {__exception.Message}",
                    LogLevel.Warn);
            }
            return null; // Suppress — the malformed connection is already dead
        }
    }
}
