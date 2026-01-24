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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Upgrade.interactable = false;
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

    private void ActiveImage(int ind)
    {
        if (buildingData[ind].upgradeCostGold <= GameManager.Instance.GuildManager.Gold)
            Upgrade.interactable = true;
        else
            Upgrade.interactable = false;

        if(ind != 0)
        {
            if (buildingData[ind].buildingLevel >= buildingData[0].buildingLevel)
                Upgrade.interactable = false;
        }

        buildingId = ind;

        imageHolder.gameObject.SetActive(true);
        buildingLvl.text = "Level : " + buildingData[ind].buildingLevel.ToString();
        gold.text = buildingData[ind].upgradeCostGold.ToString();
        gold.text = buildingData[ind].upgradeCostWood.ToString();
        stone.text = buildingData[ind].upgradeCostStone.ToString();

        imageHolder.sprite = buildingData[ind].currentBuilding;
    }

    private void updateBuilding(int id)
    {
        Building.OnUpgradeRequested?.Invoke(id);
    }
}