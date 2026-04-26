using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeroUI : MonoBehaviour
{
    [SerializeField] public GameObject ButtonContainer;
    [SerializeField] public GameObject heroButtonPrefabe;
    [SerializeField] public List<Button> itemButtons = new List<Button>();
    [SerializeField] public Image itemImage;
    [SerializeField] public TextMeshProUGUI name;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI hp;
    [SerializeField] TextMeshProUGUI damage;
    [SerializeField] TextMeshProUGUI dps;
    [SerializeField] Button UpgradeButton;

    // Tracks which hero indices are currently on a quest
    private HashSet<int> heroesOnQuest = new HashSet<int>();

    private void OnEnable()
    {
        heroIconUpdate();
    }

    public void loadGame()
    {
        List<HeroData> heroDatas = GameManager.Instance.saveManager.heroDatas;
        for (int i = 0; i < heroDatas.Count; i++)
        {
            AddButton(heroDatas[i]);
        }
    }

    public void ShowDetails(int ind)
    {
        UpgradeButton.onClick.RemoveAllListeners();
        UpgradeButton.onClick.AddListener(() => UpgradeHero(ind));

        itemImage.enabled = true;
        itemImage.sprite = itemButtons[ind].GetComponent<Image>().sprite;

        name.text = GameManager.Instance.HeroSummoner.getHeroName(ind);
        level.text = "Level: " + GameManager.Instance.HeroSummoner.getHeroLevel(ind).ToString();
        hp.text = "HP: " + GameManager.Instance.HeroSummoner.getHeroHP(ind).ToString();
        damage.text = "Damage: " + GameManager.Instance.HeroSummoner.getHeroPower(ind).ToString();
        dps.text = "DPS: " + GameManager.Instance.HeroSummoner.getHeroHitPerSecound(ind).ToString();
    }

    /// <summary>
    /// Called by goQuest() to mark which heroes are now on a quest.
    /// </summary>
    public void SetHeroesOnQuest(List<int> heroIndices)
    {
        heroesOnQuest.Clear();
        foreach (int i in heroIndices)
            heroesOnQuest.Add(i);
        heroIconUpdate();
    }

    /// <summary>
    /// Called by OnQuestComplete() to clear all quest overlays.
    /// </summary>
    public void ClearQuestOverlays()
    {
        heroesOnQuest.Clear();
        heroIconUpdate();
    }

    public void heroIconUpdate()
    {
        for (int i = 0; i < itemButtons.Count; i++)
        {
            itemButtons[i].GetComponent<Image>().sprite =
                GameManager.Instance.HeroSummoner.getCurrentHeroSprite(i);

            bool summoned = GameManager.Instance.HeroSummoner.isHeroSummoned(i);
            itemButtons[i].gameObject.SetActive(summoned);

            // Show or hide the On Quest overlay
            Transform overlay = itemButtons[i].transform.Find("QuestOverlay");
            if (overlay != null)
                overlay.gameObject.SetActive(summoned && heroesOnQuest.Contains(i));
        }
    }

    public void AddButton(HeroData data)
    {
        GameObject child = Instantiate(heroButtonPrefabe, ButtonContainer.transform);
        child.GetComponent<Image>().sprite = data.heroSprite[data.level - 1];
        child.GetComponent<Button>().onClick.AddListener(() => ShowDetails(data.uniqueId));
        itemButtons.Add(child.GetComponent<Button>());

        // Build the "On Quest" overlay in code — no prefab changes needed
        GameObject overlay = new GameObject("QuestOverlay", typeof(RectTransform));
        overlay.transform.SetParent(child.transform, false);

        RectTransform rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Dark tint
        Image tint = overlay.AddComponent<Image>();
        tint.color = new Color(0f, 0f, 0f, 0.6f);
        tint.raycastTarget = false;

        // "On Quest" label
        GameObject labelObj = new GameObject("QuestLabel", typeof(RectTransform));
        labelObj.transform.SetParent(overlay.transform, false);

        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0.25f);
        labelRt.anchorMax = new Vector2(1f, 0.75f);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "On Quest";
        label.fontSize = 14;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        overlay.SetActive(false);
    }

    void UpgradeHero(int id)
    {
        if (GameManager.Instance.HeroSummoner.UpgradeHero(id))
        {
            itemButtons[id].GetComponent<Image>().sprite =
                GameManager.Instance.HeroSummoner.getCurrentHeroSprite(id);
            ShowDetails(id);
        }
    }
}