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
            print("Veuillez assigner une r�f�rence TextMeshPro dans l'inspecteur !");
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
            print("Connect� au serveur TCP");
            UpdateUIText("Connexion");
            UpdateUIText(Camera.main.orthographicSize.ToString());


            // Lecture de la demande d'identité
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            if (serverMessage == "IDENTIFY")
            {
                print($"Envoi de l'identité au serveur : hockeyJeu");
                // Envoie l'identité au serveur
                byte[] identityMessage = Encoding.UTF8.GetBytes("hockeyJeu");
                stream.Write(identityMessage, 0, identityMessage.Length);
                print("Identité envoyée au serveur.");
            }

            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (SocketException ex)
        {
            UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
            {
                UpdateUIText($"Erreur de connexion TCP : {ex.Message}");
            });
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
            print($"Message envoy� : {message}");

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
                    UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
                    {
                        string[] jsonObjects = message.Split(new[] { '}' }, System.StringSplitOptions.RemoveEmptyEntries);
                        foreach (string jsonObject in jsonObjects)
                        {
                            string validJson = jsonObject + "}"; // Réajouter la parenthèse fermante
                            ProcessSensorDatav2(validJson);
                        }
                    });
                }
            }
            catch (System.Exception ex)
            {
                UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
                {
                    UpdateUIText($"Erreur dans ReceiveMessages : {ex.Message}");
                });
                break;
            }
        }
    }

    private long lastProcessedTimestamp = 0; // Stocke le dernier timestamp trait�
    void ProcessSensorDatav2(string jsonData)
    {
        print(jsonData);
        UpdateUIText($"Receiving messages... ");
        JsonDataJoystick sensorData = JsonUtility.FromJson<JsonDataJoystick>(jsonData);
        print(sensorData);

        long currentTimestamp = sensorData.timestamp;

        if (currentTimestamp <= lastProcessedTimestamp)
        {
            UpdateUIText("Donn�es re�ues avec un timestamp invalide ou d�synchronis�. Ignor�es.");
            return;
        }
        MovePuckV2(sensorData.joueur, new Vector2(sensorData.x, sensorData.y));
        lastProcessedTimestamp = currentTimestamp; // Met � jour le dernier timestamp trait�

    }

    void checkPos(Vector2 newVector, double x1, double x2, GameObject puck)
    {
        Vector3 newPos = puck.transform.position+new Vector3(newVector.x, -newVector.y, 0);
        print($"newVector = {newPos}");
        if (newPos.y < 18.04 && newPos.y > 13.85 && newPos.x < x1 && newPos.x > x2)
        {
            puck.transform.position = newPos;
        }
    }
    void MovePuckV2(int joueur, Vector2 jsonData)
    {
        if (puckJ1 == null || puckJ2 == null) return;
        if (joueur == 1)
        {
            checkPos(jsonData, -20.3, -24.5, puckJ1);
        }
        else if (joueur == 2)
        {
            checkPos(jsonData, -15.5, -19.7, puckJ2);
        }
    }

    /*
    void ProcessSensorDatav1(string jsonData)
    {
        try
        {
            ComplexSensorData sensorData = JsonUtility.FromJson<ComplexSensorData>(jsonData);

            long currentTimestamp = sensorData.linearAcceleration.timestamp;

            // V�rification du timestamp
            if (currentTimestamp <= lastProcessedTimestamp)
            {
                UpdateUIText("Donn�es re�ues avec un timestamp invalide ou d�synchronis�. Ignor�es.");
                return;
            }

            lastProcessedTimestamp = currentTimestamp; // Met � jour le dernier timestamp trait�
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


            // Optionnel : Afficher les donn�es dans l'UI
            UpdateUIText(accelerometerMessage);

            // D�placer le puck avec les nouvelles donn�es
            MovePuck(new Vector2(-sensorData.linearAcceleration.value[2], -sensorData.linearAcceleration.value[0]));
        }
        catch (System.Exception ex)
        {
            print($"Erreur lors du traitement des donn�es JSON : {ex.Message}");
        }
    }

    float lastTime;
    void MovePuck(Vector2 linearAcceleration)
    {


        float screenWidthCm = 35.7f; // Largeur de l'�cran en cm
        float screenHeightCm = 20.1f; // Hauteur de l'�cran en cm
        int screenWidthPx = 1920;  // Largeur de l'�cran en pixels
        int screenHeightPx = 1080; // Hauteur de l'�cran en pixels
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
    */

    void CalculateScreenBounds()
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        screenBounds = new Vector2(screenWidth / 2, screenHeight / 2);
    }


    void UpdateUIText(string text)
    {
        UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            print(text);
            if (jsonText != null)
            {
                jsonText.text = text;
            }
            else {
            Debug.LogWarning("TextMeshProUGUI non assigné dans l'inspecteur.");
            }
        });
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    void OnDestroy()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        stream?.Close();
        tcpClient?.Close();
    }

    void Cleanup()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        stream?.Close();
        tcpClient?.Close();
        print("Connexion TCP ferm�e");
    }
}
