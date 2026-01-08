using System.Collections;
using UnityEngine;

public class GuildBuilding : Building
{
    public override IEnumerator completeUpgrade(long timeLeft)
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
