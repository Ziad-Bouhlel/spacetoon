using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Net.Sockets;
using System.Text;

public class piceseScriptGrand : MonoBehaviour
{
    public Vector3 RightPosition;
    public Quaternion RightRotation;
    public bool InRightPosition;
    public bool Selected;

    public GameObject spaceship;
    public Animator spaceshipAnimator;

    private string serverIP = "127.0.0.1";
    private int serverPort = 5000;
    private TcpClient client;

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

        try
        {
            client = new TcpClient(serverIP, serverPort);
            Debug.Log("Connexion au serveur établie.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur de connexion au serveur : " + e.Message);
        }

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

                SendPlacementMessage();
            }
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

    void SendPlacementMessage()
    {
        if (client != null && client.Connected)
        {
            try
            {
                string message = "{\"piece\":\"" + gameObject.name + "\",\"status\":\"placed\"}";
                byte[] data = Encoding.UTF8.GetBytes(message);

                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Debug.Log("Message envoyé au serveur : " + message);
                BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
                boxCollider.enabled = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erreur lors de l'envoi du message : " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Connexion au serveur perdue.");
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Close();
            Debug.Log("Connexion au serveur fermée.");
        }
    }
}