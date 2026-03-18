using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class QuestItemUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    [SerializeField]private int identity;
    private void Start()
    {
       
    }

    public int Identity
    {
        get => identity;
        set => identity = value;
    }
    public void UpdateUI(int id,string questName, TimeSpan remaining)
    {
        identity= id;
        string timeText = remaining.TotalSeconds <= 0
            ? "Finished"
            : $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";

        text.text =
            $"<b>Quest:</b> {questName}\n" +
            $"<b>Time:</b> {timeText}";
    }
}