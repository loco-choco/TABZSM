using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TABZPerformanceMod
{
    [Serializable]
    public struct ConfigInfo
    {
        public float CameraFOV;
        public float CameraRenderDistance;

        public ConfigInfo(float CameraFOV, float CameraRenderDistance)
        {
            this.CameraFOV = CameraFOV;
            this.CameraRenderDistance = CameraRenderDistance;
        }

        public static ConfigInfo FromJson(string path)
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ConfigInfo>(json);
        }

        public void ToJsonFile(string path)
        {
            string file = JsonUtility.ToJson(this);
            StreamWriter s = File.CreateText(path + "/TBZPerformanceModConfig.json");
            s.Write(file);
            s.Flush();
            s.Close();
        }
    }
}
