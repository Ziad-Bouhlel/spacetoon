using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDisplay : MonoBehaviour
{
  
    public Vector3 RightPosition;
    public Quaternion RightRotation;
     public bool InRightPosition;
    void Start()
    {
        RightPosition = transform.position;
        RightRotation = transform.rotation;
        transform.position = new Vector3(Random.Range(400f, 400f), Random.Range(600f, 600f));
        InRightPosition = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
