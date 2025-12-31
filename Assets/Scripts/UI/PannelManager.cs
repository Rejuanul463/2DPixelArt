using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PannelManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> pannels;
    [SerializeField] private Transform spawner;
    [SerializeField] private List<Button> heroesSummonButtons;
    [SerializeField] private List<Button> heroSummonDelet;
    public GameObject activePannelObj;
    private int[] typeCount;

    private bool isSpawning = false;
    private int spawnType = 0;

    [SerializeField] private int requiredGold;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddGameObjects();
        typeCount = new int[6];
        deactiveAllPannels();

    }

    private void Update()
    {
        if (isSpawning)
            StartCoroutine(summonCooldown(spawnType, 2f));
    }
    private void deactiveAllPannels()
    {
        foreach (GameObject pannel in pannels)
        {
            pannel.SetActive(false);
        }
    }

    public void activePannel(int ind)
    {
        if (ind == 4)
        {
            setHeroSummonButtonsInteractable(PlayerStats.Instance.getPlayerUnlocked());
            deinteractable();
        }
        if(activePannelObj != null)
            activePannelObj.SetActive(false);
        pannels[ind].SetActive(true);
        activePannelObj = pannels[ind];
    }

    public void deactivePannel()
    {
        activePannelObj.SetActive(false);
        activePannelObj = null;
    }

    public void heroTypeMultiplayer(int type)
    {
        requiredGold += HeroManager.Instance.getHeroSummonCost(type);
        if (!PlayerStats.Instance.CanAfford(requiredGold))
        {
            requiredGold -= HeroManager.Instance.getHeroSummonCost(type);
            return;
        }

        typeCount[type]++;
        heroSummonDelet[type].interactable = true;
    }
    public void deleteType(int type)
    {
        if(typeCount[type] > 0)
        {
            requiredGold -= HeroManager.Instance.getHeroSummonCost(type);
            typeCount[type]--;
        }
        if(typeCount[type] == 0)
        {
            heroSummonDelet[type].interactable = false;
        }
    }

    public void heroSummonButton()
    {
        PlayerStats.Instance.SpendGold(requiredGold);
        requiredGold = 0;
        deactivePannel();
        isSpawning = true;
        spawnType = 0;

    }

    IEnumerator summonCooldown(int type, float cooldown)
    {
        isSpawning = false;
        yield return new WaitForSeconds(cooldown);
        if (typeCount[type] > 0)
        {
            HeroManager.Instance.SummonHero(type, spawner);
            typeCount[type]--;
        }
        else
        {
            spawnType = type + 1;
        }

        if(spawnType == typeCount.Length)
        {
            isSpawning = false;
            spawnType = 0;
        }
        else
        {
            isSpawning = true;
        }
    }

    private void setHeroSummonButtonsInteractable(int numAvailable)
    {
        for(int i = 0; i <= Mathf.Min(heroesSummonButtons.Count, numAvailable); i++)
        {
            heroesSummonButtons[i].interactable = true;
        }

        for(int i = numAvailable + 1; i < heroesSummonButtons.Count; i++)
        {
            heroesSummonButtons[i].interactable = false;
        }
    }

    private void deinteractable()
    {
        for(int i = 0; i < heroesSummonButtons.Count; i++)
        {
            if(typeCount[i] == 0)
                heroSummonDelet[i].interactable = false;
        }
    }




    private void AddGameObjects()
    {
        pannels.Add(UIManager.Instance.InventoryPannel);
        pannels.Add(UIManager.Instance.QuestPannel);
        pannels.Add(UIManager.Instance.HeroPannel);
        pannels.Add(UIManager.Instance.BuildingPannel);
        pannels.Add(UIManager.Instance.SummonPlayerPannel);
        pannels.Add(UIManager.Instance.BlackSmith);
        pannels.Add(UIManager.Instance.PauseMenuPannel);
        pannels.Add(UIManager.Instance.GoToQuestPannel);

        heroesSummonButtons.Add(UIManager.Instance.AddBerberian);
        heroesSummonButtons.Add(UIManager.Instance.AddArcher);
        heroesSummonButtons.Add(UIManager.Instance.AddGiant);
        heroesSummonButtons.Add(UIManager.Instance.AddWiz);
        heroesSummonButtons.Add(UIManager.Instance.AddZimbie);
        heroesSummonButtons.Add(UIManager.Instance.AddValkyri);

        heroSummonDelet.Add(UIManager.Instance.DelBerberian);
        heroSummonDelet.Add(UIManager.Instance.DelArcher);
        heroSummonDelet.Add(UIManager.Instance.DelGiant);
        heroSummonDelet.Add(UIManager.Instance.DelWiz);
        heroSummonDelet.Add(UIManager.Instance.DelZimbie);
        heroSummonDelet.Add(UIManager.Instance.DelValkyri);
    }
}
