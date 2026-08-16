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

    GameObject CreateUIObj(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        return go;
    }

    void BuildSuccessScreen()
    {
        // Find the existing screen-space canvas
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Root overlay (MUST be a RectTransform to fill canvas)
        runtimeSuccessOverlay = CreateUIObj("SuccessOverlay", canvas.transform);
        var overlayRT = runtimeSuccessOverlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        // Dark vignette behind the card
        var bg = CreateUIObj("SuccessBg", runtimeSuccessOverlay.transform);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.72f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // White card
        var card = CreateUIObj("SuccessCard", runtimeSuccessOverlay.transform);
        var cardImg = card.AddComponent<UnityEngine.UI.Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.25f, 0.25f);
        cardRT.anchorMax = new Vector2(0.75f, 0.75f);
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;

        // Green accent bar at top of card
        var bar = CreateUIObj("SuccessBar", card.transform);
        var barImg = bar.AddComponent<UnityEngine.UI.Image>();
        barImg.color = new Color(0.18f, 0.72f, 0.25f, 1f);
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0.82f);
        barRT.anchorMax = new Vector2(1f, 1f);
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = Vector2.zero;

        // "TRAINING COMPLETE" label in the bar
        var headerGO = CreateUIObj("SuccessHeader", bar.transform);
        var headerTxt = headerGO.AddComponent<TextMeshProUGUI>();
        headerTxt.text = "TRAINING COMPLETE";
        headerTxt.fontSize = 32;
        headerTxt.fontStyle = TMPro.FontStyles.Bold;
        headerTxt.color = Color.white;
        headerTxt.alignment = TMPro.TextAlignmentOptions.Center;
        headerTxt.enableWordWrapping = false;
        var headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.anchorMin = Vector2.zero;
        headerRT.anchorMax = Vector2.one;
        headerRT.offsetMin = Vector2.zero;
        headerRT.offsetMax = Vector2.zero;

        // Body text
        var bodyGO = CreateUIObj("SuccessBody", card.transform);
        var bodyTxt = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyTxt.text = "Fire extinguished.\nYou passed the training.";
        bodyTxt.fontSize = 26;
        bodyTxt.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        bodyTxt.alignment = TMPro.TextAlignmentOptions.Center;
        bodyTxt.enableWordWrapping = false;
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.05f, 0.45f);
        bodyRT.anchorMax = new Vector2(0.95f, 0.80f);
        bodyRT.offsetMin = Vector2.zero;
        bodyRT.offsetMax = Vector2.zero;

        // Retry Button
        var btnGO = CreateUIObj("RetryButton", card.transform);
        var btnImg = btnGO.AddComponent<UnityEngine.UI.Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        var btn = btnGO.AddComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.35f, 0.20f);
        btnRT.anchorMax = new Vector2(0.65f, 0.35f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        // Retry Button Text
        var btnTxtGO = CreateUIObj("RetryText", btnGO.transform);
        var btnTxt = btnTxtGO.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Retry Training";
        btnTxt.fontSize = 22;
        btnTxt.color = Color.white;
        btnTxt.alignment = TMPro.TextAlignmentOptions.Center;
        btnTxt.enableWordWrapping = false;
        var btnTxtRT = btnTxtGO.GetComponent<RectTransform>();
        btnTxtRT.anchorMin = Vector2.zero;
        btnTxtRT.anchorMax = Vector2.one;
        btnTxtRT.offsetMin = Vector2.zero;
        btnTxtRT.offsetMax = Vector2.zero;

        // Hint at the bottom
        var hintGO = CreateUIObj("SuccessHint", card.transform);
        var hintTxt = hintGO.AddComponent<TextMeshProUGUI>();
        hintTxt.text = "Press Escape to unlock cursor.";
        hintTxt.fontSize = 16;
        hintTxt.color = new Color(0.55f, 0.55f, 0.55f, 1f);
        hintTxt.alignment = TMPro.TextAlignmentOptions.Center;
        hintTxt.enableWordWrapping = false;
        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.05f, 0.05f);
        hintRT.anchorMax = new Vector2(0.95f, 0.15f);
        hintRT.offsetMin = Vector2.zero;
        hintRT.offsetMax = Vector2.zero;
        
        // Unlock cursor for clicking the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
