using UnityEngine;

public class QuestGenerator : MonoBehaviour
{
    public QuestData questTemplate; // empty ScriptableObject template

    // Hero type definitions (Gold/Attack cost per hero type)
    private static readonly float[] HeroGoldPerAttack = { 5f, 8f, 10f, 14f, 20f, 24f };
    private static readonly int[] HeroSlotCost = { 1, 1, 2, 2, 3, 3 };
    private static readonly float[] HeroDPS = { 20f, 80f, 30f, 120f, 40f, 150f };
    private static readonly float[] HeroHP = { 20f, 10f, 100f, 30f, 125f, 50f };

    [Header("Balance Settings")]
    [Tooltip("Player win margin - higher means easier quests")]
    [Range(1.1f, 2.0f)]
    public float playerWinMargin = 1.3f;

    [Header("Player Slot Configuration")]
    [Tooltip("Maximum hero slots available to player")]
    public int maxPlayerSlots = 10;

    public void GenerateInitialSideQuests()
    {
        for (int i = 0; i < 4; i++)
            GenerateSideQuest();
    }

    public void GenerateDailyQuests()
    {
        for (int i = 0; i < 2; i++)
            GenerateDailyQuest();
    }

    public void GenerateMainQuests()
    {
        // Loaded from story template
        foreach (QuestData q in QuestStoryLibrary.GetAllMainQuests())
            QuestManager.Instance.AddMainQuest(q);
    }

    // --------------------------------------------------------
    // SIDE QUEST GENERATION
    // --------------------------------------------------------

    public void GenerateSideQuest()
    {
        QuestData q = Instantiate(questTemplate);

        int playerLevel = PlayerStats.Instance.getPlayerLevel();
        float playerDPS = PlayerStats.Instance.getTotalDPS();
        float playerHP = PlayerStats.Instance.getTotalHP();

        q.questType = QuestData.QuestType.Side;
        q.questDifficulty = PickDifficulty(playerLevel);

        q.questName = GenerateQuestName(q.questDifficulty);
        q.description = "Defeat enemies in the wild that match your current strength.";

        ScaleQuestStats(q, playerLevel, playerDPS, playerHP);

        QuestManager.Instance.AddSideQuest(q);
    }

    // --------------------------------------------------------
    // DAILY QUEST GENERATION
    // --------------------------------------------------------

    public void GenerateDailyQuest()
    {
        QuestData q = Instantiate(questTemplate);

        int playerLevel = PlayerStats.Instance.getPlayerLevel();
        float playerDPS = PlayerStats.Instance.getTotalDPS();
        float playerHP = PlayerStats.Instance.getTotalHP();

        q.questType = QuestData.QuestType.Daily;
        q.questDifficulty = QuestData.QuestDifficulty.Easy;
        q.questName = "Daily Hunt";
        q.description = "Defeat a small group of monsters for daily rewards.";

        // Daily quests are always easier - 70% difficulty
        ScaleQuestStatsWithMultiplier(q, playerLevel, playerDPS, playerHP, 0.7f);

        QuestManager.Instance.AddDailyQuest(q);
    }

    // --------------------------------------------------------
    // Core Scaling Logic - HERO-BASED BALANCE
    // --------------------------------------------------------

    private void ScaleQuestStats(QuestData q, int playerLevel, float playerDPS, float playerHP)
    {
        float difficultyMultiplier = GetDifficultyMultiplier(q.questDifficulty);
        ScaleQuestStatsWithMultiplier(q, playerLevel, playerDPS, playerHP, difficultyMultiplier);
    }

