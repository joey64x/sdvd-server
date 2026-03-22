using System;
using HarmonyLib;
using StardewModdingAPI;

namespace JunimoServer.Services.HttpListenerGuard
{
    public class HttpListenerGuardService : ModService
    {
        public HttpListenerGuardService(Harmony harmony, IMonitor monitor) : base(monitor)
        {
            HttpListenerGuardOverrides.Initialize(monitor);

            // HttpConnection is an internal .NET class in System.Net.HttpListener.
            // Malformed HTTP requests can cause ArgumentOutOfRangeException inside
            // OnReadInternal, which propagates as an unhandled threadpool exception
            // and crashes the entire process.
            var httpConnectionType = Type.GetType(
                "System.Net.HttpConnection, System.Net.HttpListener");

            if (httpConnectionType == null)
            {
                monitor.Log("[HttpListenerGuard] Could not find HttpConnection type — patch not applied",
                    LogLevel.Warn);
                return;
            }

            var onReadInternal = AccessTools.Method(httpConnectionType, "OnReadInternal");
            if (onReadInternal == null)
            {
                monitor.Log("[HttpListenerGuard] Could not find OnReadInternal method — patch not applied",
                    LogLevel.Warn);
                return;
            }

            harmony.Patch(
                original: onReadInternal,
                finalizer: new HarmonyMethod(typeof(HttpListenerGuardOverrides),
                    nameof(HttpListenerGuardOverrides.OnReadInternal_Finalizer))
            );

            monitor.Log("[HttpListenerGuard] Patched HttpConnection.OnReadInternal", LogLevel.Info);
        }
    }
}
