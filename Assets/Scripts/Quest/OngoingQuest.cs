using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OngoingQuest : MonoBehaviour
{
    private Queue<(QuestData simulationQuestdata,string questName, DateTime startTime)> onGoingQuest = new Queue<(QuestData,string, DateTime)>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private QuestManager questManager;
    [SerializeField] private UpgradeCounter  upgradeCounter;
    public Queue<(QuestData simulationQuestdata,string questName, DateTime startTime)> OnGoingQuest
    {
        get => onGoingQuest;
        set => onGoingQuest = value;
    }

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var VARIABLE in onGoingQuest)
        {
            //upgradeCounter.CheckUpgradeProgress(VARIABLE.simulationQuestdata,VARIABLE.questName);
        }
        
    }

    public void QuestChecker(string name, DateTime startTime)
    {
        foreach (QuestData q in questManager.questData)
        {
            if (q.name == name)
            { 
                //upgradeCounter.CheckUpgradeProgress(,name);
            }
        }
    }
}
