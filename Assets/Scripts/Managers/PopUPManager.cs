using System;
using System.Collections;
using UnityEngine;

public class PopUPManager : MonoBehaviour
{
    [SerializeField] private GameObject notEnoughtGold;
    [SerializeField] private GameObject notEnoughtGems;
    [SerializeField] private GameObject notAvailable;

    [HideInInspector] private GameObject currentActive;
    private void OnEnable()
    {
        StartCoroutine(disable());
    }

    public void ShowNotEnoughtGold()
    {
        if(currentActive != null)    currentActive.SetActive(false);
        currentActive = notEnoughtGold;
        notEnoughtGold.SetActive(true);
    }

    public void ShowNotEnoughtGems()
    {
        if (currentActive != null) currentActive.SetActive(false);
        currentActive = notEnoughtGems;
        notEnoughtGems.SetActive(true);
    }

    public void ShowNotAvailable()
    {
        if (currentActive != null) currentActive.SetActive(false);
        currentActive = notAvailable;
        notAvailable.SetActive(true);
    }

    IEnumerator disable()
    {
        yield return new WaitForSeconds(1f);
        currentActive.SetActive(false);
        gameObject.SetActive(false);
    }
}
