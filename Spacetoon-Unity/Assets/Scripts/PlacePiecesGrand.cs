using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using TMPro;

public class PlacePiecesGrand : MonoBehaviour
{
    public string serverIP = "192.168.49.1"; // Adresse IP du serveur
    private int serverPort = 5000;        // Port du serveur
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;
    [SerializeField] private Timer timer; // Référence au script Timer
    [SerializeField] TextMeshProUGUI endText;
    [SerializeField] TextMeshProUGUI waitingText;
    [SerializeField] TextMeshProUGUI piecesRestantesText;
    [SerializeField] GameObject fondWaiting;
    [SerializeField] GameObject spacetoonWaiting;
    [SerializeField] private AudioSource placementAudioSource; // Référence à l'AudioSource
    public Sprite newSprite;
    [SerializeField] private GameObject spaceship; // Référence au GameObject du vaisseau
    [SerializeField] private GameObject Canva;

    // File d'attente pour les messages à traiter sur le thread principal
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    void Start()
    {
        endText.gameObject.SetActive(false);
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isRunning = true;
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
            Debug.Log("Connecté au serveur.");
            SendMessageServer("puzzleEcran");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur de connexion au serveur : " + e.Message);
        }
        timer.StartTimer();
        spaceship.SetActive(false); // Assure que le vaisseau est invisible au départ
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

