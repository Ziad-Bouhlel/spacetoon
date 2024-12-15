using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector2 respawnPos;
    void Start()
    {
        respawnPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void respawnBall()
    {
        transform.position = respawnPos;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
    }
}
