using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PlacePieces : MonoBehaviour
{
    private string serverIP = "127.0.0.1"; // Adresse IP du serveur
    private int serverPort = 5000;        // Port du serveur
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;

    void Start()
    {
        // Établir une connexion avec le serveur
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isRunning = true;
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
            Debug.Log("Connecté au serveur.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur de connexion au serveur : " + e.Message);
        }
    }

    void ReceiveMessages()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (isRunning)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Message reçu du serveur : " + message);

                    if (message.Contains("{\"piece\":"))
                    {
                        HandlePlacementMessage(message);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de la réception des messages : " + e.Message);
            isRunning = false;
        }
    }

    void HandlePlacementMessage(string message)
    {
        try
        {
            // Extraire les informations du JSON
            var json = JsonUtility.FromJson<PieceMessage>(message);

            // Trouver la pièce correspondante dans la scène
            GameObject piece = GameObject.Find(json.piece);
            if (piece != null)
            {
                // Déplacer la pièce à sa position correcte
                piece.transform.position = piece.GetComponent<piceseScript>().RightPosition;
                piece.GetComponent<piceseScript>().InRightPosition = true;
                Debug.Log($"Pièce {json.piece} placée à sa position correcte.");
            }
            else
            {
                Debug.LogWarning($"Pièce {json.piece} introuvable dans la scène.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors du traitement du message : " + e.Message);
        }
    }

    void OnDestroy()
    {
        // Arrêter la réception et fermer la connexion
        isRunning = false;
        if (receiveThread != null) receiveThread.Abort();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("Connexion au serveur fermée.");
    }

    [System.Serializable]
    private class PieceMessage
    {
        public string piece;
        public string status;
    }
}
