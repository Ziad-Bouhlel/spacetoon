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
        
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        DisableGameplay();

        string winner = DetermineWinner();

         if (winnerText != null)
        {
            StartCoroutine(ShowWinner(winner));
        }

        // Joue le son de fin de jeu
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
    }


    private void DisableGameplay()
    {
        // Désactive les mouvements des joueurs
        if (redPlayer != null)
        {
            redPlayer.enabled = false;
        }
        if (bluePlayer != null)
        {
            bluePlayer.enabled = false;
        }

        // Désactive la balle
        if (ball != null)
        {
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.velocity = Vector2.zero;
                ballRb.isKinematic = true;
            }
        }
    }



    private string DetermineWinner()
    {
        if (ballScript != null)
        {
            int redScore = ballScript.RedScore; // Obtenez les scores depuis ballScript
            int blueScore = ballScript.BlueScore;

            if (redScore > blueScore)
            {
                return "Red Team Wins!";
            }
            else if (blueScore > redScore)
            {
                return "Blue Team Wins!";
            }
            else
            {
                return "It's a Draw!";
            }
        }
        return "No winner!";
    }


    private IEnumerator ShowWinner(string winner)
    {
        winnerText.text = winner;
        winnerText.gameObject.SetActive(true);

        // Animation simple pour afficher le texte
        float scale = 0.5f;
        while (scale < 1f)
        {
            scale += Time.deltaTime * 2f; // Augmente la taille progressivement
            winnerText.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        winnerText.transform.localScale = Vector3.one;
    }


}
