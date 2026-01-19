using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroUIManager : MonoBehaviour
{
    [SerializeField] Button[] heroButtons;
    [SerializeField] HeroData[] heroDatas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        for (int i = 0; i < heroButtons.Length; i++)
        {
            int index = i; // cache
            heroButtons[index].onClick.AddListener(() => selectHero(index));
        }
    }

    void selectHero(int ind)
    {
        //add image and text
    }

}
