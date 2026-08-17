using UnityEngine;
using GameStart.Skills;
using GameStart.Class;
using GameStart.Economy;
using GameStart.Dungeons;
using GameStart.UI;
using GameStart.Player;
using GameStart.Flow;

namespace GameStart.Persistence
{
    public class PlayerSaveController : MonoBehaviour
    {
        [SerializeField] private PlayerSkills skills;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerCurrency currency;
        [SerializeField] private PlayerDungeonProgress dungeonProgress;
        [SerializeField] private QuestLog questLog;
        [SerializeField] private PlayerHealth health;

        public bool HasSave => SaveSystem.IsSaveValid();

        private void OnEnable()
        {
            if (dungeonProgress != null)
            {
                dungeonProgress.DungeonCleared += OnDungeonCleared;
            }

            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (dungeonProgress != null)
            {
                dungeonProgress.DungeonCleared -= OnDungeonCleared;
            }

            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        private void Start()
        {
            if (GameFlow.PendingNewGame)
            {
                GameFlow.PendingNewGame = false;
                StartNewGame();
                return;
            }

            // No title screen existed when this was first written - auto-continue
            // remains the fallback for entering play mode directly in this scene.
            if (HasSave)
            {
                LoadNow();
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        private void OnDungeonCleared(int index)
        {
            SaveNow();
        }

        private void OnDied()
        {
            SaveNow();
        }

        public void SaveNow()
        {
            var data = new SaveData();

            if (skills != null)
            {
                foreach (var pair in skills.AllXp)
                {
                    data.skillXp.Add(new SkillXpEntry { skill = pair.Key.ToString(), xp = pair.Value });
                }
            }

            if (inventory != null)
            {
                foreach (var slot in inventory.HotbarSlots)
                {
                    data.hotbarSlots.Add(ToSavedSlot(slot));
                }

                foreach (var slot in inventory.MainSlots)
                {
                    data.mainSlots.Add(ToSavedSlot(slot));
                }
            }

            if (currency != null)
            {
                data.gems = currency.Gems;
            }

            if (dungeonProgress != null)
            {
                data.dungeonClearedCount = dungeonProgress.ClearedCount;
            }

            if (questLog != null)
            {
                foreach (var objective in questLog.Objectives)
                {
                    data.questObjectives.Add(new SavedObjective
                    {
                        description = objective.Description,
                        targetCount = objective.TargetCount,
                        currentCount = objective.CurrentCount
                    });
                }
            }

            SaveSystem.Save(data);
        }

        /// <summary>Loads the save file into the wired components. Returns false if no save exists.</summary>
        public bool LoadNow()
        {
            SaveData data = SaveSystem.Load();
            if (data == null)
            {
                return false;
            }

            if (skills != null)
            {
                foreach (var entry in data.skillXp)
                {
                    if (System.Enum.TryParse(entry.skill, out SkillType skillType))
                    {
                        skills.LoadXp(skillType, entry.xp);
                    }
                }
            }

            if (inventory != null)
            {
                for (int i = 0; i < data.hotbarSlots.Count; i++)
                {
                    SavedSlot s = data.hotbarSlots[i];
                    inventory.LoadSlot(true, i, s.itemName, s.itemWeight, s.count);
                }

                for (int i = 0; i < data.mainSlots.Count; i++)
                {
                    SavedSlot s = data.mainSlots[i];
                    inventory.LoadSlot(false, i, s.itemName, s.itemWeight, s.count);
                }

                inventory.FinishLoading();
            }

            if (currency != null)
            {
                currency.LoadGems(data.gems);
            }

            if (dungeonProgress != null)
            {
                dungeonProgress.LoadClearedCount(data.dungeonClearedCount);
            }

            if (questLog != null)
            {
                foreach (var saved in data.questObjectives)
                {
                    questLog.SetObjectiveProgress(saved.description, saved.targetCount, saved.currentCount);
                }
            }

            return true;
        }

        /// <summary>Wipes any existing save and resets wired components to a fresh-start state.</summary>
        public void StartNewGame()
        {
            SaveSystem.DeleteSave();

            skills?.ResetAllSkills();
            inventory?.Clear();
            currency?.ResetGems();
            dungeonProgress?.ResetProgress();
        }

        private static SavedSlot ToSavedSlot(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                return new SavedSlot { itemName = "", itemWeight = 0f, count = 0 };
            }

            return new SavedSlot { itemName = slot.Item.Name, itemWeight = slot.Item.Weight, count = slot.Count };
        }
    }
}
