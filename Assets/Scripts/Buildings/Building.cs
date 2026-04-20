using System;
using System.Collections;
using UnityEngine;
using TMPro;
public class Building : MonoBehaviour
{
    [SerializeField] public BuildingData buildingDataPref;
    public static Action<int> OnUpgradeRequested;
    [HideInInspector] public BuildingData buildingData;
    private float upgradeTime;
    [SerializeField] private GameObject buildingUpgradeObject;
   [SerializeField] private TextMeshProUGUI buildingUpgradeText;

   private BuildingCounter _buildingCounter;
    //[SerializeField] private GameObject buildingUpgradePanel;
    public float UpgradeTime
    {
        get => upgradeTime;
        set => upgradeTime = value;
    }
    //[SerializeField] private TextMeshProUGUI upgradeText;
    void Awake()
    {
        _buildingCounter =  GetComponent<BuildingCounter>();
         //buildingUpgradePanel.SetActive(true);
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
       
        if (buildingData.isUpgradable && !buildingData.isUnderUpgrade)
        {
            
            if (!buildingData.isTownHall())
            {
                if (buildingData.buildingLevel == GameManager.Instance.TownHall.buildingLevel)
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
                Debug.Log("Building is being upgraded!!");
                buildingData.isUnderUpgrade = true;
                buildingData.upgradeStartTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();
                buildingDataPref.upgradeStartTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();

                buildingData.Upgrade();
                if (!buildingDataPref.isUnderUpgrade && buildingDataPref.isUpgradable)
                    buildingDataPref.Upgrade();

                // ✅ FIX: Actually start the upgrade timer coroutine
                StartCoroutine(completeUpgrade(buildingData.upgradeTime));

                GameManager.Instance.saveManager.SaveGame(); // ✅ save after upgrade starts
                Debug.Log("upgradeCalled");
            }
            else
            {
                buildingUpgradeObject.SetActive(true);
                buildingUpgradeText.text = "Not Enough Resources!";
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
        buildingUpgradeObject.SetActive(false);
    }
    
    public virtual IEnumerator completeUpgrade(long timeLeft)
    {
        //upgradeTime = timeLeft;
        float reamainingTime = (float)timeLeft;
        Debug.Log("timeleft "+timeLeft );
        while (reamainingTime > 0f)
        {
            upgradeTime = reamainingTime;
            buildingUpgradeText.text = "Building Update will finish " + reamainingTime + " seconds.";
            yield return new WaitForSeconds(1f);
            reamainingTime -= 1f;
        }
        upgradeTime = 0f;
        buildingUpgradeText.text = "Building Update Finished!!";
        Debug.Log("update completed");
        gameObject.GetComponent<SpriteRenderer>().sprite = buildingData.buildingSprites[buildingData.buildingLevel - 1];
        buildingData.CompleteUpgrade();
        if(buildingDataPref.isUnderUpgrade)
            buildingDataPref.CompleteUpgrade();
    }
}
