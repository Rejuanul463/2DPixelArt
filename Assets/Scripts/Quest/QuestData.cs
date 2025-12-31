using UnityEngine;

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

    [Header("Requirements")]
    public int requiredLevel;
    public int recommendedLevel;
    public int requiredGoldToAttack;

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;

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
