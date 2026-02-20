using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSummoner : Building
{
    [SerializeField] private HeroData[] heroClassTemplates;
    [SerializeField] public List<HeroData> heroDatas = new List<HeroData>();
    [SerializeField] private Transform summonPoint;

    [SerializeField] private Building blackSmith;

    public void LoadGame()
    {
        heroDatas = GameManager.Instance.saveManager.heroDatas;
    }

    public int isSummonable(int id , int currentCost)
    {
        if (heroClassTemplates[id].goldCost + currentCost <= GameManager.Instance.GuildManager.Gold)
        {
            return currentCost + heroClassTemplates[id].goldCost;
        }
        return currentCost;
    }
    public void summonHeroes(int[] ids, int cost)
    {
        GameManager.Instance.GuildManager.Gold -= cost;

        // Take a snapshot so coroutine data never changes
        int[] idsSnapshot = (int[])ids.Clone();

        StartCoroutine(summonAll(idsSnapshot));
    }

    IEnumerator summon(int heroIndex, int count)
    {
        for (int j = 0; j < count; j++)
        {
            yield return new WaitForSecondsRealtime(1f);

            // 1. Clone class template
            HeroData newHero = Instantiate(heroClassTemplates[heroIndex]);
            newHero.uniqueId = heroDatas.Count;
            // 3. Add to player hero list
            heroDatas.Add(newHero);

            // 4. Save immediately
            GameManager.Instance.saveManager.AddHero(newHero);

            GameManager.Instance.heroUI.AddButton(newHero);
            GameManager.Instance.heroSelectionForQuestUI.AddButton(newHero);

            // 5. Spawn prefab
            GameObject hero = Instantiate(
                newHero.heroPrefab,
                summonPoint.position,
                Quaternion.identity
            );

            hero.GetComponent<Hero>().heroData = newHero;


            heroDatas[newHero.uniqueId].isHeroSummoned = true;
        }

        GameManager.Instance.GuildManager.UnlockHero(heroIndex);
    }

    IEnumerator summonAll(int[] ids)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            Debug.Log("Summoning hero ID: " + i + " Count: " + ids[i]);
            if (ids[i] <= 0) continue;
            yield return StartCoroutine(summon(i, ids[i]));
        }

        Debug.Log("All heroes summoned");
    }

    public string getHeroName(int id)
    {
        return heroDatas[id].heroName;
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
