using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a step-by-step guided tutorial overlay.
///
/// HOW TO SET UP IN UNITY:
/// 1. Create an empty GameObject in your Game scene called "TutorialManager"
///    and attach this script.
/// 2. Build the overlay UI (all inside a full-screen Canvas set to Screen Space – Overlay,
///    sort order above everything else):
///
///    TutorialCanvas (Canvas, CanvasScaler, GraphicRaycaster)
///    └── BlockerPanel        (Image, color 0,0,0,0  — catches all input outside spotlight)
///    └── SpotlightCutout     (Image with a circle/rounded-rect sprite, Raycast Target OFF)
///    └── BubblePanel         (Image — the speech bubble background)
///        ├── TitleText       (TextMeshProUGUI — bold headline)
///        ├── MessageText     (TextMeshProUGUI — body text)
///        ├── NextButton      (Button + TextMeshProUGUI "Next / Got it!")
///        └── SkipButton      (Button + TextMeshProUGUI "Skip Tutorial")
///
/// 3. Assign all [SerializeField] references in the Inspector.
/// 4. Add your TutorialStep entries to the `steps` list.
/// 5. Call TutorialManager.Instance.StartTutorial() from your game-start logic,
///    or enable "autoStartIfFirstTime" to let it fire automatically.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject blockerPanel;
    [SerializeField] private RectTransform spotlightRect;
    [SerializeField] private GameObject bubblePanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI nextButtonText;
    [SerializeField] private Button skipButton;

    [Header("Steps")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    [Header("Settings")]
    [Tooltip("Automatically start the tutorial the very first time the player launches the game.")]
    [SerializeField] private bool autoStartIfFirstTime = true;

    [Tooltip("Label on the Next button for all steps except the last.")]
    [SerializeField] private string nextLabel = "Next →";

    [Tooltip("Label on the Next button on the final step.")]
    [SerializeField] private string finishLabel = "Got it!";

    [Tooltip("How long (seconds) the spotlight takes to animate to a new target.")]
    [SerializeField] private float spotlightAnimDuration = 0.3f;

    // ── State ──────────────────────────────────────────────────────────────────
    private int currentStep = -1;
    private bool tutorialActive = false;
    private bool waitingForTargetTap = false;
    private Coroutine spotlightCoroutine;

    // PlayerPrefs key — tutorials are shown once per install
    private const string PREF_KEY = "TutorialComplete";

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        tutorialCanvas.SetActive(false);

        nextButton.onClick.AddListener(OnNextPressed);
        skipButton.onClick.AddListener(SkipTutorial);

        if (autoStartIfFirstTime && !PlayerPrefs.HasKey(PREF_KEY))
            StartTutorial();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Show the tutorial from step 0.</summary>
    public void StartTutorial()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("TutorialManager: no steps defined.");
            return;
        }

        tutorialActive = true;
        tutorialCanvas.SetActive(true);
        currentStep = -1;
        GoToNextStep();
    }

    /// <summary>
    /// Call this from game code when the player completes an action that a
    /// waitForAction step is waiting for (e.g. after they tap the Quest button).
    /// </summary>
    public void AdvanceFromCode()
    {
        if (!tutorialActive) return;
        GoToNextStep();
    }

    /// <summary>Returns true while the tutorial is running.</summary>
    public bool IsActive => tutorialActive;

    // ── Navigation ─────────────────────────────────────────────────────────────

    private void OnNextPressed()
    {
        if (!tutorialActive) return;
        GoToNextStep();
    }

    private void GoToNextStep()
    {
        currentStep++;

        if (currentStep >= steps.Count)
        {
            EndTutorial();
            return;
        }

        ShowStep(steps[currentStep]);
    }

    private void ShowStep(TutorialStep step)
    {
        // Update text
        titleText.text = step.title;
        messageText.text = step.message;

        // Next button label
        bool isLast = currentStep == steps.Count - 1;
        nextButtonText.text = isLast ? finishLabel : nextLabel;

        // Next button visibility
        bool showNext = !step.waitForAction && !step.requireTargetTap;
        nextButton.gameObject.SetActive(showNext);

        // Spotlight
        if (step.highlightTarget != null)
        {
            AnimateSpotlightTo(step.highlightTarget, step.highlightPadding);

            if (step.requireTargetTap)
            {
                waitingForTargetTap = true;
                StartCoroutine(WaitForTargetTap(step.highlightTarget));
            }
        }
        else
        {
            // No target — hide spotlight, show full blocker
            HideSpotlight();
        }

        // Animate bubble in
        StartCoroutine(AnimateBubbleIn());
    }

    // ── Spotlight ──────────────────────────────────────────────────────────────

    private void AnimateSpotlightTo(RectTransform target, float padding)
    {
        if (spotlightCoroutine != null) StopCoroutine(spotlightCoroutine);
        spotlightCoroutine = StartCoroutine(MoveSpotlight(target, padding));
        spotlightRect.gameObject.SetActive(true);
    }

    private void HideSpotlight()
    {
        if (spotlightCoroutine != null) StopCoroutine(spotlightCoroutine);
        spotlightRect.gameObject.SetActive(false);
    }

    private IEnumerator MoveSpotlight(RectTransform target, float padding)
    {
        // Convert target world corners to this canvas space
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // corners: 0=BL, 1=TL, 2=TR, 3=BR
        Vector2 targetMin = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 targetMax = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        Vector2 targetSize = new Vector2(
            Mathf.Abs(targetMax.x - targetMin.x) + padding * 2,
            Mathf.Abs(targetMax.y - targetMin.y) + padding * 2
        );
        Vector2 targetCenter = (targetMin + targetMax) / 2f;

        // Convert screen position to local position in spotlight's parent
        RectTransform parent = spotlightRect.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, targetCenter, null, out Vector2 localCenter);

        Vector2 startSize = spotlightRect.sizeDelta;
        Vector2 startPos = spotlightRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < spotlightAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / spotlightAnimDuration);
            spotlightRect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            spotlightRect.anchoredPosition = Vector2.Lerp(startPos, localCenter, t);
            yield return null;
        }

        spotlightRect.sizeDelta = targetSize;
        spotlightRect.anchoredPosition = localCenter;
    }

    // ── Target Tap Detection ───────────────────────────────────────────────────

    private IEnumerator WaitForTargetTap(RectTransform target)
    {
        waitingForTargetTap = true;

        // Add a temporary transparent button on top of the spotlight so the
        // player can actually tap the target through the blocker
        GameObject tapZone = new GameObject("TapZone", typeof(RectTransform));
        tapZone.transform.SetParent(tutorialCanvas.transform, false);

        RectTransform tapRt = tapZone.GetComponent<RectTransform>();
        tapRt.sizeDelta = spotlightRect.sizeDelta;
        tapRt.anchoredPosition = spotlightRect.anchoredPosition;

        Image tapImg = tapZone.AddComponent<Image>();
        tapImg.color = Color.clear;

        Button tapBtn = tapZone.AddComponent<Button>();
        bool tapped = false;
        tapBtn.onClick.AddListener(() => tapped = true);

        yield return new WaitUntil(() => tapped);

        Destroy(tapZone);
        waitingForTargetTap = false;
        GoToNextStep();
    }

    // ── Bubble Animation ───────────────────────────────────────────────────────

    private IEnumerator AnimateBubbleIn()
    {
        bubblePanel.transform.localScale = Vector3.one * 0.8f;
        float elapsed = 0f;
        float dur = 0.18f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
            bubblePanel.transform.localScale = Vector3.Lerp(
                Vector3.one * 0.8f, Vector3.one, t);
            yield return null;
        }
        bubblePanel.transform.localScale = Vector3.one;
    }

    // ── End / Skip ─────────────────────────────────────────────────────────────

    private void EndTutorial()
    {
        tutorialActive = false;
        tutorialCanvas.SetActive(false);
        PlayerPrefs.SetInt(PREF_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("Tutorial complete.");
    }

    private void SkipTutorial()
    {
        StopAllCoroutines();
        EndTutorial();
    }

    // ── Save Integration ───────────────────────────────────────────────────────

    /// <summary>
    /// Called by SaveManager to persist tutorial state across sessions.
    /// Returns true if the tutorial has been completed.
    /// </summary>
    public static bool IsTutorialComplete()
    {
        return PlayerPrefs.HasKey(PREF_KEY);
    }

    /// <summary>Call this to force the tutorial to show again (e.g. from settings).</summary>
    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(PREF_KEY);
        PlayerPrefs.Save();
    }
}