                    // Ajouter le message à la file d'attente
                    messageQueue.Enqueue(message);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de la réception des messages : " + e.Message);
            isRunning = false;
        }
    }

    void Update()
    {
        // Traiter les messages en file d'attente
        while (messageQueue.TryDequeue(out string message))
        {
            if (message.Contains("{\"start\":\"puzzle\""))
            {
                restartGame();
                waitingText.gameObject.SetActive(false);
                fondWaiting.SetActive(false);
                spacetoonWaiting.SetActive(false);
                timer.StartTimer();
                
            }
            if (message.Contains("{\"piece\":"))
            {
                StartCoroutine(HandlePlacementMessage(message));
            }
            if (message.Contains("Fin du jeu"))
            {
                Debug.Log("Fin du jeu détectée !");
                timer.StopTimer(); // Arrête le timer
                ShowEndText(); // Affiche le texte de fin de jeu
            }
            if(message.Contains("Quitter")){
                waitingText.gameObject.SetActive(true);
                fondWaiting.SetActive(true);
                spacetoonWaiting.SetActive(true);
                timer.ResetTimer();
                endText.gameObject.SetActive(false);
                
            }
            if (message.Contains("piece posée"))
            {
               StartCoroutine(HandleSpaceshipAnimation(message));
            }
            if (message.Contains("Light")){
                changeSprite();
            }
        }
    }

    IEnumerator HandleSpaceshipAnimation(string message)
    {
        yield return new WaitForSeconds(3f);
        string[] words = message.Split('_');
        string pieceNumber = words[words.Length - 1];
        Debug.Log(pieceNumber);

        GameObject targetPiece = GameObject.Find("piece_" + pieceNumber);

        if (targetPiece != null)
        {
            Vector3 targetPosition = targetPiece.transform.position;
            Debug.Log(targetPosition);

            GameObject newSpaceship = Instantiate(spaceship, transform.position, Quaternion.identity);
            newSpaceship.transform.parent = Canva.transform;

            newSpaceship.SetActive(true);
            float duration = 2f;
            float elapsedTime = 0f;
            Vector3 startPosition = new Vector3(targetPosition.x, -644, targetPosition.z);

            float oscillationMagnitude = 5f;
            float oscillationFrequency = 15f;
            float rotationMagnitude = 13f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;

                Vector3 linearPosition = Vector3.Lerp(startPosition, targetPosition, progress);

                float oscillation = Mathf.Sin(elapsedTime * oscillationFrequency) * oscillationMagnitude;
                linearPosition.x += oscillation;

                newSpaceship.transform.position = linearPosition;

                float rotationOscillation = Mathf.Sin(elapsedTime * oscillationFrequency) * rotationMagnitude;
                newSpaceship.transform.rotation = Quaternion.Euler(0, 0, rotationOscillation);

                yield return null;
            }

            newSpaceship.transform.position = targetPosition;
            newSpaceship.transform.rotation = Quaternion.identity;
            yield return new WaitForSeconds(0.2f);
            newSpaceship.SetActive(false);
            Destroy(newSpaceship);
        }
        else
        {
            Debug.LogWarning($"Could not find piece_{pieceNumber} in the scene");
        }
    }


    IEnumerator HandlePlacementMessage(string message)
    {
        yield return new WaitForSeconds(5f);
        try
        {
          
            var json = JsonUtility.FromJson<PieceMessage>(message);
     
            GameObject piece = GameObject.Find(json.piece);
            
      
            if (piece != null)
            {
                piece.GetComponent<Solution>().setFound(true);
                Transform firstChild = piece.transform.GetChild(0);
                SpriteRenderer spriteRenderer = firstChild.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                spriteRenderer.sprite = newSprite;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Blanc opaque
               /* Debug.Log(json.piece +"White");
                GameObject pieceWhite = GameObject.Find(json.piece +"White");
                pieceWhite.SetActive(true);*/

                  if (placementAudioSource != null)
                {
                    placementAudioSource.Play();
                }
                else
                {
                    Debug.LogWarning("AudioSource non assignée pour le placement des pièces.");
                }
                }
              //  piece.transform.position = piece.GetComponent<RandomDisplay>().RightPosition;
               // piece.GetComponent<RandomDisplay>().InRightPosition = true;
                Debug.Log($"Pièce {json.piece} placée à sa position correcte.");

                updatePiecesRestantes();
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
      void SendMessageServer(string message)
    {
        if (client != null && client.Connected)
        {
            try
            {
                // Construire un message JSON
               
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
    void updatePiecesRestantes(){
        int nbPiece = 0;
        GameObject tmpPiece ;
        for (int i = 0; i < 35; i++)
            {
            
                 tmpPiece =GameObject.Find("piece_" + i + "");
                 if(tmpPiece.GetComponent<Solution>().isFound())
                    nbPiece++;
                
            }
        piecesRestantesText.text = "Nombre de pièces restantes : " + (35-nbPiece) + "";
    }
    public void changeSprite(){
        StartCoroutine(ChangeSpriteTime());
    }
    private IEnumerator ChangeSpriteTime(){
    Sprite oldSprite =   GameObject.Find("oldSprite").transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
    GameObject tmpPiece ;
            for (int i = 0; i < 35; i++)
            {
            
                 tmpPiece =GameObject.Find("piece_" + i + "");
                 if(!tmpPiece.GetComponent<Solution>().isFound())
                    tmpPiece.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newSprite ;
                
            }
             yield return new WaitForSeconds(5f);

                for (int i = 0; i < 35; i++)
            {
            
               tmpPiece =GameObject.Find("piece_" + i + "");
                 if(!tmpPiece.GetComponent<Solution>().isFound())
                    tmpPiece.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = oldSprite ;  
            }

    }
    void ShowEndText()
    {
        // Met à jour le texte de fin de jeu
        endText.text = $"Fin de la partie ! Vous avez fini le puzzle, il vous restait";
        // Affiche le texte
        endText.gameObject.SetActive(true);
    }

        void ShowLostText()
    {
        // Met à jour le texte de fin de jeu
        endText.text = $"Fin de la partie ! Vous n'avez pas fini le puzzle";
        // Affiche le texte
        endText.gameObject.SetActive(true);
    }

      public void SendLostGame()
    {
        if (client != null && client.Connected)
        {
            try
            {
                // Construire un message JSON
                string message = "Puzzle lost";
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
           ShowLostText();
    }

    private void restartGame(){
  Sprite oldSprite =   GameObject.Find("oldSprite").transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
      GameObject tmpPiece ;
    for (int i = 0; i < 35; i++)
                {  
                tmpPiece =GameObject.Find("piece_" + i + "");
                tmpPiece.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = oldSprite ;
                tmpPiece.GetComponent<Solution>().setFound(false);  
            
    }
    timer.ResetTimer();
    updatePiecesRestantes();
}

    void OnDestroy()
    {
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
