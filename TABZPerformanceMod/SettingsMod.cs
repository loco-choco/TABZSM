using System.IO;
using UnityEngine;
using CAMOWA;
using HarmonyLib;
using System.Reflection;

namespace TABZSettingsMod
{
    public class SettingsMod 
    {
        public static ConfigInfo config;
        public static DesyncableConfigInfo desynConfig;


        private static bool HasThePatchHappened = false;
        private static string gamePath;
        public static string GameExecutabePath
        {
            get
            {
                if (string.IsNullOrEmpty(gamePath))
                    gamePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return gamePath;
            }

            private set { }
        }

        [IMOWAModInnit("Settings Mod", 1, 1)]
        static public void ModInnit(string startingPoint)
        {
            Debug.Log("Running SettingsMod Mod");
            //Normal Settings
            string[] jsonPaths = Directory.GetFiles(GameExecutabePath, "TABZSettingsModConfig.json",SearchOption.AllDirectories);
            Camera mainCamera = Camera.main;
            if (jsonPaths.Length == 0)
            {
                Debug.Log("Config File doesn't exist, making one...");
                config = new ConfigInfo(mainCamera.fieldOfView, mainCamera.farClipPlane, 0f,2f, 0, 100, 100);
                config.ToJsonFile(GameExecutabePath);
            }
            else
                config = ConfigInfo.FromJson(jsonPaths[0]);

            //Desync Settings
            string[] desyncJsonPaths = Directory.GetFiles(GameExecutabePath, "DesyncableSettings.json", SearchOption.AllDirectories);
            if (desyncJsonPaths.Length == 0)
            {
                Debug.Log("Desyncable Config File doesn't exist, making one...");
                desynConfig = new DesyncableConfigInfo(1);
                desynConfig.ToJsonFile(GameExecutabePath);
            }
            else
                desynConfig = DesyncableConfigInfo.FromJson(desyncJsonPaths[0]);

            if (!HasThePatchHappened)
            {
                Debug.Log("Changing the camera settings");
                mainCamera.fieldOfView = config.CameraFOV;
                mainCamera.farClipPlane = config.CameraRenderDistance;

                var harmonyInstance = new Harmony("com.ivan.SettingsMod");
                harmonyInstance.PatchAll();
                HasThePatchHappened = true;
            }

            if (desynConfig.EnableDesyncableSettings)
            {
                Physics.gravity = desynConfig.WorldGravity;
            }
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
            RenderSettings.ambientIntensity = Mathf.Clamp( __instance.ambientSunCurve.Evaluate(__instance.time),SettingsMod.config.MinBrightness, SettingsMod.config.MaxBrightness);
            __instance.sun.intensity = Mathf.Clamp(__instance.sunCurve.Evaluate(__instance.time), SettingsMod.config.MinBrightness, SettingsMod.config.MaxBrightness);
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
                    Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, rotation, Time.deltaTime * SettingsMod.config.AimCameraWobble / 10f);
                }
                Transform cameraRotation = cameraRotationAT(__instance);
                cameraRotation.Rotate(Vector3.up * Input.GetAxis("Mouse X") * (SettingsMod.config.MouseXSensitivity - 100) / 100f, Space.World);
                cameraRotation.Rotate(cameraRotation.right * -Input.GetAxis("Mouse Y") * (SettingsMod.config.MouseYSensitivity - 100) / 100f, Space.World);
            }            
        }
    }
    [HarmonyPatch(typeof(DeepSky.Haze.DS_HazeView))]
    [HarmonyPatch("SetMaterialFromContext")]
    public class DS_HazeView_SetMaterialFromContextPatch
    {
        static void Prefix(ref DeepSky.Haze.DS_HazeContextItem ctx)
        {
            ctx.m_FogStartDistance = SettingsMod.config.FogStartDistance;
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

            if (SettingsMod.desynConfig.EnableDesyncableSettings)
            {
                Debug.Log("Changing the desyncable settings");
                var bH = __instance.GetComponent<BalanceHandler>();
                bH.legFallAngle = SettingsMod.desynConfig.AngleBeforeFalling;
                bH.torsoFallAngle = SettingsMod.desynConfig.AngleBeforeFalling;

                var hH = __instance.GetComponent<HealthHandler>();
                hH.MaxHealth = SettingsMod.desynConfig.PlayerHealth;
                hH.currentHealth = SettingsMod.desynConfig.PlayerHealth;

                var zB = __instance.GetComponent<ZombieBlackboard>();
                zB.RunMultiplier = SettingsMod.desynConfig.RunMultiplier;
                zB.WalkMultiplier = SettingsMod.desynConfig.WalkMultiplier;
                zB.CrouchMultiplier = SettingsMod.desynConfig.CrouchMultiplier;

                zB.JumpForce = SettingsMod.desynConfig.Jump;
                zB.GravityForce = SettingsMod.desynConfig.PlayerGravity;
            }
        }
    }
}
