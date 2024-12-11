using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    Rigidbody2D rb;
    Vector2 playerSize;
    private Vector2 mousePos;
    public Boolean middle;
    private Vector2 standartPosition;
    float horizontal, vertical;

    private void Start()
    {
        playerSize = gameObject.GetComponent<SpriteRenderer>().bounds.extents;
        rb = GetComponent<Rigidbody2D>();
        standartPosition = rb.position;
    }

    void Update()
    {
        rb.velocity = new Vector2(0, 0);

        if (Input.GetMouseButton(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            if ((mousePos.x > transform.position.x - playerSize.x &&
                mousePos.x < transform.position.x + playerSize.x) &&
                (mousePos.y > transform.position.y - playerSize.y &&
                mousePos.y < transform.position.y + playerSize.y))
            {
                if (!middle)
                {
                    if (mousePos.x < -20.2)
                    {
                        rb.MovePosition(mousePos);

                    }
                }
                else
                {
                    if (mousePos.x > -19.45)
                    {
                        rb.MovePosition(mousePos);
                    }
                }
            }
        }
        if (!middle)
        {
            if (rb.position.x < -25.3 || rb.position.x > -19.85)
            {
                respawn();
            }
        }
        else
        {
            if (rb.position.x > -14.79 || rb.position.x < -19.80)
            {
                respawn();
            }
        }

    }


    public void respawn()
    {
        rb.MovePosition(standartPosition);
    }

}
