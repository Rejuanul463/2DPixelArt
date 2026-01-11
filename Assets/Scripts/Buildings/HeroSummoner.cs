using UnityEngine;

public class HeroSummoner : Building
{
    [SerializeField] private HeroData[] heroDatas;
    [SerializeField] private Transform summonPoint;

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
}
