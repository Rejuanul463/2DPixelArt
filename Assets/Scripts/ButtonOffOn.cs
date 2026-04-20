using UnityEngine;

public class ButtonOffOn : MonoBehaviour
{
    [SerializeField] private GameObject button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnOn()
    {
        button.SetActive(true);
    }

    public void TurnOff()
    {
        button.SetActive(false);
    }
    
}
