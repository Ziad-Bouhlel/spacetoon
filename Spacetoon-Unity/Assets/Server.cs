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

    [System.Serializable]
    public class JsonDataJoystick
    {
        public long timestamp;
        public int joueur;
        public float x;
        public float y;
    }

    public TextMeshProUGUI jsonText;
    public GameObject puckJ1;
    public GameObject puckJ2;
    public float speedFactor = 20f;

    private TcpClient tcpClient;
    private NetworkStream stream;
    public string serverIP;
    public int port = 5000;
    private Thread receiveThread;

    private Vector2 screenBounds;

    private float previousTime;
    private Vector2 velocity = Vector2.zero;
    private Vector2 positionJ1, positionJ2;
    private Vector2 StandartPosJ1, StandartPosJ2;


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

        positionJ1 = new Vector2(puckJ1.transform.position.x, puckJ1.transform.position.y);
        StandartPosJ1 = positionJ1;

        positionJ2 = new Vector2(puckJ2.transform.position.x, puckJ2.transform.position.y);
        StandartPosJ2 = positionJ2;

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
            UpdateUIText(Camera.main.orthographicSize.ToString());


            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (SocketException ex)
        {
            print($"Erreur de connexion TCP : {ex.Message}");
            UpdateUIText($"Erreur de connexion TCP : {serverIP}");

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

                    ProcessSensorDatav2(message);

                }
            }
            catch (System.Exception ex)
            {
                UpdateUIText($"Erreur dans ReceiveMessages : {ex.Message}");
                break;
            }
        }
    }

    private long lastProcessedTimestamp = 0; // Stocke le dernier timestamp traité
    void ProcessSensorDatav2(string jsonData)
    {
        UpdateUIText($"Receiving messages... ");
        JsonDataJoystick sensorData = JsonUtility.FromJson<JsonDataJoystick>(jsonData);
        UpdateUIText(
            $"x : {sensorData.x}\n" +
            $"y : {sensorData.y}\n" +
            $"joueur : {sensorData.joueur}\n" +
            $"timestamp : {sensorData.timestamp}\n"
            );

        long currentTimestamp = sensorData.timestamp;

        if (currentTimestamp <= lastProcessedTimestamp)
        {
            UpdateUIText("Données reçues avec un timestamp invalide ou désynchronisé. Ignorées.");
            return;
        }
        MovePuckV2(sensorData.joueur, new Vector2(sensorData.x, sensorData.y));
        lastProcessedTimestamp = currentTimestamp; // Met à jour le dernier timestamp traité

    }

    void MovePuckV2(int joueur, Vector2 jsonData)
    {
        if (puckJ1 == null || puckJ2 == null) return;
        if (joueur == 1)
        {
            puckJ1.transform.position += new Vector3(jsonData.x, -jsonData.y, 0);
        }
        else if (joueur == 2)
        {
            puckJ2.transform.position += new Vector3(jsonData.x, -jsonData.y, 0);
        }
    }
    void ProcessSensorDatav1(string jsonData)
    {
        try
        {
            ComplexSensorData sensorData = JsonUtility.FromJson<ComplexSensorData>(jsonData);

            long currentTimestamp = sensorData.linearAcceleration.timestamp;

            // Vérification du timestamp
            if (currentTimestamp <= lastProcessedTimestamp)
            {
                UpdateUIText("Données reçues avec un timestamp invalide ou désynchronisé. Ignorées.");
                return;
            }

            lastProcessedTimestamp = currentTimestamp; // Met à jour le dernier timestamp traité
            float x = sensorData.linearAcceleration.value[0];
            float y = sensorData.linearAcceleration.value[1];
            float z = sensorData.linearAcceleration.value[2];

            if (Mathf.Abs(x) < 1 || Mathf.Abs(z) < 1) return;

            string accelerometerMessage = "";
            if (x > y && x > z)
            {
                accelerometerMessage = $"X: {x:F2}";

            }
            else if (y > z && y > x)
            {
                accelerometerMessage = $"Y: {y:F2}";
            }
            else if (z > x && z > y)
            {
                accelerometerMessage = $"Z: {z:F2}";
            }


            // Optionnel : Afficher les données dans l'UI
            UpdateUIText(accelerometerMessage);

            // Déplacer le puck avec les nouvelles données
            MovePuck(new Vector2(-sensorData.linearAcceleration.value[2], -sensorData.linearAcceleration.value[0]));
        }
        catch (System.Exception ex)
        {
            print($"Erreur lors du traitement des données JSON : {ex.Message}");
        }
    }

    float lastTime;
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



        if (puckJ1 != null)
        {
            float currenTime = Time.time;
            float deltaTime = currenTime - lastTime;
            Vector2 velocityInCmPerSecond = linearAcceleration * deltaTime * 100;

            float velocityInPixelsPerSecondX = velocityInCmPerSecond.x * pixelWidthCm;
            float velocityInPixelsPerSecondY = velocityInCmPerSecond.y * pixelHeightCm;

            velocity = new Vector2(velocityInPixelsPerSecondX, velocityInPixelsPerSecondY);

            Vector2 potentielAdd = positionJ1 * velocity * deltaTime;
            Vector2 newPos = positionJ1 + potentielAdd;
            UpdateUIText($"velo : {velocityInPixelsPerSecondX}/{velocityInPixelsPerSecondY}\n" +
                $"potentielAdd : {potentielAdd.x}/{potentielAdd.y}\n" +
                $"newPos : {newPos.x}/{newPos.y}");




            if (newPos.x < -15.5 &&
                newPos.x > -19.45 &&
                newPos.y < 18.04 &&
                newPos.y > 13)
            {
                positionJ1 += potentielAdd;

            }
            else
            {
                positionJ1 = StandartPosJ1;
            }

            // UpdateUIText($"pixel : {pixelHeightCm} / {pixelWidthCm}\n"+
            //   $"velomparS : {velocityInMetersPerSecond.x}/{ velocityInMetersPerSecond.y}\n" +
            // $"velocmparS : {velocityInCmPerSecond.x} / {velocityInMetersPerSecond.y}\n" +
            //$"velocx : {velocityInPixelsPerSecondX} / {velocityInPixelsPerSecondY}");


            puckJ1.transform.position = new Vector3(positionJ1.x, positionJ1.y, puckJ1.transform.position.z);

            lastTime = Time.time;
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
