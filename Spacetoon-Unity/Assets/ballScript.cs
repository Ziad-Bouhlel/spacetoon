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
            AudioManager.instance.PlayEffect(AudioManager.instance.goalSound); // Son de but
        }
        else if(other.tag == "blue")
         {
            redTxt.text = (++redScore).ToString();
            AudioManager.instance.PlayEffect(AudioManager.instance.goalSound); // Son de but
        }
        control.respawnBall();
        red.respawn();
        blue.respawn();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
    if (collision.collider.CompareTag("Player"))
    {
        AudioManager.instance.PlayEffect(AudioManager.instance.blopSound); // Son de collision
    }
    else
    {
        AudioManager.instance.PlayEffect(AudioManager.instance.respawnSound); // Son pour toucher le bord
    }
}
   
}
