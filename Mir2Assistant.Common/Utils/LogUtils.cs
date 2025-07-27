using Serilog;
using Mir2Assistant.Common.Models;

namespace Mir2Assistant.Common.Utils
{
    public static class LogUtils
    {
        public static void GameInfo(this MirGameInstanceModel instance, string message, params object[] args)
        {
            var prefix = $"[{instance.AccountInfo?.Account ?? "Unknown"}] ";
            Log.Information(prefix + message, args);
        }

        public static void GameDebug(this MirGameInstanceModel instance, string message, params object[] args)
        {
            var prefix = $"[{instance.AccountInfo?.Account ?? "Unknown"}] ";
            Log.Debug(prefix + message, args);
        }

        public static void GameWarning(this MirGameInstanceModel instance, string message, params object[] args)
        {
            var prefix = $"[{instance.AccountInfo?.Account ?? "Unknown"}] ";
            Log.Warning(prefix + message, args);
        }

        public static void GameError(this MirGameInstanceModel instance, string message, params object[] args)
        {
            var prefix = $"[{instance.AccountInfo?.Account ?? "Unknown"}] ";
            Log.Error(prefix + message, args);
        }
    }
} 