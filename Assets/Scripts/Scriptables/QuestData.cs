using System.Collections.Generic;
using UnityEngine;
using static QuestData;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    public enum QuestType { Main, Side, Daily }
    public enum QuestDifficulty { Easy, Medium, Hard, SuperHard, Epic }

    [Header("Quest Info")]
    public string questName;
    public QuestType questType;
    public QuestDifficulty questDifficulty;
    [TextArea] public string description;
    [SerializeField] private Sprite questIcon;

    [Header("Requirements")]
    [SerializeField] public int requiredLevel;
    [SerializeField] public int minTeamSize = 1;
    [SerializeField] public int maxTeamSize = 4;
    [SerializeField] public int estimatedDuration = 1; // In hours

    [Header("Rewards")]
    [SerializeField] public int goldRewardBase;
    [SerializeField] public int goldRewardBonus;
    [SerializeField] public int experienceReward;
    [SerializeField] public int experienceRewardBonus;
    [SerializeField] public int WoodReward;
    [SerializeField] public int StoneReward;


    [Header("Location")]
    [SerializeField] public string location = "";
    [SerializeField] public bool isInCity = true;
    [SerializeField] public int travelDistance = 0; // Distance from city in kilometers


    [Header("Enemy Info")]
    public string enemyName;
    public float hitPerSecond;
    public float hitPower;
    public float HP;
    public int enemyCount = 1;

    [Header("State")]
    public float completionTime;
    public bool isCompleted;

    public int slots;

    public float SingleDPS => hitPerSecond * hitPower;
    public float TotalDPS => SingleDPS * enemyCount;
    public float TotalHP => HP * enemyCount;

    public void CompleteTask()
    {
        isCompleted = true;
    }

    public bool isComplete()
    {
        return isCompleted;
    }
}

