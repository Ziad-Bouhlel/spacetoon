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
    public GameObject spaceshipChild;
    public GameObject Canva;

    // Variable pour suivre l'état de l'animation
    private bool isAnimating = false;

    void Start()
    {
        RightPosition = transform.position;
        Vector3[] possiblePositions =
        {
            new Vector3(980f, Random.Range(80f, 1000f), transform.position.z),
            new Vector3(Random.Range(90, 1835f), 533f, transform.position.z)
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
        if (spaceshipChild != null)
        {
            Debug.Log("Animation du vaisseau en cours...");
            GameObject newSpaceship = Instantiate(spaceship, transform.position, Quaternion.identity);
            Animator newAnimator = newSpaceship.transform.GetChild(0).GetComponent<Animator>();
            newSpaceship.transform.parent = Canva.transform;

            if (newAnimator != null)
            {
                newSpaceship.SetActive(true);

                SendAnimationStartMessage();

                newAnimator.SetTrigger("StartAnimation");
                Debug.Log("Animation du vaisseau déclenchée depuis la position de la pièce.");

                yield return new WaitForSeconds(3f);
                newAnimator.SetTrigger("StopAnim");
                newSpaceship.SetActive(false);

                Destroy(newSpaceship);
            }
        }
    }

    void SendAnimationStartMessage()
    {
        if (connexionServer != null)
        {
            connexionServer.SendMessageServer("piece posée " + gameObject.name);
            Debug.Log("Message envoyé : animation démarrée pour " + gameObject.name);
        }
        else
        {
            Debug.LogError("Connexion au serveur non définie.");
        }
    }
    public void RandomPlacementHorizontal(){
       transform.position = new Vector3(Random.Range(90, 1835f), 533f, transform.position.z);
    }



}

