using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;



public class TCPClient : MonoBehaviour
{
    [System.Serializable]
    public class SensorReading
    {
        public long timestamp;
        public float[] value;
    }

    [System.Serializable]
    public class ComplexSensorData
    {
        public SensorReading accelerometer;
        public SensorReading linearAcceleration;
    }

    public TextMeshProUGUI jsonText;
    public GameObject puck;
    public float speedFactor = 20f; 

    private TcpClient tcpClient;
    private NetworkStream stream;
    public string serverIP = "172.20.10.5";
    public int port = 5000;               
    private Thread receiveThread;

    private Vector2 screenBounds;

    private float previousTime;
    private Vector2 velocity = Vector2.zero;
    private Vector2 position;
    private Vector2 StandartPos;


    public SpriteRenderer p;
    Vector2 playerSize;


    void Start()
    {
        if (jsonText == null)
        {
            print("Veuillez assigner une référence TextMeshPro dans l'inspecteur !");
            return;
        }
        
        ConnectToServer();
        previousTime = Time.time;
        position = new Vector2(puck.transform.position.x, puck.transform.position.y);
        StandartPos = position;
        playerSize = p.bounds.extents;
        
    }

    private void Update()
    {
        CalculateScreenBounds();

    }

    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient(serverIP, port);
            stream = tcpClient.GetStream();
            print("Connecté au serveur TCP");
            UpdateUIText("Connexion");
            UpdateUIText(screenBounds.x + "/" + screenBounds.y);
            UpdateUIText(Camera.main.orthographicSize.ToString());


            SendMessage("Connecté depuis Unity");

            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (SocketException ex)
        {
            print($"Erreur de connexion TCP : {ex.Message}");
            UpdateUIText($"Erreur de connexion TCP : {ex.Message}");

        }
    }

    void SendMessage(string message)
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            print("Impossible d'envoyer un message, pas de connexion au serveur.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(data, 0, data.Length);
            print($"Message envoyé : {message}");

        }
        catch (System.Exception ex)
        {
            print($"Erreur lors de l'envoi du message : {ex.Message}");
        }
    }

    void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (tcpClient != null && tcpClient.Connected)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    print($"Message reçu : {message}");

                    ProcessSensorData(message);
                    
                }
            }
            catch (System.Exception ex)
            {
                print($"Erreur lors de la réception des données : {ex.Message}");
                break;
            }
        }
    }

    void ProcessSensorData(string jsonData)
    {
        try
        {
            ComplexSensorData sensorData = JsonUtility.FromJson<ComplexSensorData>(jsonData);

            string accelerometerMessage = 
                $"Accélération Linéaire:\n" +
                $"Timestamp: {sensorData.linearAcceleration.timestamp}\n" +
                $"X: {sensorData.linearAcceleration.value[0]:F2}\n" +
                $"Y: {sensorData.linearAcceleration.value[1]:F2}\n" +
                $"Z: {sensorData.linearAcceleration.value[2]:F2}";

            MovePuck(new Vector2(sensorData.linearAcceleration.value[0], sensorData.linearAcceleration.value[1]));

        }
        catch (System.Exception ex)
        {
            print($"Erreur lors du traitement des données JSON : {ex.Message}");
        }
    }


    void MovePuck(Vector2 linearAcceleration)
    {

        float screenWidthCm = 35.7f; // Largeur de l'écran en cm
        float screenHeightCm = 20.1f; // Hauteur de l'écran en cm
        int screenWidthPx = 1920;  // Largeur de l'écran en pixels
        int screenHeightPx = 1080; // Hauteur de l'écran en pixels
        float tableWidthCm = 143.0f;
        float tableHeightcm = 80.6f;

        Vector2 scale = new Vector2(screenWidthCm / tableWidthCm, screenHeightCm / tableHeightcm);

        float pixelWidthCm = screenWidthCm / screenWidthPx; // Taille d'un pixel en largeur
        float pixelHeightCm = screenHeightCm / screenHeightPx; // Taille d'un pixel en hauteur

        if (Mathf.Abs(linearAcceleration.x) < 0.1 || Mathf.Abs(linearAcceleration.y) < 0.1) return;
            

        if (puck != null)
        {
            float currenTime = Time.time;

            Vector2 velocityInCmPerSecond = linearAcceleration * 1000 *0.2f  ;

            float velocityInPixelsPerSecondX = velocityInCmPerSecond.x / screenWidthPx;
            float velocityInPixelsPerSecondY = velocityInCmPerSecond.y /screenHeightPx;

            velocity = new Vector2(velocityInPixelsPerSecondX, velocityInPixelsPerSecondY) * scale;

            Vector2 potentielAdd = position * velocity ;
            Vector2 newPos = position + potentielAdd;
            UpdateUIText($"velo : {velocityInPixelsPerSecondX}/{velocityInPixelsPerSecondY}\n"+
                $"potentielAdd : {potentielAdd.x}/{potentielAdd.y}\n" +
                $"newPos : {newPos.x}/{newPos.y}");




            if (newPos.x < -15.5 &&
                newPos.x > -19.45 &&
                newPos.y < 18.04 &&
                newPos.y > 13)
            {
                position += potentielAdd;

            }
            else
            {
                position = StandartPos;
            }

            // UpdateUIText($"pixel : {pixelHeightCm} / {pixelWidthCm}\n"+
            //   $"velomparS : {velocityInMetersPerSecond.x}/{ velocityInMetersPerSecond.y}\n" +
            // $"velocmparS : {velocityInCmPerSecond.x} / {velocityInMetersPerSecond.y}\n" +
            //$"velocx : {velocityInPixelsPerSecondX} / {velocityInPixelsPerSecondY}");

           
            puck.transform.position = new Vector3(position.x, position.y,puck.transform.position.z) ;
        }
    }

    void CalculateScreenBounds()
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        screenBounds = new Vector2(screenWidth / 2, screenHeight / 2);
    }


    void UpdateUIText(string text)
    {
        print(text);
        if (jsonText != null)
        {
            jsonText.text = text;
        }
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        stream?.Close();
        tcpClient?.Close();
        print("Connexion TCP fermée");
    }
}
