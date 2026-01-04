using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    [SerializeField] public int buildingLevel = 1;
    [SerializeField] public int upgradeCostGold = 100;
    [SerializeField] public int upgradeCostWood = 50;
    [SerializeField] public int upgradeCostStone = 30;
    [SerializeField] public float xpBoost = 20f;
    [SerializeField] public bool isUnderUpgrade = false;
    [SerializeField] public bool isUpgradable = true;
}
