using System.Collections;
using UnityEngine;

public class GridManagement : MonoBehaviour
{

    [SerializeField] GameObject gridRiverTile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(AnimateRiver());
    }


    IEnumerator AnimateRiver()
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);
            gridRiverTile.SetActive(true);
            yield return new WaitForSeconds(.1f);
            gridRiverTile.SetActive(false);
        }
    }

    

}
