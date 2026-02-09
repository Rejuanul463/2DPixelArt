using TMPro;
using UnityEngine;

public class GuildManager : MonoBehaviour
{
    [SerializeField] GuildData guildData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        updateResourcesUI();
    }

    public void updateResourcesUI()
    {
        GameManager.Instance.UIManager.gold.text = guildData.gold.ToString();
        GameManager.Instance.UIManager.gems.text = guildData.gems.ToString();

        GameManager.Instance.UIManager.guildLevel.text = "Guild Level : " + guildData.guildLevel.ToString();
    }

    public int Experience
    {
        get => guildData.currentExperience;
        set
        {
            guildData.currentExperience = Mathf.Max(0, value);
            if (guildData.currentExperience > guildData.experienceToNextLevel)
            {
                guildData.guildLevel += 1;
                // logic to update Experience to Next Level
            }
            updateResourcesUI();
        }
    }

    // -------- GOLD --------
    public int Gold
    {
        get => guildData.gold;
        set
        {
            guildData.gold = Mathf.Max(0, value);
            updateResourcesUI();
        }
    }

    // -------- GEMS --------
    public int Gems
    {
        get => guildData.gems;
        set
        {
            guildData.gems = Mathf.Max(0, value);
            updateResourcesUI();
        }
    }

    // -------- WOODS --------
    public int Woods
    {
        get => guildData.woods;
        set => guildData.woods = Mathf.Max(0, value);
    }

    // -------- STONES --------
    public int Stones
    {
        get => guildData.stones;
        set => guildData.stones = Mathf.Max(0, value);
    }

    // -------- GUILD LEVEL --------
    public int GuildLevel => guildData.guildLevel;

    public bool IsHeroUnlocked(int index)
    {
        if (index < 0 || index >= guildData.unlockedHeroID.Length)
            return false;

        return guildData.unlockedHeroID[index];
    }
    public void UnlockHero(int index)
    {
        if (index < 0 || index >= guildData.unlockedHeroID.Length)
            return;

        guildData.unlockedHeroID[index] = true;
    }
    public int unlockableHeroes
    {
        get => guildData.maxUnlockableHeroID;
        set => guildData.maxUnlockableHeroID += value;
    }
    public void setUnlockableHeroes(int value)
    {
        Debug.Log("Setting unlockable heroes to: " + value);
        guildData.maxUnlockableHeroID = value;
    }
}