    private void ScaleQuestStatsWithMultiplier(QuestData q, int playerLevel, float playerDPS, float playerHP, float difficultyMultiplier)
    {
        q.requiredLevel = Mathf.Max(1, playerLevel - 1);
        q.recommendedLevel = playerLevel;

        // Get available hero types at this level
        int unlockedHeroTypes = GetUnlockedHeroTypes(playerLevel);

        // Calculate optimal hero combination for this level
        HeroCombination optimalCombo = CalculateOptimalCombination(unlockedHeroTypes);

        // Enemy count varies by difficulty
        q.enemyCount = GetEnemyCount(q.questDifficulty);
        q.enemyName = GetEnemyName(q.questDifficulty);

        // === GOLD-BASED QUEST COST ===
        // Gold cost = Gold/Attack cost for optimal hero combination
        q.requiredGoldToAttack = Mathf.RoundToInt(optimalCombo.goldPerAttack * difficultyMultiplier);

        // === ENEMY STATS CALCULATION ===
        // Use optimal combination stats (not current player stats) to ensure fairness
        float optimalDPS = optimalCombo.totalDPS;
        float optimalHP = optimalCombo.totalHP;

        // Enemy DPS calculation - scales with difficulty
        // We want: playerHP / enemyTotalDPS > enemyTotalHP / playerDPS * winMargin
        float timeToKillEnemies = 10f * difficultyMultiplier; // Base time to kill enemies
        float enemyTotalHP = optimalDPS * timeToKillEnemies;
        q.HP = enemyTotalHP / q.enemyCount;

        // Enemy DPS - ensure player can survive long enough with margin
        float timePlayerShouldSurvive = timeToKillEnemies * playerWinMargin;
        float enemyTotalDPS = optimalHP / timePlayerShouldSurvive;

        // Distribute enemy DPS between hit rate and power
        q.hitPerSecond = Random.Range(0.8f, 1.5f) * Mathf.Sqrt(difficultyMultiplier);
        q.hitPower = enemyTotalDPS / (q.hitPerSecond * q.enemyCount);

        // === REWARD CALCULATION ===
        // Base reward calculation on gold investment and difficulty
        // Easy quests: 120-150% profit
        // Medium: 150-180% profit
        // Hard: 180-220% profit
        // SuperHard: 220-280% profit
        // Epic: 280-350% profit
        float profitMin = 1.2f + (difficultyMultiplier * 0.3f);
        float profitMax = profitMin + 0.3f;
        float profitMultiplier = Random.Range(profitMin, profitMax);

        q.goldReward = Mathf.RoundToInt(q.requiredGoldToAttack * profitMultiplier);

        // XP scales with level and difficulty
        q.experienceReward = Mathf.RoundToInt(playerLevel * 25 * difficultyMultiplier);

        q.completionTime = Random.Range(10f, 30f);

        // === VALIDATION ===
        ValidateQuest(q, playerLevel, optimalDPS, optimalHP);
    }

    // Calculate the best hero combination for available slots and unlocked types
    private HeroCombination CalculateOptimalCombination(int unlockedHeroTypes)
    {
        HeroCombination bestCombo = new HeroCombination();
        float bestEfficiency = 0f;

        // Try different combinations
        // Strategy: Fill slots with highest DPS heroes that fit
        for (int heroType = unlockedHeroTypes; heroType >= 0; heroType--)
        {
            int slotsUsed = 0;
            float totalDPS = 0f;
            float totalHP = 0f;
            float goldPerAttack = 0f;
            int heroCount = 0;

            // Fill slots with this hero type
            int slotCost = HeroSlotCost[heroType];
            while (slotsUsed + slotCost <= maxPlayerSlots)
            {
                slotsUsed += slotCost;
                totalDPS += HeroDPS[heroType];
                totalHP += HeroHP[heroType];
                goldPerAttack += HeroGoldPerAttack[heroType];
                heroCount++;
            }

            // Calculate efficiency (DPS per gold)
            float efficiency = goldPerAttack > 0 ? totalDPS / goldPerAttack : 0;

            if (efficiency > bestEfficiency || (efficiency == bestEfficiency && totalDPS > bestCombo.totalDPS))
            {
                bestEfficiency = efficiency;
                bestCombo.totalDPS = totalDPS;
                bestCombo.totalHP = totalHP;
                bestCombo.goldPerAttack = goldPerAttack;
                bestCombo.heroCount = heroCount;
                bestCombo.primaryHeroType = heroType;
            }
        }

        // If no heroes fit, use the smallest unlocked hero
        if (bestCombo.heroCount == 0 && unlockedHeroTypes >= 0)
        {
            int heroType = 0; // Type 1 (smallest slot cost)
            bestCombo.totalDPS = HeroDPS[heroType];
            bestCombo.totalHP = HeroHP[heroType];
            bestCombo.goldPerAttack = HeroGoldPerAttack[heroType];
            bestCombo.heroCount = 1;
            bestCombo.primaryHeroType = heroType;
        }

        return bestCombo;
    }

    private int GetUnlockedHeroTypes(int playerLevel)
    {
        if (playerLevel >= 10) return 5; // All 6 types (0-5)
        if (playerLevel >= 8) return 4;  // 5 types (0-4)
        if (playerLevel >= 6) return 3;  // 4 types (0-3)
        if (playerLevel >= 4) return 2;  // 3 types (0-2)
        if (playerLevel >= 2) return 1;  // 2 types (0-1)
        return 0;                         // 1 type (0)
    }

