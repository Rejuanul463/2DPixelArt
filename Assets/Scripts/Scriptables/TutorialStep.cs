using UnityEngine;

/// <summary>
/// Defines a single tutorial instruction step.
/// Assign these as a list on TutorialManager in the Inspector.
/// </summary>
[System.Serializable]
public class TutorialStep
{
    [Header("Instruction")]
    [Tooltip("Short headline shown in bold at the top of the bubble.")]
    public string title;

    [Tooltip("Full instruction text shown to the player.")]
    [TextArea(2, 5)]
    public string message;

    [Header("Highlight Target (optional)")]
    [Tooltip("The UI element to highlight with the spotlight. Leave null to skip spotlight.")]
    public RectTransform highlightTarget;

    [Tooltip("Padding around the highlighted element in pixels.")]
    public float highlightPadding = 20f;

    [Header("Behaviour")]
    [Tooltip("If true the player must tap the highlighted target to advance (not the Next button).")]
    public bool requireTargetTap = false;

    [Tooltip("If true the Next button is hidden and this step waits for requireTargetTap or AdvanceFromCode().")]
    public bool waitForAction = false;
}