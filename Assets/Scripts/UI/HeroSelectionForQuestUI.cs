using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionForQuestUI : MonoBehaviour
{
    [SerializeField] public GameObject ButtonContainer;
    [SerializeField] public GameObject SelectedButtonContainer;
    [SerializeField] public GameObject heroButtonPrefabe;
    [SerializeField] public List<Button> itemButtons = new List<Button>();

    [SerializeField] Button StartQuestButton;

    List<int > selectedHeroes = new List<int>();

    private int maxHeroNumber;
    private int count = 0;
    public void OnEnable()
    {
        StartQuestButton.onClick.AddListener(() => goQuest());
        StartQuestButton.interactable = false;

        foreach(int ind in selectedHeroes)
        {
            itemButtons[ind].interactable = true;
        }

        selectedHeroes.Clear();
        count = 0;
    }
    public void setMaxHeroNumber(int val)
    {
        maxHeroNumber = val;
        count = 0;
    }

    public void SelectForQuest(int ind)
    {
        if(count < maxHeroNumber)
        {
            count++;
            selectedHeroes.Add(ind);
            itemButtons[ind].interactable = false;
            CreateChildCopy(itemButtons[ind].gameObject, ind);
            StartQuestButton.interactable = true;
        }
        else
        {
            GameManager.Instance.UIManager.popUpPannel.SetActive(true);
            GameManager.Instance.popUpManager.ShowMaxPlayerCount();
        }
    }

    public void CreateChildCopy(GameObject Item, int ind)
    {
        GameObject newCopy = Instantiate(Item, SelectedButtonContainer.transform);
        newCopy.GetComponent<Button>().interactable = true;
        newCopy.GetComponent<Button>().onClick.AddListener(() => DeselectForQuest(ind, newCopy));
    }

    private void DeselectForQuest(int ind, GameObject copy)
    {
        count--;
        if(count == 0)
        {
            StartQuestButton.interactable = false;
        }
        selectedHeroes.Remove(ind);
        itemButtons[ind].interactable = true;
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
        GameManager.Instance.pannelManager.GoQuest(count, selectedHeroes);
    }
}
