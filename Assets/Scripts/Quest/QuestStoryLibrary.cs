using UnityEngine;
using System.Collections.Generic;

public static class QuestStoryLibrary
{
    // Hero type definitions (must match QuestGenerator)
    private static readonly float[] HeroGoldPerAttack = { 5f, 8f, 10f, 14f, 20f, 24f };
    private static readonly int[] HeroSlotCost = { 1, 1, 2, 2, 3, 3 };
    private static readonly float[] HeroDPS = { 20f, 80f, 30f, 120f, 40f, 150f };
    private static readonly float[] HeroHP = { 20f, 10f, 100f, 30f, 125f, 50f };

    private const int MAX_PLAYER_SLOTS = 10;
    private const float PLAYER_WIN_MARGIN = 1.25f;

    public static List<QuestData> GetAllMainQuests()
    {
        List<QuestData> quests = new List<QuestData>();

        quests.AddRange(EasyMain());
        quests.AddRange(MediumMain());
        quests.AddRange(HardMain());
        quests.AddRange(SuperHardMain());
        quests.AddRange(EpicMain());

        return quests;
    }

    private static List<QuestData> EasyMain()
    {
        return new List<QuestData>()
        {
            // Level 1 - Only Hero Type 1 available (20 DPS, 20 HP, 5 gold/attack)
            MakeBalanced("Prologue: Awakening Trail",
                "Clear your first path into the Whispering Woods.",
                QuestData.QuestDifficulty.Easy, 1, 2, 1, 0.8f),
            
            // Level 2 - Hero Types 1-2 available
            MakeBalanced("Broken Bridge Bandits",
                "Bandits block your passage. Defeat them.",
                QuestData.QuestDifficulty.Easy, 2, 3, 2, 0.85f),
            
            // Level 3
            MakeBalanced("Guard the Caravan",
                "Escort a merchant caravan across the forest edge.",
                QuestData.QuestDifficulty.Easy, 3, 4, 3, 0.9f)
        };
    }

    private static List<QuestData> MediumMain()
    {
        return new List<QuestData>()
        {
            // Level 4 - Hero Types 1-3 available
            MakeBalanced("Echo Caves Mystery",
                "Strange creatures howl deep inside the Echo Caves.",
                QuestData.QuestDifficulty.Medium, 4, 6, 2, 1.0f),
            
            // Level 5
            MakeBalanced("Attack on Ridge Village",
                "Defend the village from roaming marauders.",
                QuestData.QuestDifficulty.Medium, 5, 7, 3, 1.05f),
            
            // Level 6 - Hero Types 1-4 available
            MakeBalanced("The Blackfang Stalker",
                "Hunt the powerful beast terrorizing the forest.",
                QuestData.QuestDifficulty.Medium, 6, 8, 1, 1.1f)
        };
    }

    private static List<QuestData> HardMain()
    {
        return new List<QuestData>()
        {
            // Level 7
            MakeBalanced("Siege of Ember Fort",
                "An organized army approaches Ember Fort.",
                QuestData.QuestDifficulty.Hard, 7, 10, 4, 1.3f),
            
            // Level 8 - Hero Types 1-5 available
            MakeBalanced("Depths of the Crimson Mine",
                "Corrupted miners lurk deep below.",
                QuestData.QuestDifficulty.Hard, 8, 11, 3, 1.35f),
            
            // Level 9
            MakeBalanced("Warrior of the Fallen Clan",
                "A vengeful spirit challenges anyone who enters.",
                QuestData.QuestDifficulty.Hard, 9, 12, 1, 1.4f)
        };
    }

    private static List<QuestData> SuperHardMain()
    {
        return new List<QuestData>()
        {
            // Level 10 - All hero types available
            MakeBalanced("The Obsidian Hydra",
                "A deadly hydra emerges from volcanic cracks.",
                QuestData.QuestDifficulty.SuperHard, 10, 14, 1, 1.6f),
            
            // Level 12
            MakeBalanced("Citadel of Ruin",
                "An ancient citadel awakens after centuries.",
                QuestData.QuestDifficulty.SuperHard, 12, 15, 5, 1.65f),
            
            // Level 13
            MakeBalanced("The White Flame Knight",
                "A legendary warrior blocks your path.",
                QuestData.QuestDifficulty.SuperHard, 13, 16, 1, 1.7f)
        };
    }

