using UnityEngine;
using UnityEngine.Rendering;

public class GoldGenerator : MonoBehaviour
{

    [SerializeField] private float startTime;
    [SerializeField] private float requiredTime;
    [SerializeField] private int goldAmount;

    [SerializeField] private GameObject[] goldMine;
    [SerializeField] private int currentGoldMineIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
        requiredTime = 5f;
        goldAmount = 25;
        currentGoldMineIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - startTime >= requiredTime)
        {
            goldMine[currentGoldMineIndex].SetActive(true);
            currentGoldMineIndex = (currentGoldMineIndex + 1) % goldMine.Length;
            startTime = Time.time;
        }
    }

    public void collectGold(int index)
    {
        goldMine[index].SetActive(false);
        GameManager.Instance.GuildManager.Gold += goldAmount;
    }
}
