
using System;
using System.Collections.Generic;

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
   private List<(int,bool) > selectedHeroes = new List<(int,bool)>();
    private GameObject newCopy;
    private int maxHeroNumber;
    private int count = 0;
    private List<(GameObject,int)> heroButtons = new List<(GameObject,int)>();
    public List<(int,bool)> SelectedHeroes
    {
        get => selectedHeroes;
        set => selectedHeroes = value;
    }
    private void Start()
    {
        count = 0;
    }

    private void Update()
    {
        /*if (_pannelManager.TheResultIsOut)
        {
 
        }*/
    }
    public void SaveSelectedHeroes()
    {
        GameManager.Instance.saveManager.SaveGame();
    }

    public void LoadSelectedHeroes(List<int> saved)
    {
        if (saved == null || saved.Count == 0) return;

        foreach (int ind in saved)
        {
            if (ind >= itemButtons.Count) continue;

            selectedHeroes.Add((ind,true));
            count++;

            // Lock the button
            itemButtons[ind].interactable = false;

            // Rebuild the copy in SelectedButtonContainer
            CreateChildCopy(itemButtons[ind].gameObject, ind);
        }

        if (count > 0)
            StartQuestButton.interactable = true;
    }
    public void OnEnable()
    {
        StartQuestButton.onClick.AddListener(() => goQuest());
        StartQuestButton.interactable = false;

        foreach((int,bool) ind in selectedHeroes)
        {
            if(!ind.Item2) itemButtons[ind.Item1].interactable = true;
        }
        
        /*selectedHeroes.Clear();*/
        count = 0;
    }
    public void setMaxHeroNumber(int val)
    {
        maxHeroNumber = val;
        count = 0;
    }

// In HeroSelectionForQuestUI.cs

    public void SelectForQuest(int ind)
    {
        
        if(count < maxHeroNumber)
        {
            count++;
            selectedHeroes.Add((ind,true));
            itemButtons[ind].interactable = false;
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


    public void CreateChildCopy(GameObject Item, int ind)
    {
        newCopy = Instantiate(Item, SelectedButtonContainer.transform);
        //newCopy.GetComponent<Button>().interactable = true;
        newCopy.GetComponent<Button>().onClick.AddListener(() => DeselectForQuest(ind, newCopy));
    }

    private void DeselectForQuest(int ind, GameObject copy)
    {
        if (count >= 0)
        {
            count--;
        }
        if(count <= 0)
        {
            StartQuestButton.interactable = false;
        }
        selectedHeroes.Remove((ind,false));
        itemButtons[ind].interactable = true;
        //itemButtons[ind].gameObject.SetActive(true);
        Destroy(copy);
    }


    public void heroIconUpdate()
    {
        for (int i = 0; i < itemButtons.Count; i++)
        {
            itemButtons[i].GetComponent<Image>().sprite = GameManager.Instance.HeroSummoner.getCurrentHeroSprite(i);
            if (!GameManager.Instance.HeroSummoner.isHeroSummoned(i))
            {
                itemButtons[i].gameObject.SetActive(false);
            }
            else
            {
                itemButtons[i].gameObject.SetActive(true);
            }
        }
    }

    public void AddButton(HeroData data)
    {
        GameObject child = Instantiate(heroButtonPrefabe, ButtonContainer.transform);
        child.GetComponent<Image>().sprite = data.heroSprite[0];

        child.GetComponent<Button>().onClick.AddListener(() => SelectForQuest(data.uniqueId));

        itemButtons.Add(child.GetComponent<Button>());
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
        TextPanel.SetActive(true);
        /*foreach (var VARIABLE in itemButtons)
        {
            VARIABLE.interactable = false;
        }*/
        GameManager.Instance.pannelManager.GoQuest(count, selectedHeroes);
    }
}
