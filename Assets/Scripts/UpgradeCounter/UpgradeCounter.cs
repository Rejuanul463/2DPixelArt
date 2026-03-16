using System;
using UnityEngine;
using TMPro;

public class UpgradeCounter : MonoBehaviour
{
    [SerializeField] private float upgradeTimer;

    private const string START_TIMER = "StartTimer";
    private const string IS_UPGRADING = "IsUpgrading";

    [SerializeField] TextMeshProUGUI timerText;

    // Start is called once before the first execution of Update
    void Start()
    {
        CheckUpgradeProgress();
    }

    // Update is called once per frame
    void Update()
    {
        CheckUpgradeProgress();
    }

    public void StartUpgradeTimer()
    {
        DateTime startTime = DateTime.UtcNow;

        PlayerPrefs.SetString(START_TIMER, startTime.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.SetInt(IS_UPGRADING, 1);
        PlayerPrefs.Save();

        // upgrade time to show in the text
        timerText.text = startTime.ToString();

        Debug.Log("Upgrade Start at: " + startTime);
    }

    private void CheckUpgradeProgress()
    {
        if (PlayerPrefs.GetInt(IS_UPGRADING, 0) == 0)
            return;

        string savedTime = PlayerPrefs.GetString(START_TIMER);
        DateTime startTime = DateTime.Parse(savedTime);

        TimeSpan elapsedTime = DateTime.UtcNow - startTime;

        if (elapsedTime.TotalSeconds > upgradeTimer)
        {
            FinishUpgrade();
        }
        else
        {
            float remaining = upgradeTimer - (float)elapsedTime.TotalSeconds;

            TimeSpan time = TimeSpan.FromSeconds(remaining);

            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                time.Hours,
                time.Minutes,
                time.Seconds);

            timerText.text = "Upgrade Timer Remaining: " + formattedTime;

            Debug.Log("Remain Timer: " + formattedTime);
        }
    }

    void FinishUpgrade()
    {
        timerText.text = "Upgrade Complete";
        Debug.Log("Finish Upgrade");

        PlayerPrefs.SetInt(IS_UPGRADING, 0);
        PlayerPrefs.DeleteKey(START_TIMER);
    }
}