﻿using Newtonsoft.Json;

namespace Fushigi.Data
{
    // All the unused functions from old fushigi are commented in case we still want to keep them
    public static class UserSettings
    {
        public static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Fushigi"
            );
        public static readonly string SettingsFile = Path.Combine(SettingsDir, "UserSettings.json");
        // public static readonly int MaxRecents = 10;
        static Settings AppSettings;

        struct Settings
        {
            public string RomFSPath;
            public string? RomFSModPath;
            // public float BackupFreqMinutes = 10;
            // public Dictionary<string, string> ModPaths;
            // public List<string> RecentCourses;
            // public bool UseGameShaders;
            // public bool UseAstcTextureCache;
            // public bool HideDeletingLinkedActorsPopup;
            // public bool UseNewCamera;

            public Settings()
            {
                //BackupFreqMinutes = 10;
                RomFSPath = "";
                // ModPaths = [];
                RomFSModPath = "";
                // RecentCourses = new List<string>(MaxRecents);
                // UseGameShaders = false;
                // UseAstcTextureCache = false;
                // HideDeletingLinkedActorsPopup = false;
                // UseNewCamera = true;
            }
        }

        public static void Load()
        {
            AppSettings = new Settings();
            if (File.Exists(SettingsFile))
                AppSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFile));
        }

        public static void Save()
        {
            if (!Directory.Exists(SettingsDir))
            {
                Directory.CreateDirectory(SettingsDir);
            }

            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(AppSettings, Formatting.Indented));
        }

        // public static bool UseGameShaders() => AppSettings.UseGameShaders;
        // public static bool UseAstcTextureCache() => AppSettings.UseAstcTextureCache;
        // public static bool HideDeletingLinkedActorsPopup() => AppSettings.HideDeletingLinkedActorsPopup;

        // public static void SetGameShaders(bool value)
        // {
        //     AppSettings.UseGameShaders = value;
        //     Save(); //save setting
        // }

        // public static void SetAstcTextureCache(bool value)
        // {
        //     AppSettings.UseAstcTextureCache = value;
        //     Save(); //save setting
        // }

        // public static void SetHideDeletingLinkedObjectsPopup(bool value)
        // {
        //     AppSettings.HideDeletingLinkedActorsPopup = value;
        //     Save(); //save setting
        // }

        public static void SetRomFSPath(string path)
        {
            AppSettings.RomFSPath = path;
            Save(); //save setting
        }

        public static void SetModRomFSPath(string path)
        {
            AppSettings.RomFSModPath = !string.IsNullOrEmpty(path) ? path:null;
            Save(); //save setting
        }

        // public static void SetUseNewCamera(bool newCamera)
        // {
        //     AppSettings.UseNewCamera = newCamera;
        //     Save();
        // }

        // public static void SetBackupFreqMinutes(float minutes)
        // {
        //     AppSettings.BackupFreqMinutes = minutes;
        //     Save();
        // }

        // public static float GetBackupFreqMinutes()
        // {
        //     if (AppSettings.BackupFreqMinutes == 0)
        //         SetBackupFreqMinutes(10);
        //     return AppSettings.BackupFreqMinutes;
        // }

        public static string GetRomFSPath()
        {
            return AppSettings.RomFSPath;
        }

        public static string? GetModRomFSPath()
        {
            return AppSettings.RomFSModPath;
        }

        // public static bool GetUseNewCamera()
        // {
        //     return AppSettings.UseNewCamera;
        // }

        // public static void AppendModPath(string modname, string path)
        // {
        //     AppSettings.ModPaths.Add(modname, path);
        // }

        // public static void AppendRecentCourse(string courseName)
        // {
        //     // please let me know if this isn't a good implementation
        //     if (AppSettings.RecentCourses.Count == MaxRecents)
        //     {
        //         // since we only store the last 10, we push our array once to the left
        //         // then our new entry is appended on the 9th index
        //         var oldArray = AppSettings.RecentCourses.ToArray();
        //         var newArray = new string?[oldArray.Length];
        //         Array.Copy(oldArray, 1, newArray, 0, oldArray.Length - 1);

        //         AppSettings.RecentCourses = [.. newArray];
        //         // put our brand new path at 9
        //         AppSettings.RecentCourses[MaxRecents - 1] = courseName;
        //     }
        //     else
        //     {
        //         AppSettings.RecentCourses.Add(courseName);
        //     }
        // }

        // public static string? GetLatestCourse()
        // {
        //     int size = AppSettings.RecentCourses.Count;
            
        //     if (size == 0)
        //     {
        //         return null;
        //     }

        //     return AppSettings.RecentCourses[size - 1];
        // }
    }
}
