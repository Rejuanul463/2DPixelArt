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

    private Dictionary<int, QuestItemUI> uiDict = new Dictionary<int, QuestItemUI>();

    void Start()
    {
        LoadOngoingQuests();
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

            if (!uiDict.TryGetValue(quest.uniqueId, out QuestItemUI ui))
                continue;

            if (remaining > TimeSpan.Zero)
            {
                // Still running
                ui.UpdateUI(quest.uniqueId, quest.questName, remaining);
                onGoingQuest.Enqueue(data);
            }
            else
            {
                // Quest timer hit zero while game was open
                ui.UpdateUI(quest.uniqueId, quest.questName, TimeSpan.Zero);
                quest.isCompleted = true;

                Debug.Log("Quest Completed: " + quest.questName);

                // Unlock hero buttons in HeroSelectionForQuestUI
                if (GameManager.Instance != null && GameManager.Instance.heroSelectionForQuestUI != null)
                {
                    GameManager.Instance.heroSelectionForQuestUI.RestoreButtons(quest.heroesForQuest);
                    GameManager.Instance.heroSelectionForQuestUI.OnQuestComplete();
                }

                // Save immediately so selectedHeroesForQuest is written as empty
                if (SaveManager.Instance != null)
                    SaveManager.Instance.SaveGame();
            }
        }
    }

    void Update()
    {
        QuestUpdate();
    }

    public void AddQuestUI(int id, QuestData quest, DateTime startTime)
    {
        GameObject go = Instantiate(itemPrefab, content);
        QuestItemUI ui = go.GetComponent<QuestItemUI>();
        ui.Identity = id;
        uiDict[id] = ui;
        onGoingQuest.Enqueue((quest, startTime));
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

                if (DateTime.UtcNow >= endTime)
                {
                    quest.isCompleted = true;
                    PlayerPrefs.DeleteKey(key);
                    Debug.Log("Offline completed: " + quest.questName);
                    continue;
                }

                // Still ongoing
                AddQuestUI(quest.uniqueId, quest, savedStart);
            }
        }

        PlayerPrefs.Save();
    }

    private string GetKey(int id)
    {
        return "QuestStart_" + id;
    }
}