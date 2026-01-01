using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
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
        //if (ind == 4)
        //{
        //    setHeroSummonButtonsInteractable(PlayerStats.Instance.getPlayerUnlocked());
        //    deinteractable();
        //}
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

    //public void heroTypeMultiplayer(int type)
    //{
    //    requiredGold += HeroManager.Instance.getHeroSummonCost(type);
    //    if (!PlayerStats.Instance.CanAfford(requiredGold))
    //    {
    //        requiredGold -= HeroManager.Instance.getHeroSummonCost(type);
    //        return;
    //    }

    //    typeCount[type]++;
    //    heroSummonDelet[type].interactable = true;
    //}
    //public void deleteType(int type)
    //{
    //    if(typeCount[type] > 0)
    //    {
    //        requiredGold -= HeroManager.Instance.getHeroSummonCost(type);
    //        typeCount[type]--;
    //    }
    //    if(typeCount[type] == 0)
    //    {
    //        heroSummonDelet[type].interactable = false;
    //    }
    //}

    //public void heroSummonButton()
    //{
    //    PlayerStats.Instance.SpendGold(requiredGold);
    //    requiredGold = 0;
    //    deactivePannel();
    //    isSpawning = true;
    //    spawnType = 0;

    //}

    IEnumerator  summonCooldown(int type, float cooldown)
    {
        isSpawning = false;
        yield return new WaitForSeconds(cooldown);
        if (typeCount[type] > 0)
        {
            //HeroManager.Instance.SummonHero(type, spawner);
            //typeCount[type]--;
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
        pannels.Add(SceneManager.Instance.UIManager.InventoryPannel);
        pannels.Add(SceneManager.Instance.UIManager.QuestPannel);
        pannels.Add(SceneManager.Instance.UIManager.HeroPannel);
        pannels.Add(SceneManager.Instance.UIManager.BuildingPannel);
        pannels.Add(SceneManager.Instance.UIManager.SummonPlayerPannel);
        pannels.Add(SceneManager.Instance.UIManager.BlackSmith);
        pannels.Add(SceneManager.Instance.UIManager.PauseMenuPannel);
        pannels.Add(SceneManager.Instance.UIManager.GoToQuestPannel);

        heroesSummonButtons.Add(SceneManager.Instance.UIManager.AddBerberian);
        heroesSummonButtons.Add(SceneManager.Instance.UIManager.AddArcher);
        heroesSummonButtons.Add(SceneManager.Instance.UIManager.AddGiant);
        heroesSummonButtons.Add(SceneManager.Instance.UIManager.AddWiz);
        heroesSummonButtons.Add(SceneManager.Instance.UIManager.AddZimbie);
        heroesSummonButtons.Add(SceneManager.Instance.UIManager.AddValkyri);

        heroSummonDelet.Add(SceneManager.Instance.UIManager.DelBerberian);
        heroSummonDelet.Add(SceneManager.Instance.UIManager.DelArcher);
        heroSummonDelet.Add(SceneManager.Instance.UIManager.DelGiant);
        heroSummonDelet.Add(SceneManager.Instance.UIManager.DelWiz);
        heroSummonDelet.Add(SceneManager.Instance.UIManager.DelZimbie);
        heroSummonDelet.Add(SceneManager.Instance.UIManager.DelValkyri);
    }
}
