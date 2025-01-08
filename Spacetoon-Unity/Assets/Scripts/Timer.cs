using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float startTimeInSeconds = 300f; // 5 minutes (300 secondes)
    private float remainingTime;
    private bool isRunning = false;

    void Start()
    {
        ResetTimer(); // Initialise le timer à la durée définie
    }

    void Update()
    {
        if (!isRunning) return;

        // Réduit le temps restant
        remainingTime -= Time.deltaTime;

        // Empêche le temps de descendre en dessous de 0
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            StopTimer();
            GameLost(); // Appelle la méthode lorsque le temps est écoulé
        }

        // Met à jour l'affichage
        UpdateTimerText();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        remainingTime = startTimeInSeconds;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void GameLost()
    {
      GameObject.Find("Main Camera").GetComponent<PlacePiecesGrand>().SendLostGame();
    }
}