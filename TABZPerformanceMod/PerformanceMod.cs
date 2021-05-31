using System.IO;
using UnityEngine;
using CAMOWA;
using HarmonyLib;

namespace TABZPerformanceMod
{
    public class PerformanceMod 
    {
        public static ConfigInfo config;
        private static bool HasThePatchHappened = false;

        [IMOWAModInnit("Performance Mod", 1, 1)]
        static public void ModInnit(string startingPoint)
        {
            string[] jsonPaths = Directory.GetFiles(Directory.GetCurrentDirectory(), "TABZPerformanceModConfig.json");
            Camera mainCamera = Camera.main;
            if (jsonPaths.Length == 0)
            {
                Debug.Log("Config File doesn't exist, making one...");
                config = new ConfigInfo(mainCamera.fieldOfView, mainCamera.farClipPlane, 0f);
                config.ToJsonFile(Directory.GetCurrentDirectory());
            }
            config = ConfigInfo.FromJson(jsonPaths[0]);

            if (!HasThePatchHappened)
            {
                Debug.Log("Changing the camera settings");
                mainCamera.fieldOfView = config.CameraFOV;
                mainCamera.farClipPlane = config.CameraRenderDistance;
            }
            var harmonyInstance = new Harmony("com.ivan.PerformanceMod");
            harmonyInstance.PatchAll();
            HasThePatchHappened = true;
        }        
    }
    [HarmonyPatch(typeof(DayNightCycle))]
    [HarmonyPatch("Update")]
    public class DayNightCycle_UpdatePatch
    {
        static void Postfix(DayNightCycle __instance)
        {
            __instance.sun.shadows = LightShadows.Soft;
            RenderSettings.ambientIntensity = Mathf.Clamp( __instance.ambientSunCurve.Evaluate(__instance.time),PerformanceMod.config.MinBrightness, 2f);
            __instance.sun.intensity = Mathf.Clamp(__instance.sunCurve.Evaluate(__instance.time), PerformanceMod.config.MinBrightness, 2f);
        }
    }

    [HarmonyPatch(typeof(CameraMovement))]
    [HarmonyPatch("LateUpdate")]
    public class CameraMovement_LateUpdatePatch
    {
        static public AccessTools.FieldRef<CameraMovement, bool> mHasNetworkControlAT =
            AccessTools.FieldRefAccess<CameraMovement, bool>("mHasNetworkControl");

        static void Postfix(CameraMovement __instance)
        {
            if (mHasNetworkControlAT(__instance))
            {
                if (!Input.GetKey(KeyCode.Mouse1))
                {
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, PerformanceMod.config.CameraFOV, Time.deltaTime * 20f);
                }
            }
        }
    }


    [HarmonyPatch(typeof(NetworkPlayerActivator))]
    [HarmonyPatch("Activate")]
    public class NetworkPlayerActivator_SpawnPlayerPatch
    {
        static public AccessTools.FieldRef<NetworkPlayerActivator,Camera> playerCamera =
            AccessTools.FieldRefAccess<NetworkPlayerActivator,Camera>("mCamera");

        static void Postfix(NetworkPlayerActivator __instance)
        {
            Debug.Log("Changing the camera settings");
            Camera mainCamera = playerCamera(__instance); 
            mainCamera.fieldOfView = PerformanceMod.config.CameraFOV;
            mainCamera.farClipPlane = PerformanceMod.config.CameraRenderDistance;
        }
    }
}
