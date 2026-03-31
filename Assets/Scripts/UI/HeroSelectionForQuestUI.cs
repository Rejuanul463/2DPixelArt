using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionForQuestUI : MonoBehaviour
{
    [SerializeField] public GameObject ButtonContainer;
    [SerializeField] public GameObject SelectedButtonContainer;
    [SerializeField] public GameObject heroButtonPrefabe;
    [SerializeField] private GameObject TextPanel;
    [SerializeField] private Button StartQuestButton;

    // Maps hero uniqueId -> its source Button in ButtonContainer
    private Dictionary<int, Button> heroButtonMap = new Dictionary<int, Button>();

    // Item1 = hero uniqueId, Item2 = true means currently selected for quest
    private List<(int uid, bool active)> selectedHeroes = new List<(int, bool)>();

    // Keep itemButtons list for any external references
    public List<Button> itemButtons = new List<Button>();

    private int maxHeroNumber = 0;

    // count = heroes selected for the CURRENT quest only (not permanently locked ones)
    private int count = 0;

    private List<(GameObject copy, int uid)> activeCopies = new List<(GameObject, int)>();

    // Heroes that are permanently locked — sent on a quest, cannot be reused
    private HashSet<int> permanentlyLockedHeroes = new HashSet<int>();

    // Heroes selected in the current panel session (not yet started quest)
    private HashSet<int> pendingSelection = new HashSet<int>();

    public bool QuestInProgress => permanentlyLockedHeroes.Count > 0;

    public List<(int, bool)> SelectedHeroes
    {
        get => selectedHeroes;
        set => selectedHeroes = value;
    }

    private void Start()
    {
        count = 0;
    }

    // -------------------------------------------------------
    // SAVE / LOAD
    // -------------------------------------------------------

    public void SaveSelectedHeroes()
    {
        GameManager.Instance.saveManager.SaveGame();
    }

    public void LoadSelectedHeroes(List<int> saved, bool wasQuestInProgress)
    {
        if (saved == null || saved.Count == 0) return;

        foreach (int uid in saved)
        {
            if (!heroButtonMap.ContainsKey(uid)) continue;

            selectedHeroes.Add((uid, true));
            heroButtonMap[uid].interactable = false;
            CreateChildCopy(heroButtonMap[uid].gameObject, uid);

            if (wasQuestInProgress)
            {
                permanentlyLockedHeroes.Add(uid);
            }
            else
            {
                pendingSelection.Add(uid);
                count++;
            }
        }

        if (wasQuestInProgress)
        {
            // Lock all restored copies
            foreach (var (copy, _) in activeCopies)
            {
                if (copy == null) continue;
                var btn = copy.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.interactable = false;
            }
            StartQuestButton.interactable = false;
        }
        else if (count > 0)
        {
            StartQuestButton.interactable = true;
        }
    }

    // -------------------------------------------------------
    // UNITY CALLBACKS
    // -------------------------------------------------------

    public void OnEnable()
    {
        StartQuestButton.onClick.RemoveAllListeners();
        StartQuestButton.onClick.AddListener(() => goQuest());

        // Reset count to only count the pending (non-locked) selections
        count = 0;
        pendingSelection.Clear();
        foreach ((int uid, bool isActive) in selectedHeroes)
        {
            if (isActive && !permanentlyLockedHeroes.Contains(uid))
            {
                count++;
                pendingSelection.Add(uid);
            }
        }

        StartQuestButton.interactable = count > 0;
    }

    // -------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------

    public void setMaxHeroNumber(int val)
    {
        maxHeroNumber = val;
        Debug.Log("[HeroSelection] maxHeroNumber set to: " + val);
    }

    public void AddButton(HeroData data)
    {
        GameObject child = Instantiate(heroButtonPrefabe, ButtonContainer.transform);
        child.GetComponent<Image>().sprite = data.heroSprite[0];

        int capturedUid = data.uniqueId;
        Button btn = child.GetComponent<Button>();
        btn.onClick.AddListener(() => SelectForQuest(capturedUid));

        heroButtonMap[data.uniqueId] = btn;
        itemButtons.Add(btn);
    }

    public void loadGame()
    {
        List<HeroData> heroDatas = GameManager.Instance.saveManager.heroDatas;
        for (int i = 0; i < heroDatas.Count; i++)
            AddButton(heroDatas[i]);
    }

    public void heroIconUpdate()
    {
        foreach (var kvp in heroButtonMap)
        {
            HeroData hd = GameManager.Instance.HeroSummoner.heroDatas
                .Find(h => h.uniqueId == kvp.Key);
            if (hd == null) continue;
            kvp.Value.GetComponent<Image>().sprite =
                GameManager.Instance.HeroSummoner.getCurrentHeroSprite(hd.uniqueId);
            kvp.Value.gameObject.SetActive(
                GameManager.Instance.HeroSummoner.isHeroSummoned(hd.uniqueId));
        }
    }

    /// <summary>
    /// Called when player cancels quest (closes panel without starting).
    /// Removes pending child copies, re-enables buttons, restores heroes to scene.
    /// Does NOT touch permanently locked heroes.
    /// </summary>
    public void ClearChildren()
    {
        var toDestroy = new List<(GameObject copy, int uid)>();
        foreach (var (copy, uid) in activeCopies)
        {
            if (!permanentlyLockedHeroes.Contains(uid))
                toDestroy.Add((copy, uid));
        }

        foreach (var (copy, uid) in toDestroy)
        {
            // Re-enable source button
            if (heroButtonMap.ContainsKey(uid))
                heroButtonMap[uid].interactable = true;

            // Restore hero to scene (was deactivated on select)
            ReactivateHeroInScene(uid);

            // Remove from tracking
            activeCopies.RemoveAll(c => c.copy == copy);
            int removeIdx = selectedHeroes.FindIndex(h => h.uid == uid && h.active);
            if (removeIdx >= 0) selectedHeroes.RemoveAt(removeIdx);

            GameObject.Destroy(copy);
        }

        pendingSelection.Clear();
        count = 0;
        StartQuestButton.interactable = false;
    }

    public void RestoreButtons(List<int> uids)
    {
        foreach (int uid in uids)
            if (heroButtonMap.ContainsKey(uid) && !permanentlyLockedHeroes.Contains(uid))
                heroButtonMap[uid].interactable = true;
    }

    // -------------------------------------------------------
    // SELECTION
    // -------------------------------------------------------

    public void SelectForQuest(int uniqueId)
    {
        Debug.Log("[HeroSelection] SelectForQuest uid=" + uniqueId
            + " locked=" + permanentlyLockedHeroes.Contains(uniqueId)
            + " maxHeroNumber=" + maxHeroNumber
            + " count=" + count);

        if (permanentlyLockedHeroes.Contains(uniqueId))
        {
            Debug.Log("[HeroSelection] Blocked: hero permanently locked");
            return;
        }

        if (maxHeroNumber <= 0)
        {
            Debug.Log("[HeroSelection] Blocked: select a quest first");
            return;
        }

        if (!heroButtonMap.ContainsKey(uniqueId))
        {
            Debug.Log("[HeroSelection] Blocked: uniqueId not in map");
            return;
        }

        if (count < maxHeroNumber)
        {
            count++;
            pendingSelection.Add(uniqueId);
            selectedHeroes.Add((uniqueId, true));
            heroButtonMap[uniqueId].interactable = false;
            CreateChildCopy(heroButtonMap[uniqueId].gameObject, uniqueId);
            StartQuestButton.interactable = true;
            DeactivateHeroInScene(uniqueId);
            SaveSelectedHeroes();
        }
        else
        {
            GameManager.Instance.UIManager.popUpPannel.SetActive(true);
            GameManager.Instance.popUpManager.ShowMaxPlayerCount();
        }
    }

    public void CreateChildCopy(GameObject item, int uniqueId)
    {
        GameObject newCopy = Instantiate(item, SelectedButtonContainer.transform);
        Button btn = newCopy.GetComponent<Button>();
        btn.interactable = true;

        GameObject capturedCopy = newCopy;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => DeselectForQuest(uniqueId, capturedCopy));

        activeCopies.Add((newCopy, uniqueId));
    }

    private void DeselectForQuest(int uniqueId, GameObject copy)
    {
        // Cannot deselect if permanently locked
        if (permanentlyLockedHeroes.Contains(uniqueId)) return;

        int removeIndex = selectedHeroes.FindIndex(h => h.uid == uniqueId && h.active);
        if (removeIndex >= 0)
            selectedHeroes.RemoveAt(removeIndex);

        activeCopies.RemoveAll(c => c.copy == copy);
        pendingSelection.Remove(uniqueId);

        count = Mathf.Max(0, count - 1);
        if (count <= 0)
            StartQuestButton.interactable = false;

        if (heroButtonMap.ContainsKey(uniqueId))
            heroButtonMap[uniqueId].interactable = true;

        ReactivateHeroInScene(uniqueId);
        Destroy(copy);
        SaveSelectedHeroes();
    }

    // -------------------------------------------------------
    // QUEST LIFECYCLE
    // -------------------------------------------------------

    private void goQuest()
    {
        SaveManager.Instance.SaveGame();
        TextPanel.SetActive(true);

        // Move pending selection into permanently locked
        foreach (int uid in pendingSelection)
            permanentlyLockedHeroes.Add(uid);

        // Lock their copies
        foreach (var (copy, uid) in activeCopies)
        {
            if (!permanentlyLockedHeroes.Contains(uid)) continue;
            if (copy == null) continue;
            var btn = copy.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.interactable = false;
        }

        int questCount = pendingSelection.Count;
        pendingSelection.Clear();
        count = 0;
        StartQuestButton.interactable = false;

        GameManager.Instance.pannelManager.GoQuest(questCount, selectedHeroes);
    }

    /// <summary>
    /// Called when quest finishes. Destroys child copies and scene heroes
    /// for the heroes that were on this quest.
    /// </summary>
    public void OnQuestComplete(List<int> questHeroUids)
    {
        foreach (int uid in questHeroUids)
        {
            // Remove from permanently locked
            permanentlyLockedHeroes.Remove(uid);

            // Remove from selectedHeroes list
            selectedHeroes.RemoveAll(h => h.uid == uid);

            // Destroy child copy
            var toRemove = activeCopies.FindAll(c => c.uid == uid);
            foreach (var (copy, _) in toRemove)
            {
                if (copy != null) Destroy(copy);
            }
            activeCopies.RemoveAll(c => c.uid == uid);

            // Destroy the hero GameObject from scene permanently
            DestroyHeroInScene(uid);

            // Remove button from map (hero is gone permanently)
            if (heroButtonMap.ContainsKey(uid))
            {
                var btn = heroButtonMap[uid];
                if (btn != null) Destroy(btn.gameObject);
                heroButtonMap.Remove(uid);
            }
        }

        SaveSelectedHeroes();
    }

    // -------------------------------------------------------
    // SCENE HERO VISIBILITY
    // -------------------------------------------------------

    private void DeactivateHeroInScene(int heroUniqueId)
    {
        foreach (Hero hero in GameObject.FindObjectsOfType<Hero>())
        {
            if (hero.heroData.uniqueId == heroUniqueId)
            {
                hero.gameObject.SetActive(false);
                return;
            }
        }
    }

    private void ReactivateHeroInScene(int heroUniqueId)
    {
        foreach (Hero hero in GameObject.FindObjectsOfType<Hero>(true))
        {
            if (hero.heroData.uniqueId == heroUniqueId)
            {
                hero.gameObject.SetActive(true);
                return;
            }
        }
    }

    private void DestroyHeroInScene(int heroUniqueId)
    {
        foreach (Hero hero in GameObject.FindObjectsOfType<Hero>(true))
        {
            if (hero.heroData.uniqueId == heroUniqueId)
            {
                Destroy(hero.gameObject);
                return;
            }
        }
    }
}