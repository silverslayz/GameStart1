using System;
using System.IO;
using UnityEngine;

namespace GameStart.Persistence
{
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;

        private const string FileName = "aetherfall_save.json";
        private const string TempFileName = "aetherfall_save.json.tmp";
        private const string BackupFileName = "aetherfall_save.json.bak";

        private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        private static string TempPath => Path.Combine(Application.persistentDataPath, TempFileName);
        private static string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        public static bool SaveExists() => File.Exists(SavePath);

        /// <summary>True only if a save file exists AND actually parses as a supported version - use this to gate Continue, not SaveExists().</summary>
        public static bool IsSaveValid() => TryLoad(out _);

        /// <summary>Writes via temp file + File.Replace so a crash mid-write can't destroy the only save; the prior save is kept as a .bak.</summary>
        public static void Save(SaveData data)
        {
            data.version = CurrentVersion;
            string json = JsonUtility.ToJson(data, true);

            try
            {
                File.WriteAllText(TempPath, json);

                if (File.Exists(SavePath))
                {
                    File.Replace(TempPath, SavePath, BackupPath);
                }
                else
                {
                    File.Move(TempPath, SavePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem.Save failed: {e}");
            }
        }

        /// <summary>Returns null if no save exists, it's unreadable, corrupt, or from an unsupported version - never throws.</summary>
        public static SaveData Load()
        {
            TryLoad(out SaveData data);
            return data;
        }

        private static bool TryLoad(out SaveData data)
        {
            data = null;

            if (!File.Exists(SavePath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData parsed = JsonUtility.FromJson<SaveData>(json);

                // No migration path exists yet - any version mismatch is rejected rather than
                // risking a silent partial load (JsonUtility zero-fills missing fields instead
                // of failing). Add migration steps here once CurrentVersion moves past 1.
                if (parsed == null || parsed.version != CurrentVersion)
                {
                    Debug.LogWarning($"SaveSystem: save file failed validation (version {(parsed != null ? parsed.version.ToString() : "unreadable")}, expected {CurrentVersion}).");
                    return false;
                }

                data = parsed;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem.Load failed: {e}");
                return false;
            }
        }

        public static void DeleteSave()
        {
            TryDelete(SavePath);
            TryDelete(BackupPath);
            TryDelete(TempPath);
        }

        private static void TryDelete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
