using System.IO;
using UnityEngine;
using CAMOWA;
using HarmonyLib;

namespace TABZSettingsMod
{
    public class SettingsMod 
    {
        public static ConfigInfo config;
        private static bool HasThePatchHappened = false;
        private static string gamePath;
        public static string GameExecutabePath
        {
            get
            {
                if (string.IsNullOrEmpty(gamePath))
                    gamePath = Application.dataPath.Remove(Application.dataPath.LastIndexOf('/'));
                return gamePath;
            }

            private set { }
        }

        [IMOWAModInnit("Settings Mod", 1, 1)]
        static public void ModInnit(string startingPoint)
        {
            Debug.Log("Running SettingsMod Mod");
            string[] jsonPaths = Directory.GetFiles(GameExecutabePath, "TABZSettingsModConfig.json",SearchOption.AllDirectories);
            Camera mainCamera = Camera.main;
            if (jsonPaths.Length == 0)
            {
                Debug.Log("Config File doesn't exist, making one...");
                config = new ConfigInfo(mainCamera.fieldOfView, mainCamera.farClipPlane, 0f, 0, 100, 100);
                config.ToJsonFile(GameExecutabePath);
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
        public static void SearchForChildrenInGO(Transform t)
        {
            int ammountOfChildren = t.childCount;
            Debug.Log("Children from " + t.name);
            for (int i = 0; i < ammountOfChildren; i++)
            {
                var g = t.GetChild(i);
                Debug.Log("name: " + g.name);
                foreach (var c in g.GetComponents<MonoBehaviour>())
                    Debug.Log(c.GetType());

                SearchForChildrenInGO(g);
            }
        }
    }
    [HarmonyPatch(typeof(DayNightCycle))]
    [HarmonyPatch("Update")]
    public class DayNightCycle_UpdatePatch
    {
        static void Postfix(DayNightCycle __instance)
        {
            __instance.sun.shadows = LightShadows.Soft;
            RenderSettings.ambientIntensity = Mathf.Clamp( __instance.ambientSunCurve.Evaluate(__instance.time),SettingsMod.config.MinBrightness, 2f);
            __instance.sun.intensity = Mathf.Clamp(__instance.sunCurve.Evaluate(__instance.time), SettingsMod.config.MinBrightness, 2f);
        }
    }

    [HarmonyPatch(typeof(CameraMovement))]
    [HarmonyPatch("LateUpdate")]
    public class CameraMovement_LateUpdatePatch
    {
        static public AccessTools.FieldRef<CameraMovement, bool> mHasNetworkControlAT =
            AccessTools.FieldRefAccess<CameraMovement, bool>("mHasNetworkControl");

        static public AccessTools.FieldRef<CameraMovement, Transform> cameraRotationAT =
            AccessTools.FieldRefAccess<CameraMovement, Transform>("cameraRotation");

        static void Postfix(CameraMovement __instance)
        {
            if (mHasNetworkControlAT(__instance))
            {
                if (!Input.GetKey(KeyCode.Mouse1))
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, SettingsMod.config.CameraFOV, Time.deltaTime * 20f);

                else if (__instance.currentWeapon && __instance.currentWeapon.currentADS && SettingsMod.config.AimCameraWobble > 0)
                {
                    Quaternion rotation = Quaternion.LookRotation(__instance.currentWeapon.currentADS.forward, Vector3.up);
                    Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, rotation, Time.deltaTime * SettingsMod.config.AimCameraWobble / 10);
                }
            }
            Transform cameraRotation = cameraRotationAT(__instance);
            cameraRotation.Rotate(Vector3.up * Input.GetAxis("Mouse X") * SettingsMod.config.MouseXSensitivity / 100, Space.World);
            cameraRotation.Rotate(cameraRotation.right * -Input.GetAxis("Mouse Y") * SettingsMod.config.MouseYSensitivity / 100, Space.World);
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
            mainCamera.fieldOfView = SettingsMod.config.CameraFOV;
            mainCamera.farClipPlane = SettingsMod.config.CameraRenderDistance;
        }
    }
}
