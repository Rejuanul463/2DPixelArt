using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PannelManager : MonoBehaviour
{
    private Button inventoryButton;
    private Button questButton;
    private Button heroButton;
    private Button buildingButton;
    private Button summonButton;
    private Button blackSmithButton;
    private Button pauseButton;
    private Button heroSelectionButton;
    private QuestData _questData;
    private Button GoToQuestButton;
    private Button summonHeroButton;
    private bool publishResult = false;

    [SerializeField] private List<GameObject> pannels;
    [SerializeField] private Transform spawner;
    [SerializeField] private List<Button> heroesSummonButtons;
    [SerializeField] private List<Button> heroSummonDelet;
    [SerializeField] private List<Button> heroesQuestButtons;
    [SerializeField] private List<Button> heroQuestDeletButtons;
    [SerializeField] private GuildData _guildData;
    public GameObject activePannelObj;
    public int[] typeAvailable;

    [SerializeField] private OngoingQuest ongoingQuest;
    [SerializeField] private int requiredGold;
    private QuestData simulationQuestData;
    [SerializeField] private UpgradeCounter _upgradeCounter;
    [SerializeField] GameObject gotoQuestPrefab;
    public static event System.Action<int> OnQuestStarting;
    private bool theResultIsOut;
    [SerializeField] private HeroSelectionForQuestUI _heroSelectionForQuestUI;

    public bool TheResultIsOut
    {
        get => theResultIsOut;
        set => theResultIsOut = value;
    }

    public bool PublishResult
    {
        get => publishResult;
        set => publishResult = value;
    }

    public List<int> SelectedHeroesForQuest
    {
        get => selectedHeroesForQuest;
        set => selectedHeroesForQuest = value;
    }

    public QuestData SimulationQuestData
    {
        get => simulationQuestData;
        set => simulationQuestData = value;
    }

    void Start()
    {
        AddGameObjects();
        deactiveAllPannels();
        addListener();
        typeAvailable = new int[6];
        GameManager.Instance.upgradeCounter.OnQuestFinished += QuestFinishedHandler;
        _heroSelectionForQuestUI = GameManager.Instance.heroSelectionForQuestUI;

        // FIX: Call RestoreHeroSelectionState() here, AFTER AddGameObjects() has populated
        // heroesQuestButtons. SaveManager.Start() populated SelectedHeroesForQuest already
        // (via RestoreHeroSelectionUI), so this will correctly lock the right buttons.
        // This cannot be called from SaveManager.Start() because PannelManager.Start()
        // (which populates heroesQuestButtons) may not have run yet at that point,
        // causing an ArgumentOutOfRangeException.
        RestoreHeroSelectionState();
    }

    private void QuestFinishedHandler()
    {
        Debug.Log("Quest finished callback received");
        ResultOfTheQuest(simulationQuestData, selectedHeroesForQuest);
    }

    private void checkInterectableForSummon()
    {
        for (int i = 0; i < typeAvailable.Length; i++)
        {
            if (i > GameManager.Instance.GuildManager.unlockableHeroes)
            {
                heroesSummonButtons[i].interactable = false;
                heroSummonDelet[i].interactable = false;
                continue;
            }

            if (typeAvailable[i] > 0)
            {
                heroSummonDelet[i].interactable = true;
            }
            else
            {
                heroesSummonButtons[i].interactable = true;
                heroSummonDelet[i].interactable = false;
            }
        }
    }

    public void deactiveAllPannels()
    {
        foreach (GameObject pannel in pannels)
        {
            pannel.SetActive(false);
        }
    }

    void activePannel(int ind)
    {
        if (ind == 1)
        {
            GameManager.Instance.QuestManager.loadQuests();
        }
        if (ind == 4)
        {
            summonCost = 0;
            summonIds = new bool[6];
            typeAvailable = new int[6];
            summonHeroButton.interactable = false;
            checkInterectableForSummon();
        }
        else if (ind == 7)
        {
            if (!GameManager.Instance.QuestManager.questSelected) return;

            GameManager.Instance.heroSelectionForQuestUI.setMaxHeroNumber(
                GameManager.Instance.QuestManager.SelectedQD.maxPlayerCount);

            typeAvailable = new int[6];
            RestoreHeroSelectionState();
            Debug.Log("PlayerSelectionPannel");
        }

        if (activePannelObj != null)
            activePannelObj.SetActive(false);

        if (activePannelObj == pannels[ind])
        {
            activePannelObj.SetActive(false);
            activePannelObj = null;
            return;
        }

        pannels[ind].SetActive(true);
        activePannelObj = pannels[ind];
    }

    public void RestoreHeroSelectionState()
    {
        // Guard: heroesQuestButtons may not be populated yet if called too early
        if (heroesQuestButtons == null || heroesQuestButtons.Count == 0) return;

        for (int i = 0; i < heroesQuestButtons.Count; i++)
        {
            if (!GameManager.Instance.GuildManager.IsHeroUnlocked(i))
            {
                heroesQuestButtons[i].interactable = false;
                heroQuestDeletButtons[i].interactable = false;
                continue;
            }

            heroesQuestButtons[i].interactable = true;
            heroQuestDeletButtons[i].interactable = false;
        }

        foreach (int id in selectedHeroesForQuest)
        {
            // Guard: id must be a valid index
            if (id < 0 || id >= heroesQuestButtons.Count) continue;
            heroesQuestButtons[id].interactable = false;
            heroQuestDeletButtons[id].interactable = true;
        }
    }

    public void deactivePannel()
    {
        GameManager.Instance.QuestManager.questSelected = false;
        GameManager.Instance.QuestManager.details.text = "Quest Details";
        activePannelObj.SetActive(false);
        activePannelObj = null;
        for (int i = 0; i < typeAvailable.Length; i++)
        {
            typeAvailable[i] = 0;
        }
    }

    int summonCost = 0;
    bool[] summonIds = new bool[6];

    private void addHero(int id)
    {
        int val = GameManager.Instance.HeroSummoner.isSummonable(id, summonCost, true);

        if (val > summonCost)
        {
            summonCost = val;
            typeAvailable[id] += 1;
            summonIds[id] = true;
        }
        else
        {
            GameManager.Instance.UIManager.popUpPannel.SetActive(true);
            GameManager.Instance.popUpManager.ShowNotEnoughtGold();
        }
        checkInterectableForSummon();
        summonHeroButton.interactable = canSummon();
    }

    private bool canSummon()
    {
        foreach (bool val in summonIds)
        {
            if (val) return true;
        }
        return false;
    }

    private void removeHero(int id)
    {
        if (summonIds[id])
        {
            int val = GameManager.Instance.HeroSummoner.isSummonable(id, summonCost, false);
            summonCost = val;
            typeAvailable[id] -= 1;
            if (typeAvailable[id] == 0)
                summonIds[id] = false;
        }
        checkInterectableForSummon();
        summonHeroButton.interactable = canSummon();
    }

    private void AddGameObjects()
    {
        inventoryButton = GameManager.Instance.UIManager.InventoryButton;
        questButton = GameManager.Instance.UIManager.QuestsButton;
        heroButton = GameManager.Instance.UIManager.HeroesButton;
        buildingButton = GameManager.Instance.UIManager.BuildingsButton;
        summonButton = GameManager.Instance.UIManager.SummonPlayerButton;
        blackSmithButton = GameManager.Instance.UIManager.BlackSmithButton;
        pauseButton = GameManager.Instance.UIManager.PauseMenuButton;

        pannels.Add(GameManager.Instance.UIManager.InventoryPannel);
        pannels.Add(GameManager.Instance.UIManager.QuestPannel);
        pannels.Add(GameManager.Instance.UIManager.HeroPannel);
        pannels.Add(GameManager.Instance.UIManager.BuildingPannel);
        pannels.Add(GameManager.Instance.UIManager.SummonPlayerPannel);
        pannels.Add(GameManager.Instance.UIManager.BlackSmith);
        pannels.Add(GameManager.Instance.UIManager.PauseMenuPannel);
        pannels.Add(GameManager.Instance.UIManager.GoToQuestPannel);

        heroesSummonButtons.Add(GameManager.Instance.UIManager.AddBerberian);
        heroesSummonButtons.Add(GameManager.Instance.UIManager.AddArcher);
        heroesSummonButtons.Add(GameManager.Instance.UIManager.AddGiant);
        heroesSummonButtons.Add(GameManager.Instance.UIManager.AddWiz);
        heroesSummonButtons.Add(GameManager.Instance.UIManager.AddZimbie);
        heroesSummonButtons.Add(GameManager.Instance.UIManager.AddValkyri);

        heroSummonDelet.Add(GameManager.Instance.UIManager.DelBerberian);
        heroSummonDelet.Add(GameManager.Instance.UIManager.DelArcher);
        heroSummonDelet.Add(GameManager.Instance.UIManager.DelGiant);
        heroSummonDelet.Add(GameManager.Instance.UIManager.DelWiz);
        heroSummonDelet.Add(GameManager.Instance.UIManager.DelZimbie);
        heroSummonDelet.Add(GameManager.Instance.UIManager.DelValkyri);

        heroesQuestButtons.Add(GameManager.Instance.UIManager.QAddBerberian);
        heroesQuestButtons.Add(GameManager.Instance.UIManager.QAddArcher);
        heroesQuestButtons.Add(GameManager.Instance.UIManager.QAddGiant);
        heroesQuestButtons.Add(GameManager.Instance.UIManager.QAddWiz);
        heroesQuestButtons.Add(GameManager.Instance.UIManager.QAddZimbie);
        heroesQuestButtons.Add(GameManager.Instance.UIManager.QAddValkyri);

        heroQuestDeletButtons.Add(GameManager.Instance.UIManager.QDelBerberian);
        heroQuestDeletButtons.Add(GameManager.Instance.UIManager.QDelArcher);
        heroQuestDeletButtons.Add(GameManager.Instance.UIManager.QDelGiant);
        heroQuestDeletButtons.Add(GameManager.Instance.UIManager.QDelWiz);
        heroQuestDeletButtons.Add(GameManager.Instance.UIManager.QDelZimbie);
        heroQuestDeletButtons.Add(GameManager.Instance.UIManager.QDelValkyri);

        heroSelectionButton = GameManager.Instance.UIManager.chooseHeroesButton;
        GoToQuestButton = GameManager.Instance.UIManager.GoToQuestButton;
        summonHeroButton = GameManager.Instance.UIManager.SummonButton;
    }

    private void addListener()
    {
        inventoryButton.onClick.AddListener(() => activePannel(0));
        questButton.onClick.AddListener(() => activePannel(1));
        heroButton.onClick.AddListener(() => activePannel(2));
        buildingButton.onClick.AddListener(() => activePannel(3));
        summonButton.onClick.AddListener(() => activePannel(4));
        blackSmithButton.onClick.AddListener(() => activePannel(5));
        pauseButton.onClick.AddListener(() => activePannel(6));
        heroSelectionButton.onClick.AddListener(() =>
        {
            activePannel(7);
            GameManager.Instance.heroSelectionForQuestUI.ClearChildren();
        });

        summonHeroButton.onClick.AddListener(() => summonHeroes());
        GameManager.Instance.UIManager.play.onClick.AddListener(() => deactivePannel());

        for (int i = 0; i < heroesSummonButtons.Count; i++)
        {
            int captured = i;
            heroesSummonButtons[captured].onClick.AddListener(() => addHero(captured));
            heroSummonDelet[captured].onClick.AddListener(() => removeHero(captured));
            heroesQuestButtons[captured].onClick.AddListener(() => addHeroForQuest(captured));
            heroQuestDeletButtons[captured].onClick.AddListener(() => RemoveHeroForQuest(captured));
        }
    }

    int count = 0;
    List<int> selectedHeroesForQuest = new List<int>();

    private void addHeroForQuest(int id)
    {
        if (count < GameManager.Instance.QuestManager.SelectedQD.maxPlayerCount)
        {
            Debug.Log("PlayerAdded");
            selectedHeroesForQuest.Add(id);
            count++;
            heroesQuestButtons[id].interactable = false;
            heroQuestDeletButtons[id].interactable = true;
        }
        else
        {
            GameManager.Instance.UIManager.popUpPannel.SetActive(true);
            GameManager.Instance.popUpManager.ShowMaxPlayerCount();
        }
    }

    private void RemoveHeroForQuest(int id)
    {
        selectedHeroesForQuest.Remove(id);
        count--;
        heroesQuestButtons[id].interactable = true;
        heroQuestDeletButtons[id].interactable = false;
    }

    public void GoQuest(int cnt, List<(int, bool)> heroesForQuest)
    {
        OnQuestStarting?.Invoke(_guildData.gold);
        if (cnt <= 0) return;

        float hitDamage = 0;
        float hps = 0;
        float hp = 0;

        foreach ((int heroIndex, bool isActive) in heroesForQuest)
        {
            hitDamage += GameManager.Instance.HeroSummoner.getHeroPower(heroIndex);
            hps += GameManager.Instance.HeroSummoner.getHeroHitPerSecound(heroIndex);
            hp += GameManager.Instance.HeroSummoner.getHeroHP(heroIndex);
        }

        simulationQuestData = GameManager.Instance.QuestManager.SimulateCombat(hp, hps, hitDamage);
        DateTime startTime = DateTime.UtcNow;

        simulationQuestData.startTime = startTime;
        simulationQuestData.willWin = simulationQuestData.isCompleted;
        simulationQuestData.isCompleted = false;
        simulationQuestData.heroesForQuest = heroesForQuest.ConvertAll(h => h.Item1);

        gotoQuestPrefab.SetActive(false);

        ongoingQuest.AddQuestUI(simulationQuestData.uniqueId, simulationQuestData, startTime);
        GameManager.Instance.upgradeCounter.StartQuest(simulationQuestData.completionTime);

        Debug.Log("Quest Started: " + simulationQuestData.name);

        selectedHeroesForQuest.Clear();
        foreach ((int heroIndex, bool _) in heroesForQuest)
            selectedHeroesForQuest.Add(heroIndex);

        count = 0;

        GameManager.Instance.saveManager.SaveGame();
    }

    private void ResultOfTheQuest(QuestData questData, List<int> heroesForQuest)
    {
        if (questData != null && questData.willWin)
        {
            _upgradeCounter.QuestUpdate.text = "You Have Won The Quest!";
            Debug.Log("Wins");
            questData.isSelected = true;
            questData.isCompleted = true;
            GameManager.Instance.GuildManager.Gold += questData.goldRewardBase;
            GameManager.Instance.GuildManager.Experience += questData.experienceReward;

            foreach (int i in heroesForQuest)
            {
                Debug.Log("hero " + i);
                GameManager.Instance.HeroSummoner.heroDatas[i]
                    .upgradeHero((int)(questData.experienceReward / heroesForQuest.Count));
                Debug.Log(GameManager.Instance.HeroSummoner.heroDatas[i].xp + " " + i);
            }
        }
        else
        {
            if (questData != null) questData.isSelected = false;
            _upgradeCounter.QuestUpdate.text = "You Have Lost The Quest!";
            Debug.Log("loses");
        }

        if (questData != null && GameManager.Instance != null && GameManager.Instance.heroSelectionForQuestUI != null)
        {
            GameManager.Instance.heroSelectionForQuestUI.RestoreButtons(questData.heroesForQuest);
            Debug.Log("done quest - heroes unlocked");
            GameManager.Instance.heroSelectionForQuestUI.OnQuestComplete();
        }

        selectedHeroesForQuest.Clear();
        count = 0;

        // FIX: Save immediately after quest result so selectedHeroesForQuest is written
        // as empty to disk. Without this, if the player closes the app after the result
        // screen, the old hero indices remain in the save file and get re-locked on next launch.
        GameManager.Instance.saveManager.SaveGame();

        theResultIsOut = true;
        deactiveAllPannels();
    }

    private void summonHeroes()
    {
        GameManager.Instance.HeroSummoner.summonHeroes(typeAvailable, summonCost);
        deactivePannel();
    }
}