using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialSlideshow : MonoBehaviour
{
    [Header("UI")]
    public Image slideImage;

    [SerializeField] private GameObject tutorialPanel;
    [Header("Slides")]
    public Sprite[] slides;

    [Header("Settings")]
    public float slideDuration = 2f;
    public bool loop = true;

    private int currentIndex = 0;
    private Coroutine slideRoutine;

    public void SceneTransition()
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene");
    }
    public void StartSlideshow()
    {
        gameObject.SetActive(true);
        tutorialPanel.SetActive(true);

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(PlaySlides());
    }

    public void StopSlideshow()
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);
        tutorialPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    IEnumerator PlaySlides()
    {
        while (true)
        {
            slideImage.sprite = slides[currentIndex];

            yield return new WaitForSeconds(slideDuration);

            currentIndex++;

            if (currentIndex >= slides.Length)
            {
                if (loop)
                    currentIndex = 0;
                else
                    yield break;
            }
        }
    }
}