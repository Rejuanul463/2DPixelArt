using UnityEngine;

[CreateAssetMenu(fileName = "GearData", menuName = "Scriptable Objects/GearData")]
public class GearData : ScriptableObject
{
    public enum GearType
    {
        Weapon,
        Armor,
        Accessory
    }

    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
    
    public string gearName;
    public GearType gearType;
    public Rarity rarity;
    public Sprite gearSprite;
    public int attackBonus;
    public int defenseBonus;
    public float speedEffect;
    public float level;
    public float levelUpMultiplier;
    public float levelUpCost;
}
