using UnityEngine;

public class QuestNotificationPanelOff : MonoBehaviour
{
    [SerializeField] private GameObject questNotificationPannel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFalse()
    {
        questNotificationPannel.SetActive(false);
    }
}
