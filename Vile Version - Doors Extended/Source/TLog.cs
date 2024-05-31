using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;

namespace DoorsExpanded
{
    // Debug logging that optionally includes deduped stack traces (what the T stands for). Controlled by logLevel mod option.

    public enum TLogLevel
    {
        Normal,
        Debug,
        StackTrace,
    }

    static class TLog
    {
        private static readonly ConcurrentDictionary<string, int> methodToCounter = new();
        private static readonly ConcurrentDictionary<string, string> stackTraceToId = new();

        public static bool Enabled => DoorsExpandedMod.Settings.logLevel is not TLogLevel.Normal;

        public static void Log(object obj, string message = null, [CallerMemberName] string callerMemberName = "")
        {
            var logLevel = DoorsExpandedMod.Settings.logLevel;
            switch (logLevel)
            {
                case TLogLevel.Debug:
                case TLogLevel.StackTrace:
                    break;
                default:
                    return;
            }

            var context = Current.Game is { } game ? $"Tick {game.tickManager.TicksGame}" : $"{Current.ProgramState}";
            message ??= obj.ToString();
            if (logLevel is TLogLevel.Debug)
            {
                message = $"[{context}] {message} called from {obj as Type ?? obj.GetType()}:{callerMemberName}";
            }
            else // if (logLevel is TLogLevel.StackTrace)
            {
                var stackTrace = new StackTrace(1);
                var stackTraceStr = stackTrace.ToString().Replace("\r", "");
                var newId = false;
                var id = stackTraceToId.GetOrAdd(stackTraceStr, _ =>
                {
                    newId = true;
                    var sb = new StringBuilder();
                    var method = stackTrace.GetFrame(0).GetMethod();
                    var methodName = method.Name;
                    if (method.DeclaringType is { } declaringType)
                        methodName = declaringType + ":" + methodName;
                    var counter = methodToCounter.AddOrUpdate(methodName,
                        addValueFactory: _ => 0,
                        updateValueFactory: (_, counter) => counter + 1);
                    return $"{methodName} {{{counter}}}";
                });
                message = $"[{context}] {message} called from {id}";
                if (newId)
                    message += "\n" + stackTraceStr;
            }

            // Workaround for RW 1.2+ now monitoring all Unity Debug usage for the 1000 max message limit:
            if (!UnityEngine.Debug.unityLogger.logEnabled)
                Verse.Log.ResetMessageCount();

            UnityEngine.Debug.Log(message);
        }
    }
}
