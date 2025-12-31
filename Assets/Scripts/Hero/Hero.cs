using Pathfinding;
using System;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public HeroType type;
    public int currentLevel = 1;

    public bool isActive = false;
    public bool isAvailable = true;

    public float cooldownEndTime;

    // Dynamic stats
    public float CurrentDPS => type.hitPerSecond * type.hitPower * Mathf.Pow(type.levelUpMultiplier, currentLevel - 1);
    public float CurrentHP => type.HP * Mathf.Pow(type.levelUpMultiplier, currentLevel - 1);
    public int SlotCost => type.slotCost;

    private void Update()
    {
        CheckCooldownRestore();
    }

    public void Activate()
    {
        if (!isAvailable) return;

        isActive = true;
        HeroManager.Instance.RegisterActiveHero(this);
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        HeroManager.Instance.UnregisterActiveHero(this);
        StartCooldown();
    }

    public void StartCooldown()
    {
        isAvailable = false;
        cooldownEndTime = GetTimestamp() + type.coolDownTime;
        PlayerPrefs.SetFloat("HERO_COOLDOWN_" + type.typeId + "_" + GetInstanceID(), cooldownEndTime);
        PlayerPrefs.Save();
    }

    public void CheckCooldownRestore()
    {
        if (!isAvailable)
        {
            float savedTime = PlayerPrefs.GetFloat("HERO_COOLDOWN_" + type.typeId + "_" + GetInstanceID(), 0);
            if (GetTimestamp() >= savedTime)
            {
                isAvailable = true;
            }
            else
            {
                cooldownEndTime = savedTime;
            }
        }
    }

    private float GetTimestamp()
    {
        return (float)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}




//[SerializeField] private HeroType heroType;
//[SerializeField] private AIPath aiPath;
//[SerializeField] private bool isOnCooldown;
//[SerializeField] private float cooldownRemaining;
////[SerializeField] private Animator animator;
//// Start is called once before the first execution of Update after the MonoBehaviour is created
//void Start()
//{
//    aiPath = GetComponent<AIPath>();
//    aiPath.maxSpeed = Math.Min(heroType.speed, 4f);
//}

//private void Update()
//{
//    if (isOnCooldown)
//    {
//        cooldownRemaining -= Time.deltaTime;
//        if (cooldownRemaining <= 0f)
//        {
//            isOnCooldown = false;
//            cooldownRemaining = 0f;
//            HeroManager.Instance.AddHero(heroType.typeId);
//        }
//    }
//}

//public void setCoolDown()
//{
//    HeroManager.Instance.RemoveHero(heroType.typeId);
//    isOnCooldown = true;
//    cooldownRemaining = heroType.coolDownTime;
//}