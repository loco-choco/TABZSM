using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TABZSettingsMod
{
    [Serializable]
    public struct DesyncableConfigInfo
    {
        //Settings That might cause desyncs
        public bool EnableDesyncableSettings;
        public Vector3 WorldGravity;
        public float PlayerGravity;
        public float Jump;
        public float WalkMultiplier;
        public float RunMultiplier;
        public float CrouchMultiplier;

        public float PlayerHealth;

        public float AngleBeforeFalling;


        public DesyncableConfigInfo(object o)
        {
            EnableDesyncableSettings = false;
            WorldGravity = Physics.gravity;
            PlayerGravity = 13000f;
            Jump = 1000;
            WalkMultiplier = 0.5f;
            RunMultiplier = 1f;
            CrouchMultiplier = 0.3f;

            PlayerHealth = 100f;
            AngleBeforeFalling = 120f;
        }

        public static DesyncableConfigInfo FromJson(string path)
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<DesyncableConfigInfo>(json);
        }

        public void ToJsonFile(string path)
        {
            string file = JsonUtility.ToJson(this);
            StreamWriter s = File.CreateText(path + "/DesyncableSettings.json");
            s.Write(file);
            s.Flush();
            s.Close();
        }
    }
}
    
