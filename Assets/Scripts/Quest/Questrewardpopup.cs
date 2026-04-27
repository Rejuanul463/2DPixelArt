using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// NEW SCRIPT — attach to any GameObject in your scene (e.g. "QuestRewardPopupManager").
/// Wire up the panel and text fields in the Inspector.
/// It is triggered automatically by PannelManager when a quest is won.
/// </summary>
public class QuestRewardPopup : MonoBehaviour
{
    public static QuestRewardPopup Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Row GameObjects (auto-hidden when value is 0)")]
    [SerializeField] private GameObject goldRow;
    [SerializeField] private GameObject gemsRow;
    [SerializeField] private GameObject woodRow;
    [SerializeField] private GameObject stoneRow;
    [SerializeField] private GameObject expRow;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration  = 0.3f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    private CanvasGroup canvasGroup;
    private Coroutine   activeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (popupPanel != null)
        {
            canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = popupPanel.AddComponent<CanvasGroup>();
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    // Called by PannelManager after all rewards are applied
    public void Show(QuestData quest)
    {
        if (quest == null) return;

        if (questNameText != null)
            questNameText.text = "\"" + quest.questName + "\" Complete!";

        // Gems is hardcoded 15 — same value used in ResultOfTheQuest
        SetRow(goldRow,  goldText,  quest.goldRewardBase,    "+ " + quest.goldRewardBase    + " Gold");
        SetRow(gemsRow,  gemsText,  15,                      "+ 15 Gems");
        SetRow(woodRow,  woodText,  quest.WoodReward,        "+ " + quest.WoodReward        + " Wood");
        SetRow(stoneRow, stoneText, quest.StoneReward,       "+ " + quest.StoneReward       + " Stone");
        SetRow(expRow,   expText,   quest.experienceReward,  "+ " + quest.experienceReward  + " EXP");

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(ShowRoutine());
    }

    private void SetRow(GameObject row, TextMeshProUGUI label, int value, string displayText)
    {
        bool visible = value > 0;
        if (row   != null) row.SetActive(visible);
        if (label != null) label.text = visible ? displayText : "";
    }

    private IEnumerator ShowRoutine()
    {
        popupPanel.SetActive(true);
        canvasGroup.alpha = 0f;

        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1f - (t / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        popupPanel.SetActive(false);
        activeCoroutine = null;
    }
}