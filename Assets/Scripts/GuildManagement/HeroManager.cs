using UnityEngine;

public class HeroManager : MonoBehaviour
{
    [SerializeField] private HeroData[] heroDatas;

    
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
                Instantiate(heroDatas[i].heroPrefab, GameManager.Instance.SummonPoint.position, Quaternion.identity);
                GameManager.Instance.GuildManager.UnlockHero(i);
            }
        }
    }
}
