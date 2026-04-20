using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    private int buildingId;
    [SerializeField] Button townhall;
    [SerializeField] Button summonPoint;
    [SerializeField] Button blackSmith;
    [SerializeField] Button building1;
    [SerializeField] Button building2;
    [SerializeField] Button building3;
    [SerializeField] Button Upgrade;

    [SerializeField] BuildingData[] buildingData;
    [SerializeField] Image imageHolder;

    [SerializeField] TextMeshProUGUI buildingLvl;
    [SerializeField] TextMeshProUGUI gold;
    [SerializeField] TextMeshProUGUI wood;
    [SerializeField] TextMeshProUGUI stone;
    [SerializeField] private List<Building>  building;

    [SerializeField] private GameObject buildingUpgradePanel;
    private Building buildingScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingScript = GetComponent<Building>();
       // Upgrade.interactable = false;
        assignEvent();
    }

    private void assignEvent()
    {
        townhall.onClick.AddListener(() => ActiveImage(0));
        summonPoint.onClick.AddListener(() => ActiveImage(1));
        blackSmith.onClick.AddListener(() => ActiveImage(2));
        building1.onClick.AddListener(() => ActiveImage(3));
        building2.onClick.AddListener(() => ActiveImage(4));
        building3.onClick.AddListener(() => ActiveImage(5));
        Upgrade.onClick.AddListener(() => updateBuilding(buildingId));
    }

// BuildingUI.cs — replace ActiveImage()
    private void ActiveImage(int ind)
    {
        buildingId = ind;
        imageHolder.gameObject.SetActive(true);
        buildingLvl.text = "Level : " + buildingData[ind].buildingLevel.ToString();
        gold.text = buildingData[ind].upgradeCostGold.ToString();
        wood.text = buildingData[ind].upgradeCostWood.ToString();
        stone.text = buildingData[ind].upgradeCostStone.ToString();
        imageHolder.sprite = buildingData[ind].currentBuilding;

        // ✅ FIX: Check all 3 resources, not just gold
        bool canAfford = buildingData[ind].upgradeCostGold <= GameManager.Instance.GuildManager.Gold &&
                         buildingData[ind].upgradeCostWood <= GameManager.Instance.GuildManager.Woods &&
                         buildingData[ind].upgradeCostStone <= GameManager.Instance.GuildManager.Stones;

        // ✅ FIX: Disable if already upgrading or not upgradable
        bool isReady = buildingData[ind].isUpgradable && !buildingData[ind].isUnderUpgrade;

        // ✅ FIX: Disable if non-townhall building is at or above townhall level
        bool notBlockedByTownHall = ind == 0 || buildingData[ind].buildingLevel < buildingData[0].buildingLevel;

        Upgrade.interactable = canAfford && isReady && notBlockedByTownHall;
    }

    private void updateBuilding(int id)
    {
       buildingUpgradePanel.SetActive(true);
        Building.OnUpgradeRequested?.Invoke(id);
    }
    
}