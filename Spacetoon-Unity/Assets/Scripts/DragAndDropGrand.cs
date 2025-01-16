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
public class DragAndDropGrand : MonoBehaviour
{
    public Sprite[] Levels;
    public GameObject EndMenu;
    public int PlacedPieces = 0;
    public GameObject SelectedPiece;

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
            targetImage.gameObject.SetActive(false);
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
            ShowTargetImage();
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
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.transform != null && hit.transform.CompareTag("Puzzle") )
            {
                GameObject piece = hit.transform.gameObject;
               
                // Détection du double-clic
                if (piece == lastClickedObject && Time.time - lastClickTime < doubleClickTime && !piece.GetComponent<piceseScriptGrand>().InRightPosition)
                {
                    //RotatePiece(piece);
                    //lastClickedObject = null; 
                }
                else
                {
                    lastClickTime = Time.time;
                    lastClickedObject = piece;

                    // Sélection pour drag & drop
                    if (!piece.GetComponent<piceseScriptGrand>().InRightPosition)
                    {
                      
                        SelectedPiece = piece;
                        SelectedPiece.GetComponent<piceseScriptGrand>().Selected = true;
                        SelectedPiece.GetComponent<SortingGroup>().sortingOrder = OIL;
                        OIL++;
                    }
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
                    RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
                    if (hit.transform != null && hit.transform.CompareTag("Puzzle"))
                    {
                        GameObject piece = hit.transform.gameObject;

                        // Détection du double-tap
                        if (piece == lastClickedObject && Time.time - lastClickTime < doubleClickTime && !piece.GetComponent<piceseScriptGrand>().InRightPosition)
                        {
                            //RotatePiece(piece);
                            //lastClickedObject = null; // Réinitialiser après le double-tap
                        }
                        else
                        {
                            lastClickTime = Time.time;
                            lastClickedObject = piece;

                            // Sélection pour drag & drop
                            if (!piece.GetComponent<piceseScriptGrand>().InRightPosition && !activeTouches.ContainsKey(touch.fingerId))
                            {
                                activeTouches[touch.fingerId] = piece;
                                piece.GetComponent<piceseScriptGrand>().Selected = true;
                                piece.GetComponent<SortingGroup>().sortingOrder = OIL;
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

    public void ActiveColor(){
        SendMessageServer("Light");
    }

 
}
