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

    public GameObject spaceship;
    public Animator spaceshipAnimator;

    // Variable pour suivre l'état de l'animation
    private bool isAnimating = false;

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

        if (spaceship != null)
        {
            spaceship.SetActive(false); // Assurer que le vaisseau est invisible au départ
            if (spaceshipAnimator == null)
            {
                spaceshipAnimator = spaceship.GetComponent<Animator>();
                if (spaceshipAnimator == null)
                {
                    Debug.LogError("Aucun Animator trouvé sur le vaisseau.");
                }
            }
        }
        else
        {
            Debug.LogError("Le GameObject du vaisseau n'est pas assigné.");
        }

    }

    void Update()
    {
        if (Vector3.Distance(transform.position, RightPosition) < 30f && transform.rotation == RightRotation)
        {
            if (!Selected && !InRightPosition)
            {
                transform.position = RightPosition;
                InRightPosition = true;
                GetComponent<SortingGroup>().sortingOrder = 0;
                Camera.main.GetComponent<DragAndDropGrand>().PlacedPieces++;
        
                // Vérifier si une animation n'est pas déjà en cours
                if (!isAnimating)
                {
                    StartCoroutine(HandleSpaceshipAnimation());
                }

                 connexionServer.SendPlacementMessage(gameObject.name);
                    deleteCollider();
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




    IEnumerator HandleSpaceshipAnimation()
    {
        if (spaceshipAnimator != null)
        {
            isAnimating = true;

            spaceship.SetActive(true); // Rendre le vaisseau visible

            // Placer le vaisseau à la position de la pièce
            spaceship.transform.position = transform.position;

            // Déclencher l'animation
            spaceshipAnimator.SetTrigger("StartAnimation");
            Debug.Log("Animation du vaisseau déclenchée depuis la position de la pièce.");

            // attendre la fin de l'animation
            yield return new WaitForSeconds(3f);
            spaceshipAnimator.SetTrigger("StopAnim");
            spaceship.SetActive(false); // Rendre le vaisseau invisible après l'animation
            isAnimating = false;
        }

    }

    


}

