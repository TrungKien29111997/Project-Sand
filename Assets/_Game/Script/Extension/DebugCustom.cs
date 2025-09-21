using System;
using Debug = UnityEngine.Debug;
using Newtonsoft.Json;

namespace TrungKien
{
    public class DebugCustom
    {
        private static bool _isLogBug = true;

        private static bool IsLogBug
        {
            get
            {
                if (!_isLogBug)
                    return false;
                return true;
            }
        }

        public static void LogWarning(params object[] content)
        {
#if UNITY_EDITOR
            if (!IsLogBug)
                return;
            string str = PrepareString(content);
            Debug.LogWarning(str);
#endif
        }

        public static void Log(params object[] content)
        {
#if UNITY_EDITOR
            if (!IsLogBug)
                return;
            string str = PrepareString(content);
            Debug.Log(str);
#endif
        }
#if UNITY_EDITOR
        public static string ReturnLog(params object[] content)
        {
            string str = PrepareString(content);
            Debug.Log(str);
            return str;
        }
#endif
        public static void LogError(params object[] content)
        {
#if UNITY_EDITOR
            if (!IsLogBug)
                return;
            string str = PrepareString(content);
            Debug.LogError(str);
#endif
        }

        public static void LogColor(params object[] content)
        {
#if UNITY_EDITOR || GAME_ROCKET
            if (!IsLogBug)
                return;
            string str = PrepareString(content);
            Debug.Log("<color=\"" + "#ffa500ff" + "\">" + str + "</color>");
#endif
        }

        public static void LogColorJson(params object[] content)
        {
#if UNITY_EDITOR
            if (!IsLogBug)
                return;
            string str = PrepareStringJson(content);
            Debug.Log("<color=\"" + "#ffa500ff" + "\">" + str + "</color>");
#endif
        }

        public static void LogJson(params object[] content)
        {
#if UNITY_EDITOR
            if (!IsLogBug)
                return;
            string str = PrepareStringJson(content);
            Debug.Log(str);
#endif
        }

        public static void LogErrorJson(params object[] content)
        {
#if UNITY_EDITOR
            if (!IsLogBug)
                return;
            string str = PrepareStringJson(content);
            Debug.LogError(str);
#endif
        }

        public static string PrepareString(object[] content)
        {
            string str = "";
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] is Exception)
                {
                    Debug.LogException(content[i] as Exception);
                    return (content[i] as Exception).Message;
                }
                if (i == content.Length - 1)
                {
                    str += content[i].ToString();
                }
                else
                {
                    str += content[i].ToString() + "__";
                }
            }
            return str;
        }
        public static string PrepareStringJson(object[] content)
        {
            string str = "";
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] is Exception)
                {
                    Debug.LogException(content[i] as Exception);
                    return (content[i] as Exception).Message;
                }
                if (i == content.Length - 1)
                {
                    str += JsonConvert.SerializeObject(content[i]);
                }
                else
                {
                    str += JsonConvert.SerializeObject(content[i]) + "__";
                }
            }
            return str;
        }
        static DateTime cachedTime;
        static long cachedGCBytes;

        public static void StartProfilerFunction()
        {
            cachedTime = DateTime.Now;
            cachedGCBytes = GC.GetTotalMemory(true);
        }

        public static void LogProfilerFunction(string funcName)
        {
            TimeSpan time = DateTime.Now - cachedTime;
            long totalBytes = GC.GetTotalMemory(true) - cachedGCBytes;
            LogError($"Function {funcName} Cost: {totalBytes} bytes GCAlloc, {time.TotalMilliseconds} Ticks Execute");
        }
    }
}