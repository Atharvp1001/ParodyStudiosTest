using UnityEngine;
using TMPro;

public class CollectableCollector : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text collectText;   // "Collect 0/20"
    public TMP_Text timerText;     // "Timer: 120s"

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Game Settings")]
    public int totalCollectables = 20;
    public float startTime = 120f; // 120 seconds

    private int collectCount = 0;
    private float timer;
    private bool gameEnded = false;

    private void Start()
    {
        timer = startTime;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        if (gameEnded) return;

        // COUNTDOWN TIMER
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;

        UpdateUI();

        // Lose Condition by timer
        if (timer <= 0 && collectCount < totalCollectables)
        {
            LoseGame();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (gameEnded) return;

        // Check if object is on Collectable layer
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Collectable"))
        {
            collectCount++;
            Destroy(hit.collider.gameObject);

            UpdateUI();

            // Win Condition
            if (collectCount >= totalCollectables)
            {
                WinGame();
            }
        }
    }

    void UpdateUI()
    {
        if (collectText)
            collectText.text = "Collect: " + collectCount + "/" + totalCollectables;

        if (timerText)
            timerText.text = "Timer: " + Mathf.Ceil(timer) + "s";
    }

    void WinGame()
    {
        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        // 🔓 Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("YOU WIN!");
    }

    void LoseGame()
    {
        gameEnded = true;

        if (losePanel != null)
            losePanel.SetActive(true);

        // 🔓 Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("YOU LOSE!");
    }


    // PUBLIC method called by player script when they have been falling for too long
    public void OnPlayerFellTooLong()
    {
        if (gameEnded) return;
        Debug.Log("Player fell too long — triggering lose.");
        LoseGame();
    }
}
