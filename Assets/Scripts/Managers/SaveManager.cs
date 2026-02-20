using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    [Header("Assign In Inspector")]
    public List<HeroData> heroDatas = new List<HeroData>();
    public List<BuildingData> buildingDatas;
    public GuildData guildData;
    //public List<int> intList;

    public List<HeroData> SampleHeroData;

    private string SavePath =>
#if UNITY_EDITOR
        Application.dataPath + "/save.json";
#else
    Application.persistentDataPath + "/save.json";
#endif

    public void AddHero(HeroData hero)
    {
        if (!heroDatas.Contains(hero))
            heroDatas.Add(hero);

        DeleteSaveFile();
        SaveGame();
    }



    // ==========================
    // SAVE GAME
    // ==========================
    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();

        // ---- HEROES ----
        saveData.heroes = heroDatas.Select(hero => new HeroSaveData
        {
            id = hero.Id,
            uniqueId = hero.uniqueId,
            level = hero.level,
            hitPower = hero.hitPower,
            hitPerSecond = hero.hitPerSecond,
            HP = hero.HP,
            goldPerAttack = hero.goldPerAttack,
            isHeroSummoned = hero.isHeroSummoned,
            coolDownTime = hero.coolDownTime
        }).ToList();

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
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //saveData.quests = GameManager.Instance.QuestManager.questData.Select(q => new QuestSaveData
        //{
        //    isCompleted = q.isCompleted,
        //    questName = q.questName,
        //    //completeTime = q.completeTime
        //}).ToList();


        //saveData.intList = intList;

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

        // ---- HEROES ----
        foreach (var heroSave in saveData.heroes)
        {
            //var hero = heroDatas.FirstOrDefault(h => h.uniqueId == heroSave.uniqueId);
            HeroData hero = SampleHeroData[heroSave.id];

            if (hero == null) continue;

            hero.level = heroSave.level;
            hero.hitPower = heroSave.hitPower;
            hero.hitPerSecond = heroSave.hitPerSecond;
            hero.HP = heroSave.HP;
            hero.goldPerAttack = heroSave.goldPerAttack;
            hero.isHeroSummoned = heroSave.isHeroSummoned;
            hero.coolDownTime = heroSave.coolDownTime;

            heroDatas.Add(hero);
        }
        int ind = 0;
        // ---- BUILDINGS ----
        foreach (var buildingSave in saveData.buildings)
        {
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

            buildingDatas[ind] = building;
            ind++;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //for(int i = 0; i < saveData.quests.Count; i++)
        //{
        //    if (saveData.quests[i].isCompleted)
        //        GameManager.Instance.QuestManager.questData[i].CompleteTask();
        //}

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

        //intList = saveData.intList;

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

    // =========================================================
    // INTERNAL SERIALIZABLE CLASSES (INSIDE SAME SCRIPT)
    // =========================================================

    [Serializable]
    private class GameSaveData
    {
        public List<HeroSaveData> heroes;
        public List<BuildingSaveData> buildings;
        public GuildSaveData guild;
        //public List<QuestSaveData> quests;
        //public List<int> intList;
    }

    [Serializable]
    private class HeroSaveData
    {
        public int id;
        public int uniqueId;
        public int level;
        public float hitPerSecond;
        public float hitPower;
        public float HP;
        public float coolDownTime;
        public int goldPerAttack;
        public bool isHeroSummoned;
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

    //[Serializable]
    //private class QuestSaveData
    //{
    //    public string questName;
    //    public bool isCompleted;
    //    public long completeTime;
    //}



    private bool isLoaded = false;

    private void Awake()
    {
        LoadGame(); // Load when game starts
        isLoaded = true;
        
    }

    private void Start()
    {
        GameManager.Instance.heroUI.loadGame();
        GameManager.Instance.heroSelectionForQuestUI.loadGame();
        GameManager.Instance.HeroSummoner.LoadGame();
    }
    // ==========================
    // ANDROID LIFECYCLE
    // ==========================

    private void OnApplicationPause(bool pauseStatus)

    {
        if (pauseStatus)
        {
            Debug.Log("App Paused → Saving");
            DeleteSaveFile();
            SaveGame();
        }
        else if(!isLoaded)
        {
            Debug.Log("App Resumed → Loading");
            LoadGame();
            isLoaded = true;
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("App Quit → Saving");
        DeleteSaveFile();
        SaveGame();
    }

    private void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted: " + SavePath);
        }
        else
        {
            Debug.Log("No save file found to delete.");
        }
    }
}
