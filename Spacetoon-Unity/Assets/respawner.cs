using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Control : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector2 respawnPos;
    public TextMeshProUGUI timerText; // Texte pour afficher le timer
    public TextMeshProUGUI winnerText; // Texte pour afficher le gagnant

    public AudioClip blopSound; // Son pour collision avec un bord
    public AudioClip winSound; // Son pour quand un joueur marque un but
    public AudioSource audioSource; // Composant AudioSource
    public AudioSource backgroundMusic; // Musique d'ambiance

    private float gameTime = 120f; // 2 minutes en secondes
    private bool isGameOver = false;

    public ballScript ballScript;

    void Start()
    {
        respawnPos = transform.position;

        if (timerText != null)
        {
            timerText.text = "Timer initialized!"; 
            timerText.text = FormatTime(gameTime);
        }
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(false); // Cache le texte gagnant au début
        }
        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true; // Musique d'ambiance en boucle
            backgroundMusic.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver)
        {
            gameTime -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = FormatTime(gameTime);
            }

            if (gameTime <= 0)
            {
                EndGame();
            }
        }
        
    }

    public void respawnBall()
    {
        transform.position = respawnPos;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
    }

     private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        isGameOver = true;
        if (ballScript != null)
        {
            int redScore = ballScript.RedScore; // Obtenez les scores depuis ballScript
            int blueScore = ballScript.BlueScore;


            string winner = redScore > blueScore
                ? "Red Team Wins!"
                : blueScore > redScore
                ? "Blue Team Wins!"
                : "It's a Draw!";
            
            if (winnerText != null)


            {
                winnerText.text = winner;
                winnerText.gameObject.SetActive(true); // Affiche le texte du gagnant
            }
             if (audioSource != null && winSound != null)
            {
            audioSource.PlayOneShot(winSound);
            }
        }
                 
    }
}

