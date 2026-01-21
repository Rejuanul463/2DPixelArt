using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiHandler : MonoBehaviour
{
    [SerializeField] public Button[] itemButtons;
    [SerializeField] public Image itemImage;
    [SerializeField] public TextMeshProUGUI name;
    [SerializeField] TextMeshProUGUI description;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i; // cache
            itemButtons[index].onClick.AddListener(() => ShowDetails(index));
        }
    }

    public virtual void ShowDetails(int ind)
    {
        //add image and text
        itemImage.enabled = true;
        itemImage.sprite = itemButtons[ind].GetComponent<Image>().sprite;

        switch (ind)
        {
            case 0:
                name.text = "Stone";
                description.text = GameManager.Instance.GuildManager.Stones.ToString();
                break;

            case 1:
                name.text = "Wood";
                description.text = GameManager.Instance.GuildManager.Woods.ToString();
                break;
            case 2:
                name.text = "Gems";
                description.text = GameManager.Instance.GuildManager.Gems.ToString();
                break;
            case 3:
                name.text = "Gold";
                description.text = GameManager.Instance.GuildManager.Gold.ToString();
                break;
        }
    }
}