    private void ValidateQuest(QuestData q, int playerLevel, float optimalDPS, float optimalHP)
    {
        float timeToKillEnemies = q.TotalHP / optimalDPS;
        float timeToKillPlayer = optimalHP / q.TotalDPS;
        float winMargin = timeToKillPlayer / timeToKillEnemies;

        //Debug.Log($"=== Generated {q.questDifficulty} Quest: {q.questName} ===");
        //Debug.Log($"  Level Req: {q.requiredLevel} | Unlocked Heroes: {GetUnlockedHeroTypes(playerLevel) + 1} types");
        //Debug.Log($"  Enemy: {q.enemyCount}x {q.enemyName}");
        //Debug.Log($"    - HP: {q.HP:F1} each (Total: {q.TotalHP:F1})");
        //Debug.Log($"    - DPS: {q.SingleDPS:F2} each (Total: {q.TotalDPS:F2})");
        //Debug.Log($"  Optimal Hero Setup:");
        //Debug.Log($"    - DPS: {optimalDPS:F1} | HP: {optimalHP:F1}");
        //Debug.Log($"  Battle: Kill enemies in {timeToKillEnemies:F2}s | Survive {timeToKillPlayer:F2}s | Margin: {winMargin:F2}x");
        //Debug.Log($"  Economy: Cost {q.requiredGoldToAttack} gold → Reward {q.goldReward} gold ({((float)q.goldReward / q.requiredGoldToAttack):P0} profit)");

        if (winMargin < 1.0f)
        {
            //Debug.LogError($"  ⚠️ QUEST NOT WINNABLE! Player dies first!");
        }
        else if (winMargin < 1.15f)
        {
            //Debug.LogWarning($"  ⚠️ Quest very difficult - low win margin");
        }
        else
        {
            //Debug.Log($"  ✓ Quest is balanced and winnable");
        }
    }

    private int GetEnemyCount(QuestData.QuestDifficulty diff)
    {
        return diff switch
        {
            QuestData.QuestDifficulty.Easy => Random.Range(1, 3),
            QuestData.QuestDifficulty.Medium => Random.Range(2, 4),
            QuestData.QuestDifficulty.Hard => Random.Range(3, 5),
            QuestData.QuestDifficulty.SuperHard => Random.Range(4, 7),
            QuestData.QuestDifficulty.Epic => Random.Range(6, 10),
            _ => 1
        };
    }

    private string GetEnemyName(QuestData.QuestDifficulty diff)
    {
        return diff switch
        {
            QuestData.QuestDifficulty.Easy => "Wild Beast",
            QuestData.QuestDifficulty.Medium => "Feral Wolf",
            QuestData.QuestDifficulty.Hard => "Armored Raider",
            QuestData.QuestDifficulty.SuperHard => "Elite Guardian",
            QuestData.QuestDifficulty.Epic => "Ancient Titan",
            _ => "Unknown Enemy"
        };
    }

    private QuestData.QuestDifficulty PickDifficulty(int level)
    {
        float roll = Random.value;

        if (level < 5)
            return QuestData.QuestDifficulty.Easy;
        if (roll < 0.4f)
            return QuestData.QuestDifficulty.Easy;
        if (roll < 0.7f)
            return QuestData.QuestDifficulty.Medium;
        if (roll < 0.9f)
            return QuestData.QuestDifficulty.Hard;

        return QuestData.QuestDifficulty.SuperHard;
    }

    private float GetDifficultyMultiplier(QuestData.QuestDifficulty diff)
    {
        return diff switch
        {
            QuestData.QuestDifficulty.Easy => 0.5f,      // 50% difficulty
            QuestData.QuestDifficulty.Medium => 0.8f,    // 80% difficulty
            QuestData.QuestDifficulty.Hard => 1.1f,      // 110% difficulty
            QuestData.QuestDifficulty.SuperHard => 1.4f, // 140% difficulty
            QuestData.QuestDifficulty.Epic => 1.8f,      // 180% difficulty
            _ => 1f,
        };
    }

    private string GenerateQuestName(QuestData.QuestDifficulty diff)
    {
        return diff switch
        {
            QuestData.QuestDifficulty.Easy => "Forest Patrol",
            QuestData.QuestDifficulty.Medium => "Wolf Pack Ambush",
            QuestData.QuestDifficulty.Hard => "Raid of the Iron Ridge",
            QuestData.QuestDifficulty.SuperHard => "Siege of the Crimson Den",
            QuestData.QuestDifficulty.Epic => "Wrath of the Ancient Titan",
            _ => "Unknown Quest"
        };
    }

    // Helper class for hero combination calculations
    private class HeroCombination
    {
        public float totalDPS;
        public float totalHP;
        public float goldPerAttack;
        public int heroCount;
        public int primaryHeroType;
    }
}