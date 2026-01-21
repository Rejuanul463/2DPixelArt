using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HeroUI : UiHandler
{

    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI hp;
    [SerializeField] TextMeshProUGUI damage;
    [SerializeField] TextMeshProUGUI dps;

    [SerializeField] Button UpgradeButton;

    private void OnEnable()
    {
        heroIconUpdate();
    }

    public override void ShowDetails(int ind)
    {
        UpgradeButton.onClick.RemoveAllListeners();
        UpgradeButton.onClick.AddListener( () => UpgradeHero(ind));

        itemImage.enabled = true;
        itemImage.sprite = itemButtons[ind].GetComponent<Image>().sprite;

        name.text = "Stone";
        level.text = "Level: " + GameManager.Instance.HeroSummoner.getHeroLevel(ind).ToString();
        hp.text = "HP: " + GameManager.Instance.HeroSummoner.getHeroHP(ind).ToString();
        damage.text = "Damage: " + GameManager.Instance.HeroSummoner.getHeroPower(ind).ToString();
        dps.text = "DPS: " + GameManager.Instance.HeroSummoner.getHeroHitPerSecound(ind).ToString();
    }


    public void heroIconUpdate()
    {
        
        for(int i = 0; i < itemButtons.Length; i++)
        {
            itemButtons[i].GetComponent<Image>().sprite = GameManager.Instance.HeroSummoner.getCurrentHeroSprite(i);
        }
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
