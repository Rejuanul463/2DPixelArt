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

    // Update is called once per frame
    void Update()
    {

    }


    public void updateResourcesUI()
    {
        GameManager.Instance.UIManager.gold.text = guildData.gold.ToString();
        GameManager.Instance.UIManager.gems.text = guildData.gems.ToString();
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


    public int unlockedHeroes
    {
        get => guildData.maxUnlockedHeroID;
        set => guildData.maxUnlockedHeroID += 1;
    }

    public int unlockableHeroes
    {
        get => guildData.maxUnlockableHeroID;
        set => guildData.maxUnlockableHeroID += 1;
    }
}
