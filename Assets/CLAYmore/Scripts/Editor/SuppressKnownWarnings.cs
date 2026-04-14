using System;
using UnityEditor;
using UnityEngine;

namespace CLAYmore.Editor
{
    [InitializeOnLoad]
    public static class SuppressKnownWarnings
    {
        static SuppressKnownWarnings()
        {
            Install();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredEditMode)
                Install();
        }

        static void Install()
        {
            if (Debug.unityLogger.logHandler is FilteredLogHandler)
                return;
            Debug.unityLogger.logHandler = new FilteredLogHandler(Debug.unityLogger.logHandler);
        }
    }

    internal sealed class FilteredLogHandler : ILogHandler
    {
        private readonly ILogHandler _original;

        public FilteredLogHandler(ILogHandler original) => _original = original;

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            try
            {
                string msg = (args == null || args.Length == 0)
                    ? format
                    : string.Format(format, args);
                if (msg != null && msg.Contains("Screen position out of view frustum"))
                    return;
            }
            catch { /* невалидная format-строка — пропускаем фильтр */ }

            _original.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
            => _original.LogException(exception, context);
    }
}
