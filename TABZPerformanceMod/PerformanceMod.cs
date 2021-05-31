using System.IO;
using UnityEngine;
using CAMOWA;

namespace TABZPerformanceMod
{
    public class PerformanceMod 
    {
        [IMOWAModInnit("Performance Mod", 1, 1)]
        static public void ModInnit(string startingPoint)
        {
            ConfigInfo config;
            Camera mainCamera = Camera.main;
            string[] jsonPaths = Directory.GetFiles(Directory.GetCurrentDirectory(), "TBZPerformanceModConfig.json");
            if(jsonPaths.Length == 0)
            {
                Debug.Log("Config File doesn't exist, making one...");
                config = new ConfigInfo(mainCamera.fieldOfView, mainCamera.farClipPlane);
                config.ToJsonFile(Directory.GetCurrentDirectory());
            }
            config = ConfigInfo.FromJson(jsonPaths[0]);
            mainCamera.fieldOfView = config.CameraFOV;
            mainCamera.farClipPlane = config.CameraRenderDistance;
        }
    }
}
