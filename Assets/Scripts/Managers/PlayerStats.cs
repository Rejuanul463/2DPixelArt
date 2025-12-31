using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    private int playerLevel = 1;
    private int playerUnlocked = 0;
    [SerializeField] private int gold = 0;
    private int experience = 0;

    private int nextUpgrade = 100;

    private float totalDPS;
    private float totalHP;
    private int totalGoldSpent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddRewards(int xp, int goldAmount)
    {
        experience += xp;
        gold += goldAmount;
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public bool CanAfford(int amount) => gold >= amount;

    public void SpendGold(int amount)
    {
        gold -= amount;
    }

    public void Update()
    {
        if (experience >= nextUpgrade)
        {
            playerLevel++;
            nextUpgrade += nextUpgrade * 2;
        }
    }

    public int getPlayerLevel()
    {
        return playerLevel;
    }
    public int getGold()
    {
        return gold;
    }

    public int getExperience()
    {
        return experience;
    }

    public float getTotalDPS()
    {
        return totalDPS;
    }
    public float getTotalHP()
    {
        return totalHP;
    }

    private void OnLevelWasLoaded()
    {
        if (playerLevel >= 10)
        {
            playerUnlocked = 5;
        }
        else if (playerLevel >= 8)
        {
            playerUnlocked = 4;
        }
        else if (playerLevel >= 6)
        {
            playerUnlocked = 3;
        }
        else if (playerLevel >= 4)
        {
            playerUnlocked = 2;
        }
        else if (playerLevel >= 2)
        {
            playerUnlocked = 1;
        }
        else
        {
            playerUnlocked = 0;
        }
    }

    public int getPlayerUnlocked()
    {
        return playerUnlocked;
    }
}
