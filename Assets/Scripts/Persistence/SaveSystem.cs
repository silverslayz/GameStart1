using System.IO;
using UnityEngine;

namespace GameStart.Persistence
{
    public static class SaveSystem
    {
        private const string FileName = "aetherfall_save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool SaveExists() => File.Exists(SavePath);

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData Load()
        {
            if (!SaveExists())
            {
                return null;
            }

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void DeleteSave()
        {
            if (SaveExists())
            {
                File.Delete(SavePath);
            }
        }
    }
}
