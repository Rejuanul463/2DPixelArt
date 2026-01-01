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
    [SerializeField] private int requiredLevel;
    [SerializeField] private int minTeamSize = 1;
    [SerializeField] private int maxTeamSize = 4;
    [SerializeField] private int estimatedDuration = 1; // In hours

    [Header("Rewards")]
    [SerializeField] private int goldRewardBase;
    [SerializeField] private int goldRewardBonus;
    [SerializeField] private int experienceReward;
    [SerializeField] private int experienceRewardBonus;
    [SerializeField] private List<string> itemRewards = new();


    [Header("Location")]
    [SerializeField] private string location = "";
    [SerializeField] private bool isInCity = true;
    [SerializeField] private int travelDistance = 0; // Distance from city in kilometers


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
}

