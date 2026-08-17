using System;
using System.IO;
using UnityEngine;

namespace GameStart.Persistence
{
    public static class SaveSystem
    {
        // 1: original schema.
        // 2: added SaveData.equipment.
        public const int CurrentVersion = 2;

        /// <summary>Oldest schema this build can still read by migrating it forward.</summary>
        public const int MinSupportedVersion = 1;

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

                // Older schemas are migrated forward; newer ones are rejected, since a build
                // can't know what a future version added. Rejecting an older save outright
                // would strand the player's progress, which is worse than a partial load.
                if (parsed == null || parsed.version < MinSupportedVersion || parsed.version > CurrentVersion)
                {
                    Debug.LogWarning($"SaveSystem: save file failed validation (version {(parsed != null ? parsed.version.ToString() : "unreadable")}, supported {MinSupportedVersion}-{CurrentVersion}).");
                    return false;
                }

                Migrate(parsed);
                data = parsed;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem.Load failed: {e}");
                return false;
            }
        }

        /// <summary>
        /// Brings an older save up to the current schema in place.
        ///
        /// JsonUtility leaves fields absent from the JSON at their default, which for a list
        /// can be null rather than the field initializer's empty list, so every collection is
        /// null-guarded here instead of at each call site.
        /// </summary>
        private static void Migrate(SaveData data)
        {
            data.skillXp ??= new System.Collections.Generic.List<SkillXpEntry>();
            data.hotbarSlots ??= new System.Collections.Generic.List<SavedSlot>();
            data.mainSlots ??= new System.Collections.Generic.List<SavedSlot>();
            data.questObjectives ??= new System.Collections.Generic.List<SavedObjective>();

            // v1 -> v2: no equipment section. Nothing to convert; the player simply starts
            // with empty gear slots, and their inventory is untouched.
            data.equipment ??= new System.Collections.Generic.List<SavedEquipment>();

            data.version = CurrentVersion;
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
