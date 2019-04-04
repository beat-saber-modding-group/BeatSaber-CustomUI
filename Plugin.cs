using Harmony;
using IllusionPlugin;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomUI
{
    public class Plugin : IPlugin
    {
        public string Name => "BeatSaberCustomUI";
        public string Version => "1.4.1";

        private HarmonyInstance _harmonyInstance;
        public void OnApplicationStart()
        {
            // Disable stack traces for log and warning type log messages, as they just result in tons of useless spam
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            _harmonyInstance = HarmonyInstance.Create("com.brian91292.beatsaber.customui");
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnApplicationQuit()
        {
            _harmonyInstance.UnpatchAll("com.brian91292.beatsaber.customui");
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }
        
        public void OnFixedUpdate()
        {
        }

        public static void Log(string text,
                       [CallerFilePath] string file = "",
                       [CallerMemberName] string member = "",
                       [CallerLineNumber] int line = 0)
        {
            Console.WriteLine($"[CustomUI] {Path.GetFileName(file)}->{member}({line}): {text}");
        }
    }
}