    private static List<QuestData> EpicMain()
    {
        return new List<QuestData>()
        {
            // Level 14
            MakeBalanced("Wrath of the Dragon King",
                "The Dragon King returns to retake his throne.",
                QuestData.QuestDifficulty.Epic, 14, 20, 1, 2.0f),
            
            // Level 15
            MakeBalanced("The Border of Reality",
                "Your world tears open as monsters overflow.",
                QuestData.QuestDifficulty.Epic, 15, 22, 6, 2.1f),
            
            // Level 20
            MakeBalanced("Final Trial: The Eternal Colossus",
                "Face the greatest titan ever recorded.",
                QuestData.QuestDifficulty.Epic, 20, 25, 1, 2.5f)
        };
    }

    private static QuestData MakeBalanced(string name, string desc, QuestData.QuestDifficulty difficulty,
        int reqLvl, int recLvl, int enemyCount, float difficultyScale)
    {
        QuestData q = ScriptableObject.CreateInstance<QuestData>();

        q.questType = QuestData.QuestType.Main;
        q.questDifficulty = difficulty;
        q.questName = name;
        q.description = desc;
        q.requiredLevel = reqLvl;
        q.recommendedLevel = recLvl;
        q.enemyCount = enemyCount;
        q.enemyName = GetStoryEnemyName(difficulty);

        // Calculate optimal hero combination for this level
        HeroCombination optimal = CalculateOptimalCombination(reqLvl);

        // === GOLD COST ===
        q.requiredGoldToAttack = Mathf.RoundToInt(optimal.goldPerAttack * difficultyScale);

        // === ENEMY STATS ===
        // Base combat time (time to kill enemies)
        float baseCombatTime = 8f + (difficulty == QuestData.QuestDifficulty.Epic ? 5f : 0f);
        float combatTime = baseCombatTime * difficultyScale;

        // Enemy HP - based on how much damage player can deal in combat time
        float enemyTotalHP = optimal.totalDPS * combatTime;
        q.HP = enemyTotalHP / enemyCount;

        // Enemy DPS - player should survive longer than combat time with margin
        float playerSurvivalTime = combatTime * PLAYER_WIN_MARGIN;
        float enemyTotalDPS = optimal.totalHP / playerSurvivalTime;

        // Distribute DPS across hit rate and power
        q.hitPerSecond = Random.Range(0.8f, 1.5f) * Mathf.Sqrt(difficultyScale);
        q.hitPower = enemyTotalDPS / (q.hitPerSecond * enemyCount);

        // === REWARDS ===
        // Profit margin based on difficulty
        float profitMultiplier = difficulty switch
        {
            QuestData.QuestDifficulty.Easy => Random.Range(1.4f, 1.6f),
            QuestData.QuestDifficulty.Medium => Random.Range(1.6f, 1.9f),
            QuestData.QuestDifficulty.Hard => Random.Range(1.9f, 2.3f),
            QuestData.QuestDifficulty.SuperHard => Random.Range(2.3f, 2.8f),
            QuestData.QuestDifficulty.Epic => Random.Range(2.8f, 3.5f),
            _ => 1.5f
        };

        q.goldReward = Mathf.RoundToInt(q.requiredGoldToAttack * profitMultiplier);
        q.experienceReward = reqLvl * 50 * (int)difficultyScale;

        // Validate and log
        ValidateMainQuest(q, optimal);

        return q;
    }

