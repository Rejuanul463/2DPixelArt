using JetBrains.Annotations;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    [SerializeField] public UIManager UIManager;
    [SerializeField] public GuildManager GuildManager;
    [SerializeField] public HeroManager HeroManager;

    [SerializeField] public PopUPManager popUpManager;
    [SerializeField] public Transform SummonPoint;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        popUpManager = UIManager.popUpPannel.GetComponent<PopUPManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

}
