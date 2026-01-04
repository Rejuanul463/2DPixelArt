using UnityEngine;

public class HeroManager : MonoBehaviour
{
    [SerializeField] private HeroData[] heroDatas;

    
    public int isSummonable(int id)
    {
        if (heroDatas[id].goldCost <= GameManager.Instance.GuildManager.Gold)
        {
            return heroDatas[id].goldCost;
        }
        return 0;
    }


    public void summonHeroes(bool[] ids, int cost)
    {
        GameManager.Instance.GuildManager.Gold -= cost;
        for(int i = 0; i < ids.Length; i++)
        {
            if (ids[i])
            {
                Debug.Log("Summoned Hero: " + heroDatas[i].heroName);
            }
        }
    }
}
