using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ballScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public TextMeshProUGUI redTxt, blueTxt;
    private int redScore, blueScore;

    public Control control;
    public Player red,blue;

    public int RedScore => redScore; // Propriété pour accéder au score de l'équipe rouge
    public int BlueScore => blueScore; // Propriété pour accéder au score de l'équipe bleue


    public AudioClip blopSound; // Son pour collision
    public AudioSource audioSource; // Composant AudioSource

    public static bool wasGoal {  get; private set; }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        redScore = 0;
        blueScore = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "red")
         {
            blueTxt.text = (++blueScore).ToString();
            NotifyGoalToServer("blue");
        }
        else if(other.tag == "blue")
         {
            redTxt.text = (++redScore).ToString();
            NotifyGoalToServer("red");
        }

          // Jouer le son de but
        if (audioSource != null && control.winSound != null)
        {
            audioSource.PlayOneShot(control.winSound);
        }

        control.respawnBall();
        red.respawn();
        blue.respawn();
    }

    private void NotifyGoalToServer(string team)
    {
        string message = $"IDENTITY:hockeyJeu|GOAL:{team}";
        Debug.Log($"[NotifyGoalToServer] Message préparé : {message}");
        // Envoyez le message via un TCPClient
        TCPClient client = FindObjectOfType<TCPClient>();

        if (client != null)
        {
            client.SendMessage(message); // Appel à la méthode existante
            Debug.Log($"[NotifyGoalToServer] Message envoyé au serveur : {message}");
        }
        else
        {
            Debug.LogError("TCPClient introuvable !");
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
    if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Wall"))
        {
            // Jouer le son de collision
            if (audioSource != null && blopSound != null)
            {
                audioSource.PlayOneShot(blopSound);
            }
        }
    
    }
   
}
