using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private Button GoToQuestButton;
    private Button summonHeroButton;

    [SerializeField] private List<GameObject> pannels;
    [SerializeField] private Transform spawner;
    [SerializeField] private List<Button> heroesSummonButtons;
    [SerializeField] private List<Button> heroSummonDelet;

    [SerializeField] private List<Button> heroesQuestButtons;
    [SerializeField] private List<Button> heroQuestDeletButtons;
    public GameObject activePannelObj;

    public bool[] typeAvailable;

    [SerializeField] private int requiredGold;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddGameObjects();
        deactiveAllPannels();
        addListener();
        typeAvailable = new bool[6];
    }



    private void checkInterectableForSummon()
    {
        for(int i = 0; i < typeAvailable.Length; i++)
        {
            if(GameManager.Instance.GuildManager.IsHeroUnlocked(i) || i > GameManager.Instance.GuildManager.unlockableHeroes)
            {
                heroesSummonButtons[i].interactable = false;
                heroSummonDelet[i].interactable = false;
                continue;
            }
            
            if (typeAvailable[i])
            {
                //Debug.Log("Disable summon button");
                heroesSummonButtons[i].interactable = false;
                heroSummonDelet[i].interactable = true;
            }
            else
            {
                heroesSummonButtons[i].interactable = true;
                heroSummonDelet[i].interactable = false;
            }
        }
    }

    private void checkInterectableForQuest()
    {
        for (int i = 0; i < typeAvailable.Length; i++)
        {
            if(!GameManager.Instance.GuildManager.IsHeroUnlocked(i))
            {
                heroesQuestButtons[i].interactable = false;
                heroQuestDeletButtons[i].interactable = false;
                continue;
            }
            if (typeAvailable[i])
            {
                heroesQuestButtons[i].interactable = false;
                heroQuestDeletButtons[i].interactable = true;
            }
            else
            {
                heroesQuestButtons[i].interactable = true;
                heroQuestDeletButtons[i].interactable = false;
            }
        }
    }


    private void deactiveAllPannels()
    {
        foreach (GameObject pannel in pannels)
        {
            pannel.SetActive(false);
        }
    }
    void activePannel(int ind)
    {
        if(ind == 4)
        {
            summonCost = 0;
            summonIds = new bool[6];
            typeAvailable = new bool[6];
            summonHeroButton.interactable = false;
            checkInterectableForSummon();
        }else if(ind == 7)
        {
            typeAvailable = new bool[6];
            Debug.Log("PlayerSelectionPannel");
            checkInterectableForQuest();
        }

        if (activePannelObj != null)
            activePannelObj.SetActive(false);

        pannels[ind].SetActive(true);
        activePannelObj = pannels[ind];
    }

    public void deactivePannel()
    {
        activePannelObj.SetActive(false);
        activePannelObj = null;

        for (int i = 0; i < typeAvailable.Length; i++)
        {
            typeAvailable[i] = false;
        }
    }

    int summonCost = 0;
    bool[] summonIds = new bool[6];

    private void addHero(int id)
    {
        int val = GameManager.Instance.HeroManager.isSummonable(id, summonCost);
        
        if(val > summonCost)
        {
            summonCost += val;
            typeAvailable[id] = true;
            summonIds[id] = true;
        }
        else
        {
            // Popup Insufficient Gold Message
            GameManager.Instance.UIManager.popUpPannel.SetActive(true);
            GameManager.Instance.popUpManager.ShowNotEnoughtGold();
        }
        checkInterectableForSummon();
        
        if (canSummon()) summonHeroButton.interactable = true;
        else summonHeroButton.interactable = false;
    }

    private bool canSummon()
    {
        foreach(bool val in summonIds)
        {
            if (val) return val;
        }

        return false;
    }

    private void removeHero(int id)
    {
        if(summonIds[id])
        {
            int val = GameManager.Instance.HeroManager.isSummonable(id, summonCost);
            summonCost -= val;
            typeAvailable[id] = false;
            summonIds[id] = false;
        }
        checkInterectableForSummon();
        
        if(canSummon()) summonHeroButton.interactable = true;
        else summonHeroButton.interactable = false;
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
        heroSelectionButton.onClick.AddListener(() => activePannel(7));

        GoToQuestButton.onClick.AddListener(() => GoQuest());
        summonHeroButton.onClick.AddListener(() => summonHeroes());

        GameManager.Instance.UIManager.play.onClick.AddListener(() => deactivePannel());

        heroesSummonButtons[0].onClick.AddListener(() => addHero(0));
        heroesSummonButtons[1].onClick.AddListener(() => addHero(1));
        heroesSummonButtons[2].onClick.AddListener(() => addHero(2));
        heroesSummonButtons[3].onClick.AddListener(() => addHero(3));
        heroesSummonButtons[4].onClick.AddListener(() => addHero(4));
        heroesSummonButtons[5].onClick.AddListener(() => addHero(5));

        heroSummonDelet[0].onClick.AddListener(() => removeHero(0));
        heroSummonDelet[1].onClick.AddListener(() => removeHero(1));
        heroSummonDelet[2].onClick.AddListener(() => removeHero(2));
        heroSummonDelet[3].onClick.AddListener(() => removeHero(3));
        heroSummonDelet[4].onClick.AddListener(() => removeHero(4));
        heroSummonDelet[5].onClick.AddListener(() => removeHero(5));
    }

    private void GoQuest()
    {

        //deactivePannel();
    }

    private void summonHeroes()
    {
        GameManager.Instance.HeroManager.summonHeroes(summonIds, summonCost);
        deactivePannel();
    }

}