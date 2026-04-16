using TMPro;
using UnityEngine;

public class EconomicUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalGoldText;
    [SerializeField] private TextMeshProUGUI requiredGoldText;
    [SerializeField] GuildData guildData;

    private void OnEnable()
    {
        HeroSelectionForQuestUI.OnRequiredGoldChanged += UpdateRequiredGold;
        UpdateTotalGold(); // refresh total gold when panel opens
    }

    private void OnDisable()
    {
        HeroSelectionForQuestUI.OnRequiredGoldChanged -= UpdateRequiredGold;
    }

    private void Update()
    {
        UpdateTotalGold(); // keep total gold live
    }

    private void UpdateRequiredGold(int amount)
    {
        requiredGoldText.text =$"Required Gold: <color=yellow>" + amount.ToString();
        
    }

    private void UpdateTotalGold()
    {
        totalGoldText.text =$"Total Gold: <color=yellow>"+ guildData.gold.ToString();
    }
}