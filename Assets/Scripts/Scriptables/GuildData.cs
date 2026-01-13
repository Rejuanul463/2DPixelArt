using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

[CreateAssetMenu(fileName = "GuildData", menuName = "Scriptable Objects/GuildData")]
public class GuildData : ScriptableObject
{
    [SerializeField] public int guildLevel = 1;
    [SerializeField] public int currentExperience = 0;
    [SerializeField] public int experienceToNextLevel = 100;
    [SerializeField] public int gold = 0;
    [SerializeField] public bool[] unlockedHeroID;
    [SerializeField] public int maxUnlockableHeroID = 0;
    [SerializeField] public int gems = 0;
    [SerializeField] public int woods = 0;
    [SerializeField] public int stones = 0;
    [SerializeField] public int BlackSmithLevel = 1;
    [SerializeField] public int HeroSummonerLevel = 1;
    [SerializeField] public long[] questCompleteTime;
}
