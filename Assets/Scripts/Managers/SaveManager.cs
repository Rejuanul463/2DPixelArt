using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [Header("Assign In Inspector")]
    public List<HeroData> heroDatas = new List<HeroData>();
    public List<BuildingData> buildingDatas;
    public GuildData guildData;
    public List<HeroData> SampleHeroData;
    public PannelManager pannelManager;
    public OngoingQuest ongoingQuest;
    public List<GameObject> sceneObjects;

    private string SavePath =>
#if UNITY_EDITOR
        Application.dataPath + "/save.json";
#else
        Application.persistentDataPath + "/save.json";
#endif

    private bool isLoaded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load everything EXCEPT hero-selection UI (buttons don't exist yet)
        LoadGame();
        isLoaded = true;
    }

    private void Start()
    {
        // Buttons are created here — order matters:
        // 1. Build hero UI buttons
        GameManager.Instance.heroUI.loadGame();
        GameManager.Instance.heroSelectionForQuestUI.loadGame();

        // 2. Restore hero summoner state
        GameManager.Instance.HeroSummoner.LoadGame();

        // 3. Now that buttons exist, restore selected-hero state
        RestoreHeroSelectionUI();

        // 4. Restore quest panel button states
        GameManager.Instance.pannelManager.RestoreHeroSelectionState();
    }

    // ==========================
    // HERO SELECTION RESTORE
    // ==========================
    // Stored temporarily during LoadGame() (Awake) until Start() can apply it.
    private List<int> _pendingSelectedHeroes = new List<int>();

    private void RestoreHeroSelectionUI()
    {
        if (_pendingSelectedHeroes == null || _pendingSelectedHeroes.Count == 0) return;
        GameManager.Instance.heroSelectionForQuestUI.LoadSelectedHeroes(_pendingSelectedHeroes);
        _pendingSelectedHeroes.Clear();
    }

    // ==========================
    // SAVE GAME
    // ==========================
    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();

        // ---- SELECTED HEROES FOR QUEST ----
        // Only save heroes that are currently active/locked (Item2 == true)
        saveData.selectedHeroesForQuest = GameManager.Instance.heroSelectionForQuestUI.SelectedHeroes
            .Where(h => h.Item2)
            .Select(h => h.Item1)
            .ToList();

        // ---- HEROES ----
        saveData.heroes = heroDatas.Select(hero =>
        {
            var heroObj = GameObject.FindObjectsOfType<Hero>()
                .FirstOrDefault(h => h.heroData.uniqueId == hero.uniqueId);
            Vector3 pos = heroObj != null ? heroObj.transform.position : Vector3.zero;

            return new HeroSaveData
            {
                name = hero.name,
                id = hero.Id,
                uniqueId = hero.uniqueId,
                level = hero.level,
                hitPower = hero.hitPower,
                hitPerSecond = hero.hitPerSecond,
                HP = hero.HP,
                goldPerAttack = hero.goldPerAttack,
                isHeroSummoned = hero.isHeroSummoned,
                coolDownTime = hero.coolDownTime,
                position = pos
            };
        }).ToList();

        // ---- ONGOING QUESTS ----
        saveData.ongoingQuests = new List<OngoingQuestSaveData>();
        foreach (var entry in ongoingQuest.OngoingQuests)
        {
            saveData.ongoingQuests.Add(new OngoingQuestSaveData
            {
                questUniqueId = entry.quest.uniqueId,
                startTime = entry.startTime.ToString("o"),
                heroesForQuest = entry.quest.heroesForQuest
            });
        }

        // ---- BUILDINGS ----
        saveData.buildings = buildingDatas.Select(b => new BuildingSaveData
        {
            buildingID = b.buildingID,
            buildingLevel = b.buildingLevel,
            isUnderUpgrade = b.isUnderUpgrade,
            isUpgradable = b.isUpgradable,
            upgradeTime = b.upgradeTime,
            upgradeStartTime = b.upgradeStartTime,
            upgradeCostGold = b.upgradeCostGold,
            upgradeCostWood = b.upgradeCostWood,
            upgradeCostStone = b.upgradeCostStone,
            xpBoost = b.xpBoost
        }).ToList();

        // ---- GUILD ----
        saveData.guild = new GuildSaveData
        {
            guildLevel = guildData.guildLevel,
            currentExperience = guildData.currentExperience,
            experienceToNextLevel = guildData.experienceToNextLevel,
            gold = guildData.gold,
            gems = guildData.gems,
            woods = guildData.woods,
            stones = guildData.stones,
            BlackSmithLevel = guildData.BlackSmithLevel,
            HeroSummonerLevel = guildData.HeroSummonerLevel,
            maxUnlockableHeroID = guildData.maxUnlockableHeroID,
            unlockedHeroID = guildData.unlockedHeroID,
            questCompleteTime = guildData.questCompleteTime
        };

        // ---- QUESTS ----
        saveData.quests = GameManager.Instance.QuestManager.questData.Select(q => new QuestSaveData
        {
            isCompleted = q.isCompleted,
            questName = q.questName,
            heroesForQuest = q.heroesForQuest, // Store hero indices for quest tracking
        }).ToList();

        // ---- SCENE OBJECTS ----
        saveData.sceneObjects = new List<SceneObjectSaveData>();
        foreach (var obj in sceneObjects)
        {
            if (obj == null) continue;
            saveData.sceneObjects.Add(new SceneObjectSaveData
            {
                objectName = obj.name,
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,
                isActive = obj.activeSelf
            });
        }

        string[] tags = { "Heroes" };
        foreach (var tag in tags)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (obj == null) continue;
                saveData.sceneObjects.Add(new SceneObjectSaveData
                {
                    objectName = obj.name,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation,
                    scale = obj.transform.localScale,
                    isActive = obj.activeSelf
                });
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("GAME SAVED");
    }

    // ==========================
    // LOAD GAME
    // ==========================
    private void LoadGame()
    {
        isLoaded = true;
        if (!File.Exists(SavePath))
        {
            Debug.Log("NO SAVE FILE FOUND");
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        // ---- SELECTED HEROES FOR QUEST ----
        // Store for later — UI buttons don't exist yet in Awake
        if (saveData.selectedHeroesForQuest != null && saveData.selectedHeroesForQuest.Count > 0)
        {
            _pendingSelectedHeroes = new List<int>(saveData.selectedHeroesForQuest);
        }

        // ---- ONGOING QUESTS ----
        if (saveData.ongoingQuests != null)
        {
            foreach (var saved in saveData.ongoingQuests)
            {
                QuestData quest = GameManager.Instance.QuestManager.questData
                    .FirstOrDefault(q => q.uniqueId == saved.questUniqueId);
                quest.heroesForQuest = saved.heroesForQuest; // Restore hero indices for quest tracking
                if (quest == null || quest.isCompleted) continue;

                DateTime startTime = DateTime.Parse(
                    saved.startTime,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind
                );
                DateTime endTime = startTime.AddSeconds(quest.completionTime);

                if (DateTime.UtcNow >= endTime)
                {
                    quest.isCompleted = true;
                    GameManager.Instance.heroSelectionForQuestUI.RestoreButtons(quest.heroesForQuest); // Unlock buttons since quest is done
                    continue;
                }

                ongoingQuest.AddQuestUI(quest.uniqueId, quest, startTime);
            }
        }

        // ---- HEROES ----
        foreach (var heroSave in saveData.heroes)
        {
            HeroData hero = SampleHeroData[heroSave.id];
            if (hero == null) continue;

            hero.name = heroSave.name;
            hero.uniqueId = heroSave.uniqueId;
            hero.Id = heroSave.id;
            hero.level = heroSave.level;
            hero.hitPower = heroSave.hitPower;
            hero.hitPerSecond = heroSave.hitPerSecond;
            hero.HP = heroSave.HP;
            hero.goldPerAttack = heroSave.goldPerAttack;
            hero.isHeroSummoned = heroSave.isHeroSummoned;
            hero.coolDownTime = heroSave.coolDownTime;

            heroDatas.Add(hero);

            var heroObj = GameObject.FindObjectsOfType<Hero>()
                .FirstOrDefault(h => h.heroData.uniqueId == hero.uniqueId);
            if (heroObj != null)
                heroObj.transform.position = heroSave.position;
        }

        // ---- BUILDINGS ----
        for (int i = 0; i < saveData.buildings.Count; i++)
        {
            var buildingSave = saveData.buildings[i];
            var building = buildingDatas.FirstOrDefault(b => b.buildingID == buildingSave.buildingID);
            if (building == null) continue;

            building.buildingLevel = buildingSave.buildingLevel;
            building.isUnderUpgrade = buildingSave.isUnderUpgrade;
            building.isUpgradable = buildingSave.isUpgradable;
            building.upgradeTime = buildingSave.upgradeTime;
            building.upgradeStartTime = buildingSave.upgradeStartTime;
            building.upgradeCostGold = buildingSave.upgradeCostGold;
            building.upgradeCostWood = buildingSave.upgradeCostWood;
            building.upgradeCostStone = buildingSave.upgradeCostStone;
            building.xpBoost = buildingSave.xpBoost;

            if (!building.isUnderUpgrade)
                building.CompleteUpgrade();

            buildingDatas[i] = building;
        }

        // ---- QUESTS ----
        for (int i = 0; i < saveData.quests.Count; i++)
        {
            GameManager.Instance.QuestManager.questData[i].heroesForQuest = saveData.quests[i].heroesForQuest; // Restore hero indices for quest tracking
            if (saveData.quests[i].isCompleted)
                GameManager.Instance.QuestManager.questData[i].CompleteTask();
        }
        GameManager.Instance.QuestManager.loadQuests();

        // ---- GUILD ----
        guildData.guildLevel = saveData.guild.guildLevel;
        guildData.currentExperience = saveData.guild.currentExperience;
        guildData.experienceToNextLevel = saveData.guild.experienceToNextLevel;
        guildData.gold = saveData.guild.gold;
        guildData.gems = saveData.guild.gems;
        guildData.woods = saveData.guild.woods;
        guildData.stones = saveData.guild.stones;
        guildData.BlackSmithLevel = saveData.guild.BlackSmithLevel;
        guildData.HeroSummonerLevel = saveData.guild.HeroSummonerLevel;
        guildData.maxUnlockableHeroID = saveData.guild.maxUnlockableHeroID;
        guildData.unlockedHeroID = saveData.guild.unlockedHeroID;
        guildData.questCompleteTime = saveData.guild.questCompleteTime;

        // ---- RESTORE SCENE OBJECTS ----
        foreach (var objSave in saveData.sceneObjects)
        {
            GameObject obj = sceneObjects.FirstOrDefault(o => o.name == objSave.objectName);
            if (obj != null)
            {
                obj.transform.position = objSave.position;
                obj.transform.rotation = objSave.rotation;
                obj.transform.localScale = objSave.scale;
                obj.SetActive(objSave.isActive);
            }
        }

        Debug.Log("GAME LOADED");
    }

    // ==========================
    // DELETE SAVE
    // ==========================
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("SAVE DELETED");
        }
    }

    // ==========================
    // SERIALIZABLE CLASSES
    // ==========================
    [Serializable]
    private class GameSaveData
    {
        public List<HeroSaveData> heroes;
        public List<BuildingSaveData> buildings;
        public GuildSaveData guild;
        public List<QuestSaveData> quests;
        public List<SceneObjectSaveData> sceneObjects;
        public List<OngoingQuestSaveData> ongoingQuests;
        public List<int> selectedHeroesForQuest;
    }

    [Serializable]
    private class OngoingQuestSaveData
    {
        public int questUniqueId;
        public string startTime;
        public List<int> heroesForQuest; // Store hero indices for quest tracking
    }

    [Serializable]
    private class HeroSaveData
    {
        public string name;
        public int id;
        public int uniqueId;
        public int level;
        public float hitPerSecond;
        public float hitPower;
        public float HP;
        public float coolDownTime;
        public int goldPerAttack;
        public bool isHeroSummoned;
        public Vector3 position;
    }

    [Serializable]
    private class BuildingSaveData
    {
        public int buildingID;
        public int buildingLevel;
        public bool isUnderUpgrade;
        public bool isUpgradable;
        public long upgradeTime;
        public long upgradeStartTime;
        public int upgradeCostGold;
        public int upgradeCostWood;
        public int upgradeCostStone;
        public float xpBoost;
    }

    [Serializable]
    private class GuildSaveData
    {
        public int guildLevel;
        public int currentExperience;
        public int experienceToNextLevel;
        public int gold;
        public bool[] unlockedHeroID;
        public int maxUnlockableHeroID;
        public int gems;
        public int woods;
        public int stones;
        public int BlackSmithLevel;
        public int HeroSummonerLevel;
        public long[] questCompleteTime;
    }

    [Serializable]
    private class QuestSaveData
    {
        public string questName;
        public bool isCompleted;
        public long completeTime;
        public List<int> heroesForQuest; // Store hero indices for quest tracking
    }

    [Serializable]
    private class SceneObjectSaveData
    {
        public string objectName;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool isActive;
    }

    // ==========================
    // ANDROID LIFECYCLE
    // ==========================
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("App Paused → Saving");
            SaveGame();
        }
        else if (!isLoaded)
        {
            Debug.Log("App Resumed → Loading");
            LoadGame();
            isLoaded = true;
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("App Quit → Saving");
        SaveGame();
    }
}