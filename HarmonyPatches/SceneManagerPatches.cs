using Harmony;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    [HarmonyPatch(typeof(SceneManager))]
    [HarmonyPatch("Internal_SceneLoaded", MethodType.Normal)]
    public class SceneManagerSceneLoadedPatch
    {
        public static bool Prefix(Scene scene, LoadSceneMode mode, UnityAction<Scene, LoadSceneMode> ___sceneLoaded)
        {
            if (___sceneLoaded == null) return true;
            foreach (var action in ___sceneLoaded.GetInvocationList())
            {
                try
                {
                    action?.DynamicInvoke(new object[] { scene, mode });
                }
                catch (Exception ex)
                {
                    Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SceneManager))]
    [HarmonyPatch("Internal_SceneUnloaded", MethodType.Normal)]
    public class SceneManagerSceneUnloadedPatch
    {
        public static bool Prefix(Scene scene, UnityAction<Scene, LoadSceneMode> ___sceneUnloaded)
        {
            if (___sceneUnloaded == null) return true;
            foreach (var action in ___sceneUnloaded.GetInvocationList())
            {
                try
                {
                    action?.DynamicInvoke(new object[] { scene });
                }
                catch (Exception ex)
                {
                    Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SceneManager))]
    [HarmonyPatch("Internal_ActiveSceneChanged", MethodType.Normal)]
    public class SceneManagerActiveSceneChangedPatch
    {
        public static bool Prefix(Scene previousActiveScene, Scene newActiveScene, UnityAction<Scene, Scene> ___activeSceneChanged)
        {
            if (___activeSceneChanged == null) return true;
            foreach (var action in ___activeSceneChanged.GetInvocationList())
            {
                try
                {
                    action?.DynamicInvoke(new object[] { previousActiveScene, newActiveScene });
                }
                catch (Exception ex)
                {
                    Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
                }
            }
            return false;
        }
    }
}
