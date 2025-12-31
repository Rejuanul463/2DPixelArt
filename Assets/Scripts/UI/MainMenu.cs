using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject settingPannel;
    [SerializeField] private GameObject mainMenuPannel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private GameObject volumeFullSprite;
    [SerializeField] private GameObject volumeMidSprite;

    GameManager gameManager;

    private void Start()
    {

    }
    public void newGame()
    {
        continueButton.interactable = true;
        SceneManager.LoadScene("GameTown");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        settingPannel.SetActive(true);
    }

    public void Update()
    {
        AudioManager.Instance.setVolume(volumeSlider.value);

        if (volumeSlider.value == 0)
        {
            volumeFullSprite.SetActive(false);
            volumeMidSprite.SetActive(false);
        }
        else if (volumeSlider.value < 0.5f)
        {
            volumeFullSprite.SetActive(false);
            volumeMidSprite.SetActive(true);
        }
        else
        {
            volumeFullSprite.SetActive(true);
            volumeMidSprite.SetActive(false);
        }
    }
    public void back()
    {
        settingPannel.SetActive(false);
    }


}
