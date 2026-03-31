using System;
using System.Collections.Generic;
using UnityEngine;

public class OngoingQuest : MonoBehaviour
{
    private Queue<(QuestData quest, DateTime startTime)> onGoingQuest
        = new Queue<(QuestData, DateTime)>();

    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject itemPrefab;

    public Queue<(QuestData quest, DateTime startTime)> OngoingQuests
    {
        get => onGoingQuest;
        set => onGoingQuest = value;
    }
    // 🔥 Faster lookup (no Find)
    private Dictionary<int, QuestItemUI> uiDict = new Dictionary<int, QuestItemUI>();

    void Start()
    {
    
        LoadOngoingQuests(); // 👈 Load first
        QuestUpdate();  
    }

    public void QuestUpdate()
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
                Debug.Log($"✅ Quest Completed: {quest.questName}");
                // NOTE: Do NOT call RestoreButtons here — heroes remain permanently
                // locked and removed from the scene after being sent on a quest.
            }
        }
    }
    void Update()
    {
       QuestUpdate();
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
    
    public void RebuildUI()
    {
        foreach (var data in onGoingQuest)
        {
            GameObject go = Instantiate(itemPrefab, content);
            QuestItemUI ui = go.GetComponent<QuestItemUI>();

            ui.Identity = data.quest.uniqueId;
            uiDict[data.quest.uniqueId] = ui;
        }
    }
    // ✅ LOAD QUESTS (OFFLINE SUPPORT)
    void LoadOngoingQuests()
    {
        QuestData[] allQuests = Resources.LoadAll<QuestData>("Quests");

        foreach (var quest in allQuests)
        {
            string key = GetKey(quest.uniqueId);

            if (PlayerPrefs.HasKey(key) && !quest.isCompleted)
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString(key));
                DateTime savedStart = DateTime.FromBinary(binary);

                DateTime endTime = savedStart.AddSeconds(quest.completionTime);

                // ✅ Already finished while offline
                if (DateTime.UtcNow >= endTime)
                {
                    quest.isCompleted = true;
                    PlayerPrefs.DeleteKey(key);
                    Debug.Log($"✅ Offline completed: {quest.questName}");
                    continue;
                }

                // ⏳ Still ongoing — rebuild it
                AddQuestUI(quest.uniqueId, quest, savedStart);
            }
        }

        PlayerPrefs.Save();
    }
    // 🔑 Key generator
    private string GetKey(int id)
    {
        return "QuestStart_" + id;
    }
}