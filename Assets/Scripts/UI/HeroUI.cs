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

    //private void OnEnable()
    //{
    //    heroIconUpdate();
    //}


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
        UpgradeButton.onClick.AddListener( () => UpgradeHero(ind));

        itemImage.enabled = true;
        itemImage.sprite = itemButtons[ind].GetComponent<Image>().sprite;

        name.text = GameManager.Instance.HeroSummoner.getHeroName(ind);
        level.text = "Level: " + GameManager.Instance.HeroSummoner.getHeroLevel(ind).ToString();
        hp.text = "HP: " + GameManager.Instance.HeroSummoner.getHeroHP(ind).ToString();
        damage.text = "Damage: " + GameManager.Instance.HeroSummoner.getHeroPower(ind).ToString();
        dps.text = "DPS: " + GameManager.Instance.HeroSummoner.getHeroHitPerSecound(ind).ToString();
    }


    public void heroIconUpdate()
    {
        for(int i = 0; i < itemButtons.Count; i++)
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
        
        child.GetComponent<Button>().onClick.AddListener(() => ShowDetails(data.uniqueId));

        itemButtons.Add(child.GetComponent<Button>());
    }



    void UpgradeHero(int id)
    {
        if (GameManager.Instance.HeroSummoner.UpgradeHero(id))
        {
            itemButtons[id].GetComponent<Image>().sprite = GameManager.Instance.HeroSummoner.getCurrentHeroSprite(id);
            ShowDetails(id);
        }
    }

}
