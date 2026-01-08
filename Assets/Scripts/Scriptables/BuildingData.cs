using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    enum BuildingType
    {
        House,
        Farm,
        Shop,
        BlackSmith,
        SummonPoint,
        Guild
    }

    [SerializeField] BuildingType buildingType;
    [SerializeField] public Sprite currentBuilding;
    [SerializeField] Sprite[] buildingSprites;
    [SerializeField] public int buildingLevel = 1;
    [SerializeField] public int maxBuildingLevel = 4;
    [SerializeField] public int upgradeCostGold = 100;
    [SerializeField] public int upgradeCostWood = 50;
    [SerializeField] public int upgradeCostStone = 30;
    [SerializeField] public float xpBoost = 20f;
    [SerializeField] public bool isUnderUpgrade = false;
    [SerializeField] public bool isUpgradable = true;
    [SerializeField] public long upgradeTime = 60;
    [SerializeField] public float UpgradeMultiplier = 1.5f;
    [SerializeField] public long upgradeStartTime;

    public void Upgrade()
    {
        
        if (buildingLevel >= maxBuildingLevel)
        {
            isUpgradable = false;
            return;
        }
        buildingLevel += 1;
        isUnderUpgrade = true;
        upgradeTime = upgradeTime * (long)UpgradeMultiplier;
        upgradeCostGold = (int)(upgradeCostGold * UpgradeMultiplier);
        upgradeCostWood = (int)(upgradeCostWood * UpgradeMultiplier);
        upgradeCostStone = (int)(upgradeCostStone * UpgradeMultiplier);
        xpBoost = (int)(xpBoost * UpgradeMultiplier);
    }

    public void CompleteUpgrade()
    {
        if (!isUpgradable) return;
        isUnderUpgrade = false;
        currentBuilding = buildingSprites[buildingLevel - 1];
    }
}
