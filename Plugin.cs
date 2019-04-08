using Harmony;
using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomUI
{
    public class Plugin : IBeatSaberPlugin
    {
        internal static Ref<PluginConfig> config;
        internal static IConfigProvider configProvider;

        public void Init(IPALogger logger, [Config.Prefer("json")] IConfigProvider cfgProvider)
        {
            Logger.log = logger;
            configProvider = cfgProvider;

            config = cfgProvider.MakeLink<PluginConfig>((p, v) =>
            {
                if (v.Value == null || v.Value.RegenerateConfig)
                    p.Store(v.Value = new PluginConfig() { RegenerateConfig = false });
                config = v;
            });
        }

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

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        public static void Log(string text, IPALogger.Level level = IPALogger.Level.Info, [CallerFilePath] string file = "",
               [CallerMemberName] string member = "",
               [CallerLineNumber] int line = 0)
        {
            Logger.log.Log(level, $"{Path.GetFileName(file)}->{member}({line}): {text}");
        }
    }
}
