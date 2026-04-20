using UnityEngine;
using UnityEngine.UI;

public class BuildingCounterOFf : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonClicked()
    {
        notificationPanel.SetActive(false);
        button.gameObject.SetActive(false);
    }
    
}
