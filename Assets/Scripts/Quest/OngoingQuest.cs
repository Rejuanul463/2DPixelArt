using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OngoingQuest : MonoBehaviour
{
    private Queue<(QuestData,DateTime)> onGoingQuest = new Queue<(QuestData,DateTime)>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private QuestManager questManager;
    [SerializeField] private UpgradeCounter  upgradeCounter;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<QuestItemUI> uiList;
    private GameObject go;
    private QuestItemUI _questItemUI;
    public Queue<(QuestData,DateTime)> OnGoingQuest
    {
        get => onGoingQuest;
        set => onGoingQuest = value;
    }

    void Start()
    {
    
    }
    
    public void AddQuestUI(int id, QuestData quest, DateTime startTime)
    {
        GameObject go = Instantiate(itemPrefab, content);

        QuestItemUI ui = go.GetComponent<QuestItemUI>();

        ui.Identity = id;

        uiList.Add(ui);

        onGoingQuest.Enqueue((quest, startTime));
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var questData in onGoingQuest)
        {
            foreach (var ui in uiList)
            {
                if (ui.Identity == questData.Item1.uniqueId)
                {
                    DateTime endTime =
                        questData.Item2.AddSeconds(questData.Item1.completionTime);

                    TimeSpan remaining = endTime - DateTime.UtcNow;

                    ui.UpdateUI(
                        questData.Item1.uniqueId,
                        questData.Item1.questName,
                        remaining
                    );
                }
            }
        }
    }
    
}
