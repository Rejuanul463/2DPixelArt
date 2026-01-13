using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    // Use a struct to hold all quest info
    public struct QuestDetails
    {
        public string name;
        public string description;
        public string enemyName;
        public float enemyHP;
        public float hitPerSecond;
        public float hitPower;
        public float reward;
        public int enemyCount;
        public int maxPlayerCount;

        // Constructor
        public QuestDetails(string name, string desc, string eName, float eHP, float hps, float hit, float gold, int count, int plc)
        {
            this.name = name;
            description = desc;
            enemyName = eName;
            enemyHP = eHP;
            hitPerSecond = hps;
            hitPower = hit;
            reward = gold;
            enemyCount = count;
            maxPlayerCount = plc;
        }
    }

    public QuestDetails SelectedQD;

    [SerializeField] QuestData[] questData;
    [SerializeField] int latestQuestInd;

    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] public TextMeshProUGUI details;

    public bool questSelected = false;

    private void Start()
    {
        questSelected = false;
        loadQuests();
    }

    private void loadQuests()
    {
        for (int i = latestQuestInd; i < questData.Length; i++)
        {
            if (GameManager.Instance.GuildManager.GuildLevel < questData[i].requiredLevel)
                return;

            if (questData[i].isCompleted)
                continue;

            // Instantiate prefab
            GameObject item = Instantiate(itemPrefab, content);
            item.transform.localScale = Vector3.one;

            // Set name
            TextMeshProUGUI nameText = item.transform.Find("name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = questData[i].questName;

            // Set type
            TextMeshProUGUI typeText = item.transform.Find("type")?.GetComponent<TextMeshProUGUI>();
            if (typeText != null)
                typeText.text = questData[i].questType == QuestData.QuestType.Main ? "Main" : "Side";

            // Set difficulty and color
            TextMeshProUGUI diffText = item.transform.Find("Difficulty")?.GetComponent<TextMeshProUGUI>();
            if (diffText != null)
            {
                switch (questData[i].questDifficulty)
                {
                    case QuestData.QuestDifficulty.Easy:
                        diffText.text = "Easy"; diffText.color = Color.green; break;
                    case QuestData.QuestDifficulty.Medium:
                        diffText.text = "Medium"; diffText.color = Color.cyan; break;
                    case QuestData.QuestDifficulty.Hard:
                        diffText.text = "Hard"; diffText.color = Color.yellow; break;
                    case QuestData.QuestDifficulty.SuperHard:
                        diffText.text = "Super Hard"; diffText.color = Color.red; break;
                    default:
                        diffText.text = "Epic"; diffText.color = Color.magenta; break;
                }
            }

            // Prepare quest details struct
            QuestDetails qDetails = new QuestDetails(
                questData[i].questName,
                questData[i].description,
                questData[i].enemyName,
                questData[i].HP,
                questData[i].hitPerSecond,
                questData[i].hitPower,
                questData[i].goldRewardBase,
                questData[i].enemyCount,
                questData[i].maxTeamSize
            );

            // Assign onClick using lambda and pass the struct
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnQuestButtonPressed(qDetails));
            }
        }
    }

    // Now the button sends the struct instead of multiple arguments
    private void OnQuestButtonPressed(QuestDetails qD)
    {
        SelectedQD = qD;
        questSelected = true;
        details.text = "Quest Details" + qD.name + " : " + qD.description + "\n"
                        + "Enemy : " + qD.enemyName + "\n"
                        + "HP : " + qD.enemyHP + ",  Hit/s : " + qD.hitPerSecond + "\n"
                        + "HitDmg : " + qD.hitPower + ",  Gold : " + qD.reward;


    }



    public bool SimulateCombat(float heroHP, float heroHPS, float heroHitPower)
    {
        questSelected = false;
        float enemyHP = SelectedQD.enemyHP;
        float enemyHPS = SelectedQD.hitPerSecond;
        float enemyHitPower = SelectedQD.hitPower;
        int enemyCount = SelectedQD.enemyCount;

        // Total damage per second for each side
        float totalHeroDPS = heroHPS * heroHitPower;
        float totalEnemyDPS = enemyHPS * enemyHitPower * enemyCount;

        // Total health for each side
        float totalHeroHP = heroHP;
        float totalEnemyHP = enemyHP * enemyCount;

        // Time to kill the other side
        float timeForHeroesToKillEnemies = totalEnemyHP / totalHeroDPS;
        float timeForEnemiesToKillHeroes = totalHeroHP / totalEnemyDPS;

        // Debug log (optional)
        Debug.Log($"Heroes DPS: {totalHeroDPS}, Total Enemy HP: {totalEnemyHP}, Time to kill: {timeForHeroesToKillEnemies}");
        Debug.Log($"Enemies DPS: {totalEnemyDPS}, Total Hero HP: {totalHeroHP}, Time to kill: {timeForEnemiesToKillHeroes}");

        // Winner is the side that kills the other first
        if( timeForHeroesToKillEnemies <= timeForEnemiesToKillHeroes)
        {
            GameManager.Instance.GuildManager.Gold += (int)SelectedQD.reward;
            //GameManager.Instance.GuildManager.
            return true;
        }
        else
        {
            return false;
        }
    }
}

