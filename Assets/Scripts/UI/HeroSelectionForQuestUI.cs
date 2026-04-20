using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionForQuestUI : MonoBehaviour
{
    [SerializeField] public GameObject ButtonContainer;
    [SerializeField] public GameObject SelectedButtonContainer;
    [SerializeField] public GameObject heroButtonPrefabe;
    [SerializeField] public List<Button> itemButtons = new List<Button>();
    [SerializeField] private GameObject TextPanel;
    [SerializeField] Button StartQuestButton;
    private PannelManager _pannelManager;
    public static event System.Action<int> OnRequiredGoldChanged;
    private int _heroTotalGold=0;
    // Item1 = hero index, Item2 = true means "currently selected/locked for quest"
    private List<(int, bool)> selectedHeroes = new List<(int, bool)>();
    private int maxHeroNumber;
    private int count = 0,gold;
    private Dictionary<int, int> heroGoldCosts = new Dictionary<int, int>();
    // Tracks the instantiated child copies in SelectedButtonContainer so we can restore them
    private List<(GameObject copy, int heroIndex)> activeCopies = new List<(GameObject, int)>();

    public int HeroTotalGold
    {
        get => _heroTotalGold;
        set => _heroTotalGold = value;
    }
    public List<(int, bool)> SelectedHeroes
    {
        get => selectedHeroes;
        set => selectedHeroes = value;
    }

    private void Start()
    {
        // Hide the template button (index 0 is the prefab placeholder in the inspector list)
        // if (itemButtons.Count > 0)
        //     itemButtons[0].gameObject.SetActive(false);

        count = 0;
    }

    public void SaveSelectedHeroes()
    {
        GameManager.Instance.saveManager.SaveGame();
    }

    /// <summary>
    /// Called by SaveManager after buttons exist (in Start, not Awake).
    /// Restores which heroes were selected before the game was closed.
    /// </summary>
    public void LoadSelectedHeroes(List<int> saved)
    {
        if (saved == null || saved.Count == 0) return;

        foreach (int ind in saved)
        {
            if (ind >= itemButtons.Count) continue;

            // Mark as selected
            selectedHeroes.Add((ind, true));
            count++;

            // Lock the source button
            itemButtons[ind].interactable = false;
            _heroTotalGold += heroGoldCosts.ContainsKey(ind) ? heroGoldCosts[ind] : 0;
            // Rebuild the visual copy in the selected area
            CreateChildCopy(itemButtons[ind].gameObject, ind);
        }

        if (count > 0)
            StartQuestButton.interactable = true;
    }

    public void OnEnable()
    {
        StartQuestButton.onClick.RemoveAllListeners();
        StartQuestButton.onClick.AddListener(() => goQuest());

        // Rebuild count from current selectedHeroes so max-hero limit stays correct
        count = 0;
        foreach ((int heroIndex, bool isActive) in selectedHeroes)
        {
            if (isActive) count++;
        }

        StartQuestButton.interactable = count > 0;

        // Re-enable buttons for heroes that are NOT currently selected
        foreach ((int heroIndex, bool isActive) in selectedHeroes)
        {
            if (!isActive && heroIndex < itemButtons.Count)
                itemButtons[heroIndex].interactable = true;
        }
    }

    public void setMaxHeroNumber(int val)
    {
        maxHeroNumber = val;
    }

    public void SelectForQuest(int ind)
    {
        if (count < maxHeroNumber)
        {
            count++;
            selectedHeroes.Add((ind, true));
            itemButtons[ind].interactable = false;
            gold = heroGoldCosts.ContainsKey(ind) ? heroGoldCosts[ind] : 0;
            _heroTotalGold += gold;                          // ✅ add first
            OnRequiredGoldChanged?.Invoke(_heroTotalGold);   // ✅ then fire
            CreateChildCopy(itemButtons[ind].gameObject, ind);
            StartQuestButton.interactable = true;
            SaveSelectedHeroes();
        }
        else
        {
            GameManager.Instance.UIManager.popUpPannel.SetActive(true);
            GameManager.Instance.popUpManager.ShowMaxPlayerCount();
        }
    }
    public void CreateChildCopy(GameObject item, int ind)
    {
        GameObject newCopy = Instantiate(item, SelectedButtonContainer.transform);
        newCopy.GetComponent<Button>().interactable = true;

        // Capture local reference for the closure
        GameObject capturedCopy = newCopy;
        newCopy.GetComponent<Button>().onClick.RemoveAllListeners();
        newCopy.GetComponent<Button>().onClick.AddListener(() => DeselectForQuest(ind, capturedCopy));

        activeCopies.Add((newCopy, ind));
    }

    private void DeselectForQuest(int ind, GameObject copy)
    {
        int removeIndex = selectedHeroes.FindIndex(h => h.Item1 == ind && h.Item2 == true);
        if (removeIndex >= 0)
            selectedHeroes.RemoveAt(removeIndex);

        activeCopies.RemoveAll(c => c.copy == copy);
        count = Mathf.Max(0, count - 1);

        if (count <= 0)
            StartQuestButton.interactable = false;

        if (ind >= 0 && ind < itemButtons.Count)
            itemButtons[ind].interactable = true;

        gold = heroGoldCosts.ContainsKey(ind) ? heroGoldCosts[ind] : 0;
        _heroTotalGold -= gold;                          // ✅ subtract first
        _heroTotalGold = Mathf.Max(0, _heroTotalGold);  // ✅ then clamp
        OnRequiredGoldChanged?.Invoke(_heroTotalGold);   // ✅ then fire
        Destroy(copy);
        SaveSelectedHeroes();
    }
    /// <summary>
    /// Called when a quest finishes. Unlocks all previously selected heroes
    /// and clears the selection so a new quest can be started.
    /// </summary>
    public void OnQuestComplete()
    {
        // Mark all heroes as no longer locked
        for (int i = 0; i < selectedHeroes.Count; i++)
        {
            var h = selectedHeroes[i];
            selectedHeroes[i] = (h.Item1, false);

            if (h.Item1 < itemButtons.Count)
                itemButtons[h.Item1].interactable = true;
        }

        // Destroy all child copies
        foreach (var (copyObj, _) in activeCopies)
        {
            if (copyObj != null) Destroy(copyObj);
        }
        activeCopies.Clear();
        selectedHeroes.Clear();
        _heroTotalGold = 0;
        count = 0;
        StartQuestButton.interactable = false;
        SaveSelectedHeroes();
    }

    public void heroIconUpdate()
    {
        for (int i = 0; i < itemButtons.Count; i++)
        {
            itemButtons[i].GetComponent<Image>().sprite =
                GameManager.Instance.HeroSummoner.getCurrentHeroSprite(i);

            itemButtons[i].gameObject.SetActive(
                GameManager.Instance.HeroSummoner.isHeroSummoned(i));
        }
    }

// HeroSelectionForQuestUI.cs — fix AddButton()
    public void AddButton(HeroData data)
    {
        int buttonIndex = itemButtons.Count; // ✅ capture index BEFORE adding to list

        GameObject child = Instantiate(heroButtonPrefabe, ButtonContainer.transform);
        child.GetComponent<Image>().sprite = data.heroSprite[0];
        child.GetComponent<Button>().onClick.AddListener(() => SelectForQuest(buttonIndex)); // ✅ use index, not uniqueId
        itemButtons.Add(child.GetComponent<Button>());
        heroGoldCosts[buttonIndex] = data.goldCost; // ✅ key by index too
    }

    public void loadGame()
    {
        List<HeroData> heroDatas = GameManager.Instance.saveManager.heroDatas;
        for (int i = 0; i < heroDatas.Count; i++)
        {
            AddButton(heroDatas[i]);
        }
    }

    private void goQuest()
    {
        SaveManager.Instance.SaveGame();
        // SaveManager.Instance.loadGame();
        TextPanel.SetActive(true);
        GameManager.Instance.pannelManager.GoQuest(count, selectedHeroes);
    }

    public void ClearChildren()
    {
        foreach (Transform child in SelectedButtonContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        count = 0;
       // selectedHeroes.Clear();
        _heroTotalGold = 0;                              // ✅ add this
        OnRequiredGoldChanged?.Invoke(_heroTotalGold);   // ✅ update UI
    }

    public void RestoreButtons(List<int> selectedHeroIndices)
    {
        foreach (int index in selectedHeroIndices)
        {
            if (index >= 0 && index < itemButtons.Count)
            {
                itemButtons[index].interactable = true;
            }
        }
    }


    public void DeselectSelectedPlayer()
    {
        foreach(int btn in selectedHeroes.Where(h => h.Item2).Select(h => h.Item1))
        {
            itemButtons[btn].interactable = true;
        }
    }

    public GameObject notificationPannel;
    public void OpenNotification()
    {
        notificationPannel.SetActive(true);
    }
}