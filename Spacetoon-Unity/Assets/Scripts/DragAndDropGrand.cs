using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine.UI;
using System.Linq;

public class DragAndDropGrand : MonoBehaviour
{
    public Sprite[] Levels;
    public GameObject EndMenu;
    public int PlacedPieces = 0;
    public GameObject SelectedPiece;
    public Button[] buttons; // Les 4 boutons passés en paramètre
    public Sprite newImage;  // L'image à appliquer temporairement
    public Sprite originalImage; // L'image d'origine

    private Dictionary<int, GameObject> activeTouches = new Dictionary<int, GameObject>();
    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();
    private int OIL = 1;

    // Variables pour détecter le double-clic/double-tap
    private float doubleClickTime = 0.3f; // Temps maximum entre deux clics/taps
    private float lastClickTime = 0f; // Temps du dernier clic/tap
    private GameObject lastClickedObject = null; // Dernier objet cliqué/tapé
   public string serverIP = "192.168.49.1"; // Adresse IP du serveur
    private int serverPort = 5000;        // Port du serveur
    private TcpClient client;

    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private bool finPartie= false;

    public Button targetImage;

    public string identityName;

    public List<CheckPiecesScript> squares;
    void Start()
    {
       
    
          try
        {
        client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isRunning = true;
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
            Debug.Log("Connecté au serveur.");
            SendMessageServer(identityName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur de connexion au serveur : " + e.Message);
        }
        if (targetImage == null)
        {
            Debug.LogError("targetImage est null dans Start");
        }
        else
        {
           // Debug.Log("targetImage est bien assigné dans Start");
            //targetImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();
       
       
        while (messageQueue.TryDequeue(out string message))
        {
            if (message.Contains("Puzzle lost"))
            {
               ShowTargetImage(); 
                 for (int i = 0; i < 35; i++)
                {
                    Debug.Log(i);
                    GameObject.Find("piece_" + i + "").GetComponent<piceseScriptGrand>().deleteCollider();
            
                }
                foreach(CheckPiecesScript c in squares){
                    
                    if(!c.AllObjectsInRightPosition()){
                        c.Lost();
                    }
                }
            }
        
        }

         if (PlacedPieces == 35 && !finPartie)
        {
            //ShowTargetImage();
           SendMessageServer("Fin du jeu");
           finPartie = true;
           
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

   public void ShowTargetImage()
    {
            targetImage.gameObject.SetActive(true); 
    }

  public void SendPlacementMessage(string name)
    {
        if (client != null && client.Connected)
        {
            try
            {
                // Construire un message JSON
                string message = "{\"piece\":\"" + name + "\",\"status\":\"placed\"}";
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

      public void SendMessageServer(string message)
    {
        if (client != null && client.Connected)
        {
            try
            {
                // Construire un message JSON
               
                byte[] data = Encoding.UTF8.GetBytes(message );

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

    private GameObject GetTopMostPieceAtPosition(Vector2 position)
    {
        // Récupérer toutes les pièces à cette position
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);

        // Filtrer pour ne garder que les pièces de puzzle
        var puzzlePieces = hits
            .Where(hit => hit.collider != null &&
                   hit.transform.CompareTag("Puzzle"))
            .Select(hit => hit.transform.gameObject)
            .ToList();

        if (puzzlePieces.Count == 0)
            return null;

        // Retourner la pièce avec le plus grand sortingOrder
        return puzzlePieces
            .OrderByDescending(piece => piece.GetComponent<SortingGroup>().sortingOrder)
            .FirstOrDefault();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject topPiece = GetTopMostPieceAtPosition(mousePosition);

            if (topPiece != null && !topPiece.GetComponent<piceseScriptGrand>().InRightPosition)
            {
                // Détection du double-clic
                if (topPiece == lastClickedObject && Time.time - lastClickTime < doubleClickTime)
                {
                    // Votre logique de double-clic ici
                }
                else
                {
                    lastClickTime = Time.time;
                    lastClickedObject = topPiece;

                    // Sélection pour drag & drop
                    SelectedPiece = topPiece;
                    SelectedPiece.GetComponent<piceseScriptGrand>().Selected = true;
                    SelectedPiece.GetComponent<SortingGroup>().sortingOrder = OIL;
                    OIL++;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (SelectedPiece != null)
            {
                SelectedPiece.GetComponent<piceseScriptGrand>().Selected = false;
                SelectedPiece = null;
            }
        }

        if (SelectedPiece != null)
        {
            Vector3 MousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SelectedPiece.transform.position = new Vector3(MousePoint.x, MousePoint.y, 0);
        }
    }

    private void HandleTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    GameObject topPiece = GetTopMostPieceAtPosition(touchPosition);

                    if (topPiece != null)
                    {
                        // Détection du double-tap
                        if (topPiece == lastClickedObject && Time.time - lastClickTime < doubleClickTime &&
                            !topPiece.GetComponent<piceseScriptGrand>().InRightPosition)
                        {
                            // Votre logique de double-tap ici
                        }
                        else
                        {
                            lastClickTime = Time.time;
                            lastClickedObject = topPiece;

                            // Sélection pour drag & drop
                            if (!topPiece.GetComponent<piceseScriptGrand>().InRightPosition &&
                                !activeTouches.ContainsKey(touch.fingerId))
                            {
                                activeTouches[touch.fingerId] = topPiece;
                                topPiece.GetComponent<piceseScriptGrand>().Selected = true;
                                topPiece.GetComponent<SortingGroup>().sortingOrder = OIL;
                                OIL++;
                            }
                        }
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (activeTouches.ContainsKey(touch.fingerId))
                    {
                        GameObject piece = activeTouches[touch.fingerId];
                        piece.transform.position = touchPosition;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (activeTouches.ContainsKey(touch.fingerId))
                    {
                        GameObject piece = activeTouches[touch.fingerId];
                        piece.GetComponent<piceseScriptGrand>().Selected = false;
                        activeTouches.Remove(touch.fingerId);
                    }
                    break;
            }
        }
    }

    private void RotatePiece(GameObject piece)
    {
        piece.transform.Rotate(0, 0, 90);
    }



    public void BacktoMenu()
    {
        SceneManager.LoadScene("menuDuJeu");
         if (client != null && client.Connected)
            {
                try
                {
                    // Construire un message JSON
                    string message = "{Quitter}";
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

    public void ActiveColor()
    {
        SendMessageServer("Light");
        if (buttons.Length != 4 || newImage == null || originalImage == null)
        {
            Debug.LogError("Les boutons et les images doivent être correctement assignés.");
            return;
        }

        foreach (Button button in buttons)
        {
            button.image.sprite = newImage;
        }

        StartCoroutine(RestoreOriginalImages());
    }

    private IEnumerator RestoreOriginalImages()
    {
        yield return new WaitForSeconds(5f);

        foreach (Button button in buttons)
        {
            button.image.sprite = originalImage;
        }
    }
 
}
