using System;
using System.Collections;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] public BuildingData buildingDataPref;
    public static Action<int> OnUpgradeRequested;
    protected BuildingData buildingData;

    void Awake()
    {
        buildingData = Instantiate(buildingDataPref);
    }

    private void OnEnable()
    {
        OnUpgradeRequested += HandleUpgradeRequest;
    }

    private void OnDisable()
    {
        OnUpgradeRequested -= HandleUpgradeRequest;
    }

    private void HandleUpgradeRequest(int id)
    {

        if (buildingData.buildingID != id)
            return;
        Debug.Log("upgrade");
        upgradeBuilding();
    }

    public void upgradeBuilding()
    {
        if(buildingData.isUpgradable && !buildingData.isUnderUpgrade)
        {
            if (!buildingData.isTownHall())
            {
                if(buildingData.buildingLevel == GameManager.Instance.TownHall.buildingLevel)
                {
                    GameManager.Instance.popUpManager.ShowNotAvailable();
                    return;
                }
            }
            if (buildingData.upgradeCostGold <= GameManager.Instance.GuildManager.Gold &&
               buildingData.upgradeCostWood <= GameManager.Instance.GuildManager.Woods &&
               buildingData.upgradeCostStone <= GameManager.Instance.GuildManager.Stones)
            {
                GameManager.Instance.GuildManager.Gold -= buildingData.upgradeCostGold;
                GameManager.Instance.GuildManager.Woods -= buildingData.upgradeCostWood;
                GameManager.Instance.GuildManager.Stones -= buildingData.upgradeCostStone;
                buildingData.isUnderUpgrade = true;
                buildingData.upgradeStartTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();
                buildingDataPref.upgradeStartTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                buildingData.Upgrade();
                if(!buildingDataPref.isUnderUpgrade && buildingDataPref.isUpgradable)
                    buildingDataPref.Upgrade();
                upgradeCompletion(buildingData.upgradeTime);
                Debug.Log("upgradeCalled");
            }
            else
            {
                Debug.Log("NotEnough resource");
                GameManager.Instance.popUpManager.ShowNotEnoughtResources();
            }
        }

    }

    public void Start()
    {
        GetComponent<SpriteRenderer>().sprite = buildingData.currentBuilding;
    }

    public void upgradeCompletion(long timeLeft)
    {
        Debug.Log("CallingUpdate");
        StartCoroutine(completeUpgrade(timeLeft));
    }


    public virtual IEnumerator completeUpgrade(long timeLeft)
    {
        Debug.Log("update");
        yield return new WaitForSeconds(timeLeft);
        gameObject.GetComponent<SpriteRenderer>().sprite = buildingData.buildingSprites[buildingData.buildingLevel - 1];
        buildingData.CompleteUpgrade();
        if(buildingDataPref.isUnderUpgrade)
            buildingDataPref.CompleteUpgrade();
    }
}
