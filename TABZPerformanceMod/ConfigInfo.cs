using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TABZSettingsMod
{
    [Serializable]
    public struct ConfigInfo
    {
        public float CameraFOV;
        public float CameraRenderDistance;
        public float FogStartDistance;
        public float MinBrightness;
        public float MaxBrightness;

        //Porcentages
        public int AimCameraWobble;
        public int MouseXSensitivity;
        public int MouseYSensitivity;


        public ConfigInfo(float CameraFOV, float CameraRenderDistance, float MinBrightness, float MaxBrightness, int AimCameraWobble, int MouseXSensitivity, int MouseYSensitivity)
        {
            this.CameraFOV = CameraFOV;
            this.CameraRenderDistance = CameraRenderDistance;
            this.MinBrightness = MinBrightness;
            this.MaxBrightness = MaxBrightness;
            this.AimCameraWobble = AimCameraWobble;
            this.MouseXSensitivity = MouseXSensitivity;
            this.MouseYSensitivity = MouseYSensitivity;

            FogStartDistance = 0f;
        }

        public static ConfigInfo FromJson(string path)
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ConfigInfo>(json);
        }

        public void ToJsonFile(string path)
        {
            string file = JsonUtility.ToJson(this);
            StreamWriter s = File.CreateText(path + "/TABZSettingsModConfig.json");
            s.Write(file);
            s.Flush();
            s.Close();
        }
    }
}
