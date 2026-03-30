using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeCounter : MonoBehaviour
{
    private DateTime endTime;
    [SerializeField] TextMeshProUGUI questUpdate;
    private const string QUEST_END = "QuestEndTime";
    public Action OnQuestFinished;
    //[SerializeField] private GameObject TextPanel;
    void Start()
    {
        //TextPanel.SetActive(true);
        RestoreQuestTimer();
    }

    public TextMeshProUGUI QuestUpdate
    {
        get => questUpdate;
        set => questUpdate = value;
    }
    // Start quest
    public void StartQuest(float duration)
    {
        endTime = DateTime.UtcNow.AddSeconds(duration);

        PlayerPrefs.SetString(QUEST_END, endTime.ToString("o"));
        PlayerPrefs.Save();

        Debug.Log("Quest Started. Ends at: " + endTime);
        questUpdate.text = "Quest Started";
        StartCoroutine(QuestCoroutine());
    }

    // Coroutine countdown
  public  IEnumerator QuestCoroutine()
    {
        while (true)
        {
            TimeSpan remaining = endTime - DateTime.UtcNow;

            if (remaining.TotalSeconds <= 0)
                break;

            Debug.Log("Remaining Time: " + Mathf.Ceil((float)remaining.TotalSeconds));
            questUpdate.text = "Quest Will be Updated in "+ (int)remaining.TotalSeconds ;
            yield return new WaitForSeconds(1f);
        }

        QuestFinished();
    }

    // Restore quest when game restarts
    void RestoreQuestTimer()
    {
        if (!PlayerPrefs.HasKey(QUEST_END))
            return;
        
        string savedTime = PlayerPrefs.GetString(QUEST_END);
        endTime = DateTime.Parse(savedTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        TimeSpan remaining = endTime -  DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0)
        {
            QuestFinished();
        }
        else
        {
            Debug.Log("Restored Quest. Remaining: " + Mathf.Ceil((float)remaining.TotalSeconds));
            StartCoroutine(QuestCoroutine());
        }
    }

    // Quest completion
    void QuestFinished()
    {
        //questUpdate.text = "Quest Finished";
        Debug.Log("Quest Complete");
        OnQuestFinished?.Invoke();
        PlayerPrefs.DeleteKey(QUEST_END);

        // Add reward logic here
    }
}