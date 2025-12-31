using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public QuestUI questUI;
    [Header("Quest Lists")]
    public List<QuestData> mainQuests = new List<QuestData>();
    public List<QuestData> sideQuests = new List<QuestData>();
    public List<QuestData> dailyQuests = new List<QuestData>();


    [Header("Quest Generation Settings")]
    public int maxSideQuests = 5;
    public int maxDailyQuests = 3;

    public QuestGenerator generator;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // generate initial quests
        generator.GenerateDailyQuests();
        generator.GenerateInitialSideQuests();
        generator.GenerateMainQuests();

        StartCoroutine(DelayedPrint());

    }

    IEnumerator DelayedPrint()
    {
        yield return new WaitForSeconds(1f);
        PrintAllQuests();
    }

    private void Update()
    {
        // If quests are low, generate new ones
        if (sideQuests.Count < 2)
            generator.GenerateSideQuest();

        if (dailyQuests.Count < 1)
            generator.GenerateDailyQuests();
    }

    public void AddSideQuest(QuestData quest)
    {
        sideQuests.Add(quest);
    }

    public void AddDailyQuest(QuestData quest)
    {
        dailyQuests.Add(quest);
    }

    public void AddMainQuest(QuestData quest)
    {
        mainQuests.Add(quest);
    }


    public void PrintAllQuests()
    {
        Debug.Log("---- Active Quests ----");
        foreach (var q in sideQuests)
        {
            Debug.Log($"Name: {q.questName}, Type: {q.questType}, Difficulty: {q.questDifficulty}, Enemies: {q.enemyCount}, Total HP: {q.TotalHP}, Total DPS: {q.TotalDPS}, Gold Required: {q.requiredGoldToAttack}");
        }
    }
}
