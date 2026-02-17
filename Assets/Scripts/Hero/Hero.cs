using Pathfinding;
using System.Collections;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] public HeroData heroData;
    [SerializeField] private AIPath heroAIPath;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heroAIPath = GetComponent<AIPath>();
        heroAIPath.maxSpeed = heroData.speed;
        GetComponent<DestinationSetter>().UpdateHero(heroData.level);
    }

}
