using Pathfinding;
using System.Collections;
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


        StartCoroutine(SetDestination());
    }

    
    IEnumerator SetDestination()
    {
        while (true)
        {
            if (aiPath.reachedDestination == true)
            {
                //play idle animation
                yield return new WaitForSeconds(.5f);
                //Play walk animation
                int x;
                do
                {
                    x = Random.Range(0, destinationPoints.points.Length);
                } while (x == ignoreIndex);

                ignoreIndex = x;
                DestinationPoint.transform.position = destinationPoints.points[x];
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetQuestDestination(Vector3 pos)
    {
        StopAllCoroutines();
        DestinationPoint.transform.position = pos;
    }

    public void BackFromQuest()
    {
        StartCoroutine(SetDestination());
    }
}
