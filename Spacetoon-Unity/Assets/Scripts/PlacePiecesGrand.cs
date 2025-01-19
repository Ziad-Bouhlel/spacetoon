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
    [SerializeField] TextMeshProUGUI piecesRestantesText;
    [SerializeField] GameObject waiting;
    [SerializeField] private AudioSource placementAudioSource; 
    [SerializeField] private AudioSource ambientSound; // Référence à l'AudioSource

    public Sprite newSprite;
    [SerializeField] private GameObject spaceship; // Référence au GameObject du vaisseau
    [SerializeField] private GameObject Canva;

    // File d'attente pour les messages à traiter sur le thread principal
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    [SerializeField] private TextMeshProUGUI j1Text;
    [SerializeField] private TextMeshProUGUI j2Text;
    [SerializeField] private TextMeshProUGUI j3Text;
    [SerializeField] private TextMeshProUGUI j4Text;

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
        StringBuilder messageBuilder = new StringBuilder();
        
        while (isRunning)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(receivedData);
                
                // Traiter tous les messages complets
                string accumulated = messageBuilder.ToString();
                int startIdx = 0;
                int curlyCount = 0;
                
                // Parcourir les caractères pour identifier les messages JSON complets
                for (int i = 0; i < accumulated.Length; i++)
                {
                    if (accumulated[i] == '{')
                    {
                        if (curlyCount == 0)
                        {
                            startIdx = i;
                        }
                        curlyCount++;
                    }
                    else if (accumulated[i] == '}')
                    {
                        curlyCount--;
                        
                        // Message JSON complet trouvé
                        if (curlyCount == 0)
                        {
                            string completeMessage = accumulated.Substring(startIdx, i - startIdx + 1);
                            messageQueue.Enqueue(completeMessage);
                            Debug.Log("Message reçu du serveur : " + completeMessage);
                            
                            // Si c'est le dernier caractère, vider le builder
                            if (i == accumulated.Length - 1)
                            {
                                messageBuilder.Clear();
                            }
                        }
                    }
                }
                
                // Si il reste des données incomplètes, garder uniquement la partie non traitée
                if (messageBuilder.Length > 0 && curlyCount > 0)
                {
                    messageBuilder = new StringBuilder(accumulated.Substring(startIdx));
                }
                else if (curlyCount == 0)
                {
                    messageBuilder.Clear();
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

    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            Debug.Log(message);
            if (message.Contains("{\"start\":\"puzzle\""))
            {
               
                retrieveValueTimer(message);
                restartGame();
                waiting.SetActive(false);
                ambientSound.Play();
                timer.StartTimer();
                
            }
            if (message.Contains("{\"piece\":"))
            {
                StartCoroutine(HandlePlacementMessage(message));
            }
            if (message.Contains("Fin du jeu"))
            {
                   message = message.Replace("}","");
            message = message.Replace("{","");
                Debug.Log("Fin du jeu détectée !");
                timer.StopTimer(); // Arrête le timer
                ShowEndText(); // Affiche le texte de fin de jeu
            }
            if(message.Contains("Quitter")){
                   message = message.Replace("}","");
            message = message.Replace("{","");
                waiting.SetActive(true);
                ambientSound.Stop();
                endText.gameObject.SetActive(false);
                
            }
            if (message.Contains("piece posée"))
            {
                   message = message.Replace("}","");
            message = message.Replace("{","");
               StartCoroutine(HandleSpaceshipAnimation(message));
            }
            if (message.Contains("Light")){
                   message = message.Replace("}","");
            message = message.Replace("{","");
                changeSprite();
            }
            if(message.Contains("Joueur")){
                   message = message.Replace("}","");
            message = message.Replace("{","");
                nbPiecesJoueur(message);
            }
        }
    }

private void retrieveValueTimer(string message){
    Message json = JsonUtility.FromJson<Message>(message);
    timer.startTimeInSeconds = json.chrono;
}

private void nbPiecesJoueur(string message){
    if(message.Contains("Joueur 1")){
        j1Text.text = message;
    }else  if(message.Contains("Joueur 2")){
    j2Text.text = message;
    }else  if(message.Contains("Joueur 3")){
    j3Text.text = message;
    }else  if(message.Contains("Joueur 4")){
    j4Text.text = message;
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
          endText.gameObject.SetActive(true);
        
        endText.text = $"Fin de la partie ! Vous n'avez pas fini le puzzle";
       
      
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
    nbPiecesJoueur("Joueur 1  :  10/10");
    nbPiecesJoueur("Joueur 2  :  8/8");
    nbPiecesJoueur("Joueur 3  :  9/9");
    nbPiecesJoueur("Joueur 4  :  8/8");
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
    [System.Serializable]
public class Message
{
    public string start;
    public int chrono;
}
}
