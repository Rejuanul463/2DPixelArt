using System;
using System.Collections.Generic;
using UnityEngine;

public class OngoingQuest : MonoBehaviour
{
    private Queue<(QuestData quest, DateTime startTime)> onGoingQuest
        = new Queue<(QuestData, DateTime)>();

    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject itemPrefab;

    // 🔥 Faster lookup (no Find)
    private Dictionary<int, QuestItemUI> uiDict = new Dictionary<int, QuestItemUI>();

    void Start()
    {
        LoadOngoingQuests();
    }

    void Update()
    {
        if (onGoingQuest.Count == 0) return;

        int count = onGoingQuest.Count;

        for (int i = 0; i < count; i++)
        {
            var data = onGoingQuest.Dequeue();

            QuestData quest = data.quest;
            DateTime startTime = data.startTime;

            DateTime endTime = startTime.AddSeconds(quest.completionTime);
            TimeSpan remaining = endTime - DateTime.UtcNow;

            // Try get UI safely
            if (!uiDict.TryGetValue(quest.uniqueId, out QuestItemUI ui))
                continue;

            if (remaining > TimeSpan.Zero)
            {
                // 🟢 Ongoing
                ui.UpdateUI(quest.uniqueId, quest.questName, remaining);

                // Put back in queue
                onGoingQuest.Enqueue(data);
            }
            else
            {
                // ✅ COMPLETED
                ui.UpdateUI(quest.uniqueId, quest.questName, TimeSpan.Zero);

                quest.isCompleted = true;

                /*// ❌ Remove from save
                PlayerPrefs.DeleteKey(GetKey(quest.uniqueId));

                // ❌ Remove UI
                Destroy(ui.gameObject);
                uiDict.Remove(quest.uniqueId);*/

                Debug.Log($"✅ Quest Completed: {quest.questName}");
            }
        }
    }

    /*public void DestroyUI(GameObject obj)
    {
        Destroy(ui.gameObject);
        uiDict.Remove(quest.uniqueId);
    }*/
    // ✅ ADD QUEST
    public void AddQuestUI(int id, QuestData quest, DateTime startTime)
    {
        GameObject go = Instantiate(itemPrefab, content);
        QuestItemUI ui = go.GetComponent<QuestItemUI>();
        ui.Identity = id;
        uiDict[id] = ui;
        onGoingQuest.Enqueue((quest, startTime));
        // 💾 SAVE
        PlayerPrefs.SetString(GetKey(id), startTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    // ✅ LOAD QUESTS (OFFLINE SUPPORT)
    void LoadOngoingQuests()
    {
        QuestData[] allQuests = Resources.LoadAll<QuestData>(""); // Or your quest folder

        foreach (var quest in allQuests)
        {
            string key = GetKey(quest.uniqueId);

            // Check if quest was started but not completed
            if (PlayerPrefs.HasKey(key) && !quest.isCompleted)
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString(key));
                DateTime savedStart = DateTime.FromBinary(binary);

                // Recreate UI + add to queue
                AddQuestUI(quest.uniqueId, quest, savedStart);
            }
        }
    }
    // 🔑 Key generator
    private string GetKey(int id)
    {
        return "QuestStart_" + id;
    }
}