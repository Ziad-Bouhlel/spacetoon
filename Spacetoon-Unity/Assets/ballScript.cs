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
        }
        else if(other.tag == "blue")
         {
            redTxt.text = (++redScore).ToString();
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
