using System.Collections;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] public BuildingData buildingData;

    public void upgradeBuilding()
    {
        if(buildingData.isUpgradable && !buildingData.isUnderUpgrade)
        {
            if(buildingData.upgradeCostGold <= GameManager.Instance.GuildManager.Gold &&
               buildingData.upgradeCostWood <= GameManager.Instance.GuildManager.Woods &&
               buildingData.upgradeCostStone <= GameManager.Instance.GuildManager.Stones)
            {
                GameManager.Instance.GuildManager.Gold -= buildingData.upgradeCostGold;
                GameManager.Instance.GuildManager.Woods -= buildingData.upgradeCostWood;
                GameManager.Instance.GuildManager.Stones -= buildingData.upgradeCostStone;
                buildingData.isUnderUpgrade = true;
                buildingData.upgradeStartTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();
                buildingData.Upgrade();
                upgradeCompletion(buildingData.upgradeTime);
            }
            else
            {
                GameManager.Instance.popUpManager.ShowNotEnoughtResources();
            }
        }
    }

    public void Start()
    {
        Debug.Log("workign");
        GetComponent<SpriteRenderer>().sprite = buildingData.currentBuilding;
    }

    //public bool upgradable = false;

    //public void Update()
    //{
    //    if (upgradable)
    //    {
    //        upgradable = false;
    //        buildingData.Upgrade();
    //        StartCoroutine(completeUpgrade(0));
    //        Debug.Log("Upgraded without resources for testing :" + buildingData.buildingLevel);
    //    }
        
    //}
    public void upgradeCompletion(long timeLeft)
    {
        StartCoroutine(completeUpgrade(timeLeft));
    }


    public virtual IEnumerator completeUpgrade(long timeLeft)
    {
        yield return new WaitForSeconds(timeLeft);
        if (buildingData.buildingLevel <= 3)
        {
            Debug.Log("Unlockable heroes increased to: ");
            GameManager.Instance.GuildManager.setUnlockableHeroes(GameManager.Instance.GuildManager.unlockableHeroes + 1);
        }
        else if (buildingData.buildingLevel == 4) GameManager.Instance.GuildManager.setUnlockableHeroes(6);
        buildingData.CompleteUpgrade();
    }
}
