using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance;

    [Header("Hero Prefabs")]
    public HeroType[] heroTypes;
    public GameObject[] heroPrefabs;

    [Header("Slots & Limits")]
    public int maxSlotCapacity = 12;

    // All heroes owned by player
    public List<Hero> allHeroes = new List<Hero>();

    // Active heroes currently deployed
    public List<Hero> activeHeroes = new List<Hero>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --------------------------------------------------------
    // HERO SPAWNING
    // --------------------------------------------------------
    public void SpawnHero(int typeId, Transform spawnPoint)
    {
        GameObject go = Instantiate(heroPrefabs[typeId], spawnPoint.position, Quaternion.identity);
        Hero hero = go.GetComponent<Hero>();
        hero.type = heroTypes[typeId];
        hero.currentLevel = heroTypes[typeId].level;

        allHeroes.Add(hero);
    }

    // --------------------------------------------------------
    // HERO ACTIVATION / DEACTIVATION
    // --------------------------------------------------------
    public bool CanActivateHero(Hero hero)
    {
        if (!hero.isAvailable) return false;

        int usedSlots = GetUsedSlots();
        return usedSlots + hero.SlotCost <= maxSlotCapacity;
    }

    public void ActivateHero(Hero hero)
    {
        if (!CanActivateHero(hero)) return;

        hero.Activate();
    }

    public void DeactivateHero(Hero hero)
    {
        hero.Deactivate();
    }

    public void RegisterActiveHero(Hero hero)
    {
        if (!activeHeroes.Contains(hero))
            activeHeroes.Add(hero);
    }

    public void UnregisterActiveHero(Hero hero)
    {
        if (activeHeroes.Contains(hero))
            activeHeroes.Remove(hero);
    }

    // --------------------------------------------------------
    // STATS CALCULATION
    // --------------------------------------------------------
    public float GetTotalActiveDPS()
    {
        float total = 0;
        foreach (Hero hero in activeHeroes)
            total += hero.CurrentDPS;
        return total;
    }

    public float GetTotalActiveHP()
    {
        float total = 0;
        foreach (Hero hero in activeHeroes)
            total += hero.CurrentHP;
        return total;
    }

    public int GetUsedSlots()
    {
        int total = 0;
        foreach (Hero hero in activeHeroes)
            total += hero.SlotCost;
        return total;
    }

    public int GetRemainingSlots()
    {
        return maxSlotCapacity - GetUsedSlots();
    }

    // --------------------------------------------------------
    // UTILITY
    // --------------------------------------------------------
    public int GetTotalHeroCount(int typeId)
    {
        int count = 0;
        foreach (Hero h in allHeroes)
            if (h.type.typeId == typeId)
                count++;
        return count;
    }

    public int GetActiveHeroCount(int typeId)
    {
        int count = 0;
        foreach (Hero h in activeHeroes)
            if (h.type.typeId == typeId)
                count++;
        return count;
    }


    // Hero Generation for Summoning
    public int getHeroSummonCost(int type)
    {
        return heroTypes[type].goldCost;
    }

    public void SummonHero(int type, Transform spawner)
    {
        Instantiate(heroPrefabs[type], spawner.position, Quaternion.identity);
    }
}
