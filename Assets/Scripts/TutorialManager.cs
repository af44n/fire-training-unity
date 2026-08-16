using UnityEngine;
using TMPro;

/// <summary>
/// TutorialManager drives all UI prompts and phase transitions.
/// The success screen is built fully in code so it always looks correct.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public enum TrainingPhase
    {
        Waiting,
        FireDetected,
        PickUpExtinguisher,
        ExtinguishFire,
        Success
    }

    [Header("UI References")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI phaseText;
    public GameObject      warningPanel;
    public GameObject      successPanel;   // kept for scene reference; replaced at runtime

    [Header("Waypoint")]
    public GameObject waypointArrow;

    [Header("Phase Tracking")]
    public TrainingPhase currentPhase = TrainingPhase.Waiting;

    // Built at runtime
    private GameObject runtimeSuccessOverlay;

    // ── Unity callbacks ──────────────────────────────────────────────

    void Start()
    {
        // Hide scene-saved success panel; we build our own at runtime
        if (successPanel != null) successPanel.SetActive(false);
        if (warningPanel != null) warningPanel.SetActive(false);
        if (waypointArrow != null) waypointArrow.SetActive(false);

        UpdateUI("Fire safety training starting in 5 seconds...");
        UpdatePhase("FIRE SAFETY TRAINING");
    }

    // ── Phase callbacks ──────────────────────────────────────────────

    /// <summary>Called by FireManager when the fire activates.</summary>
    public void OnFireStarted()
    {
        currentPhase = TrainingPhase.FireDetected;

        if (warningPanel != null) warningPanel.SetActive(true);
        if (waypointArrow != null) waypointArrow.SetActive(true);

        UpdatePhase("PHASE: EVACUATE");
        UpdateUI("FIRE DETECTED\nEvacuate to the adjacent room and locate the fire extinguisher.");
    }

    /// <summary>Called when the player enters the hallway trigger.</summary>
    public void OnPlayerEnteredHallway()
    {
        if (currentPhase == TrainingPhase.FireDetected)
        {
            currentPhase = TrainingPhase.PickUpExtinguisher;
            UpdatePhase("PHASE: PICK UP EXTINGUISHER");
            UpdateUI("Good. Now pick up the fire extinguisher.\nWalk up to it and press E.");
        }
    }

    /// <summary>Called when the extinguisher is grabbed.</summary>
    public void OnExtinguisherPickedUp()
    {
        if (currentPhase == TrainingPhase.PickUpExtinguisher ||
            currentPhase == TrainingPhase.FireDetected)
        {
            currentPhase = TrainingPhase.ExtinguishFire;
            if (waypointArrow != null) waypointArrow.SetActive(false);
            UpdatePhase("PHASE: EXTINGUISH FIRE");
            UpdateUI("Aim at the fire and hold G to spray CO2 foam.\nDon't stop until the fire is out.");
        }
    }

    /// <summary>Called by FireManager when fire health reaches 0.</summary>
    public void OnFireExtinguished()
    {
        currentPhase = TrainingPhase.Success;

        if (warningPanel != null) warningPanel.SetActive(false);
        if (successPanel != null) successPanel.SetActive(false);

        UpdatePhase("TRAINING COMPLETE");
        UpdateUI("");

        BuildSuccessScreen();
    }

    // ── Success screen (built entirely in code) ──────────────────────

    void BuildSuccessScreen()
    {
        // Find the existing screen-space canvas
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Root overlay (semi-dark, non-blocking)
        runtimeSuccessOverlay = new GameObject("SuccessOverlay");
        runtimeSuccessOverlay.transform.SetParent(canvas.transform, false);

        // Dark vignette behind the card
        var bg = new GameObject("SuccessBg");
        bg.transform.SetParent(runtimeSuccessOverlay.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.72f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // White card
        var card = new GameObject("SuccessCard");
        card.transform.SetParent(runtimeSuccessOverlay.transform, false);
        var cardImg = card.AddComponent<UnityEngine.UI.Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.25f, 0.3f);
        cardRT.anchorMax = new Vector2(0.75f, 0.7f);
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;

        // Green accent bar at top of card
        var bar = new GameObject("SuccessBar");
        bar.transform.SetParent(card.transform, false);
        var barImg = bar.AddComponent<UnityEngine.UI.Image>();
        barImg.color = new Color(0.18f, 0.72f, 0.25f, 1f);
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0.82f);
        barRT.anchorMax = new Vector2(1f, 1f);
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = Vector2.zero;

        // "TRAINING COMPLETE" label in the bar
        var headerGO = new GameObject("SuccessHeader");
        headerGO.transform.SetParent(bar.transform, false);
        var headerTxt = headerGO.AddComponent<TextMeshProUGUI>();
        headerTxt.text = "TRAINING COMPLETE";
        headerTxt.fontSize = 28;
        headerTxt.fontStyle = TMPro.FontStyles.Bold;
        headerTxt.color = Color.white;
        headerTxt.alignment = TMPro.TextAlignmentOptions.Center;
        var headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.anchorMin = Vector2.zero;
        headerRT.anchorMax = Vector2.one;
        headerRT.offsetMin = Vector2.zero;
        headerRT.offsetMax = Vector2.zero;

        // Body text
        var bodyGO = new GameObject("SuccessBody");
        bodyGO.transform.SetParent(card.transform, false);
        var bodyTxt = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyTxt.text = "Fire extinguished.\nYou passed the training.";
        bodyTxt.fontSize = 22;
        bodyTxt.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        bodyTxt.alignment = TMPro.TextAlignmentOptions.Center;
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.05f, 0.35f);
        bodyRT.anchorMax = new Vector2(0.95f, 0.80f);
        bodyRT.offsetMin = Vector2.zero;
        bodyRT.offsetMax = Vector2.zero;

        // Hint at the bottom
        var hintGO = new GameObject("SuccessHint");
        hintGO.transform.SetParent(card.transform, false);
        var hintTxt = hintGO.AddComponent<TextMeshProUGUI>();
        hintTxt.text = "Press Escape to exit play mode.";
        hintTxt.fontSize = 14;
        hintTxt.color = new Color(0.55f, 0.55f, 0.55f, 1f);
        hintTxt.alignment = TMPro.TextAlignmentOptions.Center;
        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.05f, 0.05f);
        hintRT.anchorMax = new Vector2(0.95f, 0.28f);
        hintRT.offsetMin = Vector2.zero;
        hintRT.offsetMax = Vector2.zero;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    void UpdateUI(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }

    void UpdatePhase(string phase)
    {
        if (phaseText != null)
            phaseText.text = phase;
    }
}
