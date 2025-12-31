using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public QuestData quest;

    public void StartQuestBattle()
    {
        Debug.Log($"=== Starting Quest: {quest.questName} ===");

        // 1. Check Level Requirement
        if (PlayerStats.Instance.getPlayerLevel() < quest.requiredLevel)
        {
            Debug.Log($"? Player level too low! Required: {quest.requiredLevel}, You: {PlayerStats.Instance.getPlayerLevel()}");
            return;
        }

        // 2. Check Gold Requirement
        if (!PlayerStats.Instance.CanAfford(quest.requiredGoldToAttack))
        {
            Debug.Log($"? Not enough gold to start quest. Need {quest.requiredGoldToAttack}");
            return;
        }

        // Deduct gold
        //PlayerStats.Instance.SpendGold(quest.requiredGoldToAttack);

        Debug.Log($"?? Player spent {quest.requiredGoldToAttack} gold to attack.");

        // 3. Calculate combat numbers ===========================================================================will be changed
        float playerDPS = PlayerStats.Instance.getTotalDPS();
        float playerHP = PlayerStats.Instance.getTotalHP();

        float enemyTotalHP = quest.TotalHP;
        float enemyTotalDPS = quest.TotalDPS;

        Debug.Log($"Enemy Count: {quest.enemyCount}");
        Debug.Log($"Enemy Total HP: {enemyTotalHP}");
        Debug.Log($"Enemy Total DPS: {enemyTotalDPS}");

        // Battle time calculations
        float timeToKillEnemies = enemyTotalHP / playerDPS;
        float timeToKillPlayer = playerHP / enemyTotalDPS;

        // 4. Determine winner
        if (timeToKillEnemies < timeToKillPlayer)
        {
            Debug.Log("?? Player Wins the Quest!");

            // Give rewards
            PlayerStats.Instance.AddRewards(quest.experienceReward, quest.goldReward);

            Debug.Log($"?? Rewards Earned ? XP: {quest.experienceReward}, Gold: {quest.goldReward}");

            quest.isCompleted = true;
        }
        else
        {
            Debug.Log("? Player Lost the Quest...");
            Debug.Log("Tip: Try increasing HP or DPS, or deploy different hero types.");
        }

        // Battle summary log
        Debug.Log("=== Battle Summary ===");
        Debug.Log($"Player DPS: {playerDPS}, Player HP: {playerHP}");
        Debug.Log($"Enemy DPS: {enemyTotalDPS}, Enemy HP: {enemyTotalHP}");
        Debug.Log($"Time to Kill Enemies: {timeToKillEnemies}");
        Debug.Log($"Time to Die: {timeToKillPlayer}");
        Debug.Log("======================");
    }
}
