using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroType")]
public class HeroData : ScriptableObject
{
    public GameObject heroPrefab;
    public Sprite[] heroSprite;
    public int Id;
    public string heroName;
    public int goldCost;
    public float hitPerSecond;
    public float hitPower;
    public float HP;
    public int level;
    public float levelUpMultiplier;
    public float coolDownTime;
    public int goldPerAttack;
    public float speed => 3 * hitPerSecond;
    public float DPS => hitPerSecond * hitPower;
}
