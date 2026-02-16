using Pathfinding;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private AIPath heroAIPath;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heroAIPath = GetComponent<AIPath>();
        heroAIPath.maxSpeed = heroData.speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
