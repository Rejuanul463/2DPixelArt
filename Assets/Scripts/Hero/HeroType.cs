using UnityEngine;

[CreateAssetMenu(fileName = "HeroType", menuName = "Scriptable Objects/HeroType")]
public class HeroType : ScriptableObject
{
    public GameObject heroPrefab;
    public int typeId;
    public string heroName;
    public int goldCost;
    public float hitPerSecond;
    public float hitPower;
    public float HP;
    public int slotCost;
    public int level;
    public float levelUpMultiplier;
    public float coolDownTime;
    public int goldPerAttack;
    public float speed => 3 * hitPerSecond;
    public float DPS => hitPerSecond * hitPower;
}
