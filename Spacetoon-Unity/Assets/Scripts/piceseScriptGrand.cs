using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class piceseScriptGrand : MonoBehaviour
{
    public Vector3 RightPosition;
    public Quaternion RightRotation;
    public bool InRightPosition;
    public bool Selected;
    

    public DragAndDropGrand connexionServer;
    void Start()
    {
        RightPosition = transform.position;
        Vector3[] possiblePositions =
        {
            new Vector3(1013f, Random.Range(342f, 695f), transform.position.z),
            new Vector3(Random.Range(657f, 1365f), 514f, transform.position.z)
        };

        transform.position = possiblePositions[Random.Range(0, possiblePositions.Length)];

        RightRotation = transform.rotation;
        // Établir une connexion avec le serveur
        
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, RightPosition) < 30f && transform.rotation == RightRotation)
        {
            if (!Selected)
            {
                if (InRightPosition == false)
                {
                    transform.position = RightPosition;
                    InRightPosition = true;
                    GetComponent<SortingGroup>().sortingOrder = 0;
                    Camera.main.GetComponent<DragAndDropGrand>().PlacedPieces++;
                   
                    // Envoyer un message au serveur
                    connexionServer.SendPlacementMessage(gameObject.name);
                    deleteCollider();
                }
            }
        }
    }

    public void deleteCollider()
    {
       BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
       if (boxCollider != null){
        boxCollider.enabled = false;
       Debug.Log(gameObject.name + " collider delete");
       }
    }


}
