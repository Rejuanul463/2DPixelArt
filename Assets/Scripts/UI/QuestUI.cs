using System.Collections;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public float DelayedAttribute = 3600f;
    private bool isUpdated = false;
    [SerializeField] private GameObject questUIHolder;
    [SerializeField] private TextMeshProUGUI questUIText;
    [SerializeField] private GameObject questTextMeshPro;
    [SerializeField] private GameObject QuestStartPannel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isUpdated)
        {
            isUpdated = true;
            StartCoroutine(UpdataeQuest());
        }
    }

    IEnumerator UpdataeQuest()
    {
        //update quest UI elements here
        PrintAllQuests();
        yield return new WaitForSeconds(DelayedAttribute);
        isUpdated = false;
    }


    public void showQuest(string questText)
    {
        questTextMeshPro.GetComponent<TextMeshProUGUI>().text = "Quest Details:\n" + questText;
    }

    public void PrintAllQuests()
    {
        foreach (var q in QuestManager.Instance.mainQuests)
        {
            GameObject newChild = Instantiate(questUIHolder, transform);
           
            //Debug.Log($"Name: {q.questName}, Type: {q.questType}, Difficulty: {q.questDifficulty}, Enemies: {q.enemyCount}, Total HP: {q.TotalHP}, Total DPS: {q.TotalDPS}, Gold Required: {q.requiredGoldToAttack}");
            TextMeshProUGUI[] allChildTexts = newChild.GetComponentsInChildren<TextMeshProUGUI>();
            allChildTexts[0].text = $"{q.questName}, ({ q.questType})";
            allChildTexts[1].text = $" {q.questDifficulty}";
            if (q.questDifficulty == QuestData.QuestDifficulty.Easy)
            {
                allChildTexts[1].color = Color.gray;
                newChild.GetComponent<QuestButton>().QuestDetails = $"Name: {q.questName}, \nQuest Type: ({q.questType}),\nDifficulty: <color=\"grey\">{q.questDifficulty}</color>, \nEnemies: {q.enemyCount}, \nTotal HP: {q.TotalHP}, \nTotal DPS: {q.TotalDPS}, \nGold Reward: {q.goldReward}, \n Required level: {q.requiredLevel}";
            }
            else if (q.questDifficulty == QuestData.QuestDifficulty.Medium)
            {
                allChildTexts[1].color = Color.blue;
                newChild.GetComponent<QuestButton>().QuestDetails = $"Name: {q.questName}, \nQuest Type: ({q.questType}),\nDifficulty: <color=\"blue\">{q.questDifficulty}</color>, \nEnemies: {q.enemyCount}, \nTotal HP: {q.TotalHP}, \nTotal DPS: {q.TotalDPS}, \nGold Reward: {q.goldReward}, \n Required level: {q.requiredLevel}";
            }
            else if (q.questDifficulty == QuestData.QuestDifficulty.Hard)
            {
                allChildTexts[1].color = Color.magenta; 
                newChild.GetComponent<QuestButton>().QuestDetails = $"Name: {q.questName}, \nQuest Type: ({q.questType}),\nDifficulty: <color=\"purple\">{q.questDifficulty}</color>, \nEnemies: {q.enemyCount}, \nTotal HP: {q.TotalHP}, \nTotal DPS: {q.TotalDPS}, \nGold Reward: {q.goldReward}, \n Required level: {q.requiredLevel}";
            }
            else if (q.questDifficulty == QuestData.QuestDifficulty.SuperHard)
            {
                allChildTexts[1].color = Color.cyan;
                newChild.GetComponent<QuestButton>().QuestDetails = $"Name: {q.questName}, \nQuest Type: ({q.questType}),\nDifficulty: <color=\"cyan\">{q.questDifficulty}</color>, \nEnemies: {q.enemyCount}, \nTotal HP: {q.TotalHP}, \nTotal DPS: {q.TotalDPS}, \nGold Reward: {q.goldReward}, \n Required level: {q.requiredLevel}";
            }
            else if (q.questDifficulty == QuestData.QuestDifficulty.Epic)
            {
                allChildTexts[1].color = Color.red;
                newChild.GetComponent<QuestButton>().QuestDetails = $"Name: {q.questName}, \nQuest Type: ({q.questType}),\nDifficulty: <color=\"red\">{q.questDifficulty}</color>, \nEnemies: {q.enemyCount}, \nTotal HP: {q.TotalHP}, \nTotal DPS: {q.TotalDPS}, \nGold Reward: {q.goldReward}, \n Required level: {q.requiredLevel}";
            }

        }
    }


    public void ShowQuest(string text)
    {
        questUIText.text += text;
    }

    public void GoToQuest()
    {
        QuestStartPannel.SetActive(true);

        UIManager.Instance.pannelManager.activePannelObj.SetActive(false);
    }


}
