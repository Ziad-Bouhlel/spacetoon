using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Net.Sockets;
using System.Text;

public class piceseScript_ : MonoBehaviour
{
    public Vector3 RightPosition;
    public Quaternion RightRotation;
    public bool InRightPosition;
    public bool Selected;
    
    // Paramètres pour la connexion réseau
    private string serverIP = "127.0.0.1"; // Adresse IP du serveur
    private int serverPort = 5000;        // Port du serveur
    private TcpClient client;

    void Start()
    {
        RightPosition = transform.position;
        transform.position = new Vector3(Random.Range(0, 0), Random.Range(0, 0));
        RightRotation = transform.rotation;
        // Établir une connexion avec le serveur
       
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, RightPosition) < 50f && transform.rotation == RightRotation)
        {
            if (!Selected)
            {
                if (InRightPosition == false)
                {
                    transform.position = RightPosition;
                    InRightPosition = true;
                    GetComponent<SortingGroup>().sortingOrder = 0;
                    Camera.main.GetComponent<DragAndDrop_>().PlacedPieces++;

                    // Envoyer un message au serveur
                    SendPlacementMessage();
                }
            }
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
