using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [SerializeField] public SaveManager SaveManager;
    [SerializeField] public AudioManager AudioManager;
    public enum SceneStatus
    {
        newGame,
        ContinueGame
    }

    public SceneStatus CurrentSceneStatus { get; private set; } = SceneStatus.newGame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    
}