    private static HeroCombination CalculateOptimalCombination(int playerLevel)
    {
        int unlockedHeroTypes = GetUnlockedHeroTypes(playerLevel);
        HeroCombination bestCombo = new HeroCombination();
        float bestEfficiency = 0f;

        // Try each unlocked hero type to fill all slots
        for (int heroType = unlockedHeroTypes; heroType >= 0; heroType--)
        {
            int slotsUsed = 0;
            float totalDPS = 0f;
            float totalHP = 0f;
            float goldPerAttack = 0f;
            int heroCount = 0;

            int slotCost = HeroSlotCost[heroType];

            // Fill all available slots with this hero type
            while (slotsUsed + slotCost <= MAX_PLAYER_SLOTS)
            {
                slotsUsed += slotCost;
                totalDPS += HeroDPS[heroType];
                totalHP += HeroHP[heroType];
                goldPerAttack += HeroGoldPerAttack[heroType];
                heroCount++;
            }

            // Calculate DPS per gold efficiency
            float efficiency = goldPerAttack > 0 ? totalDPS / goldPerAttack : 0;

            if (efficiency > bestEfficiency || (efficiency == bestEfficiency && totalDPS > bestCombo.totalDPS))
            {
                bestEfficiency = efficiency;
                bestCombo.totalDPS = totalDPS;
                bestCombo.totalHP = totalHP;
                bestCombo.goldPerAttack = goldPerAttack;
                bestCombo.heroCount = heroCount;
                bestCombo.heroType = heroType;
            }
        }

        // Fallback: if no heroes fit, use smallest hero
        if (bestCombo.heroCount == 0 && unlockedHeroTypes >= 0)
        {
            bestCombo.totalDPS = HeroDPS[0];
            bestCombo.totalHP = HeroHP[0];
            bestCombo.goldPerAttack = HeroGoldPerAttack[0];
            bestCombo.heroCount = 1;
            bestCombo.heroType = 0;
        }

        return bestCombo;
    }

    private static int GetUnlockedHeroTypes(int playerLevel)
    {
        if (playerLevel >= 10) return 5; // All 6 types (0-5)
        if (playerLevel >= 8) return 4;  // 5 types (0-4)
        if (playerLevel >= 6) return 3;  // 4 types (0-3)
        if (playerLevel >= 4) return 2;  // 3 types (0-2)
        if (playerLevel >= 2) return 1;  // 2 types (0-1)
        return 0;                         // 1 type (0)
    }

    private static void ValidateMainQuest(QuestData q, HeroCombination optimal)
    {
        float timeToKillEnemies = q.TotalHP / optimal.totalDPS;
        float timeToKillPlayer = optimal.totalHP / q.TotalDPS;
        float winMargin = timeToKillPlayer / timeToKillEnemies;

        //Debug.Log($"=== Main Quest Created: {q.questName} ===");
        //Debug.Log($"  Level {q.requiredLevel} | Difficulty: {q.questDifficulty} | Enemies: {q.enemyCount}x {q.enemyName}");
        //Debug.Log($"  Unlocked Heroes: {GetUnlockedHeroTypes(q.requiredLevel) + 1} types | Best: Type {optimal.heroType + 1} x{optimal.heroCount}");
        //Debug.Log($"  Optimal Stats: DPS={optimal.totalDPS:F1}, HP={optimal.totalHP:F1}, Gold/Atk={optimal.goldPerAttack:F1}");
        //Debug.Log($"  Enemy Stats: HP={q.HP:F1} each ({q.TotalHP:F1} total), DPS={q.SingleDPS:F2} each ({q.TotalDPS:F2} total)");
        //Debug.Log($"  Battle: Kill in {timeToKillEnemies:F2}s | Survive {timeToKillPlayer:F2}s | Win Margin: {winMargin:F2}x");
        //Debug.Log($"  Economy: Cost {q.requiredGoldToAttack}g → Reward {q.goldReward}g ({((float)q.goldReward / q.requiredGoldToAttack * 100 - 100):F0}% profit) + {q.experienceReward} XP");

        if (winMargin < 1.0f)
        {
            //Debug.LogError($"  ❌ UNWINNABLE - Player will die first!");
        }
        else if (winMargin < 1.1f)
        {
            //Debug.LogWarning($"  ⚠️ Very tight - low safety margin");
        }
        else
        {
            //Debug.Log($"  ✅ Balanced and winnable");
        }
        //Debug.Log("");
    }

    private static string GetStoryEnemyName(QuestData.QuestDifficulty diff)
    {
        return diff switch
        {
            QuestData.QuestDifficulty.Easy => "Bandit",
            QuestData.QuestDifficulty.Medium => "Beast",
            QuestData.QuestDifficulty.Hard => "Corrupted Warrior",
            QuestData.QuestDifficulty.SuperHard => "Elite Boss",
            QuestData.QuestDifficulty.Epic => "Legendary Titan",
            _ => "Story Enemy"
        };
    }

    private class HeroCombination
    {
        public float totalDPS;
        public float totalHP;
        public float goldPerAttack;
        public int heroCount;
        public int heroType;
    }
}