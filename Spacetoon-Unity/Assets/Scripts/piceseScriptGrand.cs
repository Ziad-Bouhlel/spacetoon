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

    // Références pour l'animation du vaisseau
    public GameObject spaceship; // Vaisseau à déplacer
    private Animator spaceshipAnimator;

    // Paramètres pour la connexion réseau
    private string serverIP = "127.0.0.1"; // Adresse IP du serveur
    private int serverPort = 5000;        // Port du serveur
    private TcpClient client;

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
        try
        {
            client = new TcpClient(serverIP, serverPort);
            Debug.Log("Connexion au serveur établie.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur de connexion au serveur : " + e.Message);
        }

        // Initialisation de l'Animator du vaisseau
        if (spaceship != null)
        {
            spaceshipAnimator = spaceship.GetComponent<Animator>();
            if (spaceshipAnimator == null)
            {
                Debug.LogError("Aucun Animator trouvé sur le vaisseau.");
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
            if (!Selected)
            {
                if (InRightPosition == false)
                {
                    transform.position = RightPosition;
                    InRightPosition = true;
                    GetComponent<SortingGroup>().sortingOrder = 0;
                    Camera.main.GetComponent<DragAndDropGrand>().PlacedPieces++;

                    // Lancer l'animation du vaisseau
                    StartSpaceshipAnimation();

                    // Envoyer un message au serveur
                    SendPlacementMessage();
                }
            }
        }
    }

    void StartSpaceshipAnimation()
    {
        if (spaceshipAnimator != null)
        {
            // Placer le vaisseau à la position de la pièce
            spaceship.transform.position = transform.position;

            // Déclencher l'animation
            spaceshipAnimator.SetTrigger("Launch");
            Debug.Log("Animation du vaisseau déclenchée depuis la position de la pièce.");
        }
    }

    void SendPlacementMessage()
    {
        if (client != null && client.Connected)
        {
            try
            {
                // Construire un message JSON
                string message = "{\"piece\":\"" + gameObject.name + "\",\"status\":\"placed\"}";
                byte[] data = Encoding.UTF8.GetBytes(message);

                // Envoyer les données au serveur
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Debug.Log("Message envoyé au serveur : " + message);
                BoxCollider boxCollider = GetComponent<BoxCollider>();
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
        // Fermer la connexion au serveur lors de la destruction de l'objet
        if (client != null)
        {
            client.Close();
            Debug.Log("Connexion au serveur fermée.");
        }
    }
}
