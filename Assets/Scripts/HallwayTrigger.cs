using UnityEngine;

/// <summary>
/// HallwayTrigger detects when the player XR Rig enters the hallway/adjacent room.
/// Attach this to a trigger collider GameObject in the hallway.
/// </summary>
public class HallwayTrigger : MonoBehaviour
{
    private TutorialManager tutorialManager;
    private bool triggered = false;

    void Start()
    {
        tutorialManager = Object.FindAnyObjectByType<TutorialManager>();

        // Make sure collider is a trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // Check for Player tag — the XR Origin root is tagged "Player"
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Camera Offset") ||
            other.gameObject.name.Contains("XR Origin") || other.gameObject.name.Contains("Main Camera"))
        {
            triggered = true;
            if (tutorialManager != null)
                tutorialManager.OnPlayerEnteredHallway();

            Debug.Log("[HallwayTrigger] Player entered hallway!");
        }
    }
}
