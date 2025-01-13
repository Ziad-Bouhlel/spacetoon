using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;

public class ManageMenu : MonoBehaviour
{
    public GameObject iconHockey; // Icône de hockey
    public GameObject iconPuzzle; // Icône de puzzle
    public GameObject haloHockey; // Halo lumineux pour le hockey
    public GameObject haloPuzzle; // Halo lumineux pour le puzzle
    public Button playButton; // Bouton pour lancer le jeu

    private GameObject selectedIcon = null; // L'icône actuellement sélectionnée

    private bool puzzleSelected = false;
    private bool hockeySelected = false;

    // Paramètres pour la connexion réseau
    private string serverIP = "127.0.0.1"; // Adresse IP du serveur
    private int serverPort = 5000;        // Port du serveur
    private TcpClient client;
    
    [SerializeField] private AudioSource placementAudioSource; 

    void Start()
    {
        // Initialisation : désactiver les halos et désactiver le bouton Play
        haloHockey.SetActive(false);
        haloPuzzle.SetActive(false);
        SetPlayButtonInteractable(false);

        // Ajouter l'événement au bouton Play
        playButton.onClick.AddListener(StartGame);

        // Établir une connexion avec le serveur
        try
        {
            client = new TcpClient(serverIP, serverPort);
            Debug.Log("Connexion au serveur établie.");
            SendMessageServer("menuDuJeu");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur de connexion au serveur : " + e.Message);
        }
    }

    // Méthode pour rendre le bouton Play interactable ou non
    void SetPlayButtonInteractable(bool interactable)
    {
        playButton.interactable = interactable; // Active ou désactive les clics
        Color buttonColor = playButton.image.color; // Récupère la couleur actuelle
        buttonColor.a = interactable ? 1f : 0.5f; // Change l'opacité (1 = opaque, 0.5 = semi-transparent)
        playButton.image.color = buttonColor; // Applique la nouvelle couleur
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == iconHockey)
                {
                    Debug.Log("cliquage hockey");
                    SelectGame(iconHockey);
                }
                else if (hit.collider.gameObject == iconPuzzle)
                {
                    Debug.Log("cliquage puzzle");
                    SelectGame(iconPuzzle);
                }
            }
        }
    }

    void SelectGame(GameObject icon)
    {
        // Réinitialiser l'état des icônes
        if (icon == iconHockey && selectedIcon != iconHockey)
        {
            hockeySelected = true;
            haloHockey.SetActive(true);
            haloPuzzle.SetActive(false);
            iconHockey.transform.localScale *= 1.3f;
            if (puzzleSelected)
            {
                iconPuzzle.transform.localScale /= 1.3f;
            }
        }
        else if (icon == iconPuzzle && selectedIcon != iconPuzzle)
        {
            puzzleSelected = true;
            haloPuzzle.SetActive(true);
            haloHockey.SetActive(false);
            iconPuzzle.transform.localScale *= 1.3f;
            if (hockeySelected)
            {
                iconHockey.transform.localScale /= 1.3f;
            }
        }

        selectedIcon = icon;
        SetPlayButtonInteractable(true);

       if (placementAudioSource != null)
            {
                placementAudioSource.Play();
            }
            else
            {
                Debug.LogWarning("AudioSource non assignée pour le placement des pièces.");
            }
    }

    void StartGame()
    {
        if (selectedIcon == iconHockey)
        {
            SceneManager.LoadScene("AirHockey");
        }
        else if (selectedIcon == iconPuzzle)
        {
             string message = "{\"start\":\"puzzle\"}";
            SendMessageServer(message); // Envoi de la donnée au serveur
            SceneManager.LoadScene("puzzleRomain");
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