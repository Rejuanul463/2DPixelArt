using UnityEngine;
using System.Collections;

public class HeroSummoner : Building
{
    [SerializeField] private HeroData[] heroDatas;
    [SerializeField] private Transform summonPoint;

    [SerializeField] private Building blackSmith;

    public int isSummonable(int id , int currentCost)
    {
        if (heroDatas[id].goldCost + currentCost <= GameManager.Instance.GuildManager.Gold)
        {
            return currentCost + heroDatas[id].goldCost;
        }
        return currentCost;
    }

    public void summonHeroes(bool[] ids, int cost)
    {
        GameManager.Instance.GuildManager.Gold -= cost;
        for(int i = 0; i < ids.Length; i++)
        {
            if (ids[i])
            {
                Instantiate(heroDatas[i].heroPrefab, summonPoint.position, Quaternion.identity);
                heroDatas[i].isHeroSummoned = true;
                GameManager.Instance.GuildManager.UnlockHero(i);
            }
        }
    }

    public float getHeroHP(int id)
    {
        return heroDatas[id].HP;
    }
    public float getHeroHitPerSecound(int id)
    {
        return heroDatas[id].hitPerSecond;
    }
    public float getHeroPower(int id)
    {
        return heroDatas[id].hitPower;
    }

    public int getHeroLevel(int id)
    {
        return heroDatas[id].level;
    }

    public Sprite getCurrentHeroSprite(int id)
    {
        return heroDatas[id].heroSprite[getHeroLevel(id)];
    }

    public override IEnumerator completeUpgrade(long timeLeft)
    {
        yield return new WaitForSeconds(timeLeft);
        if (buildingData.buildingLevel <= 3)
        {
            Debug.Log("Unlockable heroes increased to: ");
            GameManager.Instance.GuildManager.setUnlockableHeroes(GameManager.Instance.GuildManager.unlockableHeroes + 1);
        }
        else if (buildingData.buildingLevel == 4) GameManager.Instance.GuildManager.setUnlockableHeroes(6);
        gameObject.GetComponent<SpriteRenderer>().sprite = buildingData.buildingSprites[buildingData.buildingLevel - 1];
        buildingData.CompleteUpgrade();

        if (buildingDataPref.isUnderUpgrade)
            buildingDataPref.CompleteUpgrade();
    }


    public bool UpgradeHero(int id)
    {
        if (getHeroLevel(id) >= blackSmith.buildingData.buildingLevel)
        {
            return false;
        }

        int reqGold = (int) (heroDatas[id].goldCost * heroDatas[id].levelUpMultiplier * getHeroLevel(id));


        if(GameManager.Instance.GuildManager.Gold < reqGold)
        {
            Debug.Log("NotEnoughtGold");
            GameManager.Instance.popUpManager.gameObject.SetActive(true);
            GameManager.Instance.popUpManager.ShowNotEnoughtGold();
            return false;
        }

        heroDatas[id].level += 1;
        heroDatas[id].goldCost = (int)(heroDatas[id].goldCost * heroDatas[id].levelUpMultiplier);
        heroDatas[id].HP = (int)(heroDatas[id].HP * heroDatas[id].levelUpMultiplier);
        heroDatas[id].hitPower = (int)(heroDatas[id].hitPower * heroDatas[id].levelUpMultiplier);

        GameManager.Instance.GuildManager.Gold -= reqGold;

        return true;

    }

    public bool isHeroSummoned(int id)
    {
        return heroDatas[id].isHeroSummoned;
    }

}
