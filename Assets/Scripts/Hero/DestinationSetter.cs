using Pathfinding;
using UnityEngine;
public class DestinationSetter : MonoBehaviour
{
    [SerializeField] private DestinationPoints destinationPoints;
    private GameObject DestinationPoint;

    [HideInInspector] public AIPath aiPath;
    [HideInInspector] private AIDestinationSetter destinationSetter;

    private int ignoreIndex = -1;

    private void Start()
    {
        DestinationPoint = new GameObject("HeroDestination");
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        destinationSetter.target = DestinationPoint.transform;
    }

    private void Update()
    {
        if(aiPath.reachedDestination == true)
        {
            SetDestination();
        }
    }
    public void SetDestination()
    {
        int x;
        do
        {
            x = Random.Range(0, destinationPoints.points.Length);
        } while(x == ignoreIndex);
        ignoreIndex = x;
        DestinationPoint.transform.position = destinationPoints.points[x];
    }
}
