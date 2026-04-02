using Mono.Cecil;
using UnityEngine;

public class PlayerInstantiate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnAllHeroes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnAllHeroes()
    {
        foreach (var heroData in SaveManager.Instance.heroDatas)
        {
            if (heroData.heroPrefab == null)
            {
                continue;
            }
            Vector3 spawnPos = new Vector3(-51.88f, 27.62f, 0f);

            GameObject heroObj = Instantiate(heroData.heroPrefab, spawnPos, Quaternion.identity);
            Hero heroComp = heroObj.GetComponent<Hero>();
            // Assign stats
            heroComp.heroData.name = heroData.name;
            heroComp.heroData.Id = heroData.Id;
            heroComp.heroData.uniqueId = heroData.uniqueId;
            heroComp.heroData.level = heroData.level;
            heroComp.heroData.hitPower = heroData.hitPower;
            heroComp.heroData.HP = heroData.HP;
            heroComp.heroData.goldPerAttack = heroData.goldPerAttack;
            heroComp.heroData.isHeroSummoned = heroData.isHeroSummoned;
        }
    }
}
