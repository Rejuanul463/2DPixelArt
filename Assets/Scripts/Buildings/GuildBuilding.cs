using System.Collections;
using UnityEngine;

public class GuildBuilding : Building
{
    public override IEnumerator completeUpgrade(long timeLeft)
    {
        Debug.Log("update");
        yield return new WaitForSeconds(timeLeft);
        gameObject.GetComponent<SpriteRenderer>().sprite = buildingData.buildingSprites[buildingData.buildingLevel - 1];
        buildingData.CompleteUpgrade();
        buildingDataPref = buildingData;
    }
}
