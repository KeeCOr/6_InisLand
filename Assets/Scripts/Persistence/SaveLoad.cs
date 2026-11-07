using System;
using System.IO;
using UnityEngine;

namespace IL6
{
    [Serializable]
    public class SaveFileV1
    {
        public int version = 1;
        public string savedAtUtc;
        public bool npcTutorialEnabled;
        public bool storyIntroSeen;
        public bool storyTutorialEnabled;
        public bool storyTutorialComplete;
        public int storyTutorialStage;
        public bool storyVillageRuined;
        public int[] researchLevels;
        public int currentDay = 1;
        public ResourceSnapshot resources = new();
        public uint weatherRng = 42;
        public bool pregnancyActive;
        public int pregnancyStartDay;
        public int pregnancyDueDay;
        public string pregnancyParents;
        public WorldSave world;
        public bool hasWorld;
    }

    public static class SaveLoad
    {
        public const string FileName = "save_v1.json";
        public const int SlotCount=3;
        public static int ActiveSlot {get;private set;}=1;
        public static void SelectSlot(int slot)
        {
            if(slot<1 || slot>SlotCount)throw new ArgumentOutOfRangeException(nameof(slot));
            ActiveSlot=slot;
        }
        public static string StoragePath => FilePath;
        public static string SlotPath(int slot)
        {
            if(slot<1 || slot>SlotCount)throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(StorageDirectory,slot==1?FileName:$"save_slot_{slot}_v1.json");
        }
        public static bool HasSlotFile(int slot)=>File.Exists(SlotPath(slot));
        private static string FilePath=>SlotPath(ActiveSlot);

        private static string StorageDirectory
        {
            get
            {
#if UNITY_EDITOR
                // Batch validation must never overwrite the player's actual save.
                string testDirectory=Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY");
                if(!string.IsNullOrEmpty(testDirectory) && Path.IsPathRooted(testDirectory))
                {Directory.CreateDirectory(testDirectory);return testDirectory;}
#endif
                return Application.persistentDataPath;
            }
        }

        public static bool Save(SaveFileV1 data)
        {
            try
            {
                if(data==null || !WorldPersistence.IsValid(data.world))return false;
                data.hasWorld=data.world!=null;
                data.savedAtUtc=DateTime.UtcNow.ToString("O");
                string json = JsonUtility.ToJson(data, prettyPrint: false);
                Directory.CreateDirectory(StorageDirectory);
                string temp=FilePath+".tmp";
                File.WriteAllText(temp,json);
                if(File.Exists(FilePath))File.Replace(temp,FilePath,null);
                else File.Move(temp,FilePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoad] save failed: {ex}");
                return false;
            }
        }

        public static SaveFileV1 Load()=>LoadSlot(ActiveSlot);
        public static SaveFileV1 LoadSlot(int slot)
        {
            try
            {
                string path=SlotPath(slot);
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                var parsed = JsonUtility.FromJson<SaveFileV1>(json);
                if (parsed == null || parsed.version != 1 || parsed.currentDay<1) return null;
                if(!parsed.hasWorld)parsed.world=null;
                else if(parsed.world==null || !WorldPersistence.IsValid(parsed.world))return null;
                return parsed;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveLoad] slot {slot} could not be read: {ex.Message}");
                return null;
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoad] clear failed: {ex}");
            }
        }
    }
}
