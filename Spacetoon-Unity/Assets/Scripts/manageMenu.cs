using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManageMenu : MonoBehaviour
{
    public GameObject iconHockey; // Ic�ne de hockey
    public GameObject iconPuzzle; // Ic�ne de puzzle
    public GameObject haloHockey; // Halo lumineux pour le hockey
    public GameObject haloPuzzle; // Halo lumineux pour le puzzle
    public Button playButton; // Bouton pour lancer le jeu

    private GameObject selectedIcon = null; // L'ic�ne actuellement s�lectionn�e

    private bool puzzleSelected = false;
    private bool hockeySelected = false;

    void Start()
    {
        // Initialisation : d�sactiver les halos et d�sactiver le bouton Play
        haloHockey.SetActive(false);
        haloPuzzle.SetActive(false);
        SetPlayButtonInteractable(false);

        // Ajouter l'�v�nement au bouton Play
        playButton.onClick.AddListener(StartGame);
    }

    // M�thode pour rendre le bouton Play interactable ou non
    void SetPlayButtonInteractable(bool interactable)
    {
        playButton.interactable = interactable; // Active ou d�sactive les clics
        Color buttonColor = playButton.image.color; // R�cup�re la couleur actuelle
        buttonColor.a = interactable ? 1f : 0.5f; // Change l'opacit� (1 = opaque, 0.5 = semi-transparent)
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
        // R�initialiser l'�tat des ic�nes
        if (icon == iconHockey && selectedIcon != iconHockey)
        {
            hockeySelected = true;
            // Activer le halo pour Hockey et d�sactiver celui pour Puzzle
            haloHockey.SetActive(true);
            haloPuzzle.SetActive(false);

            // Agrandir Hockey et ramener Puzzle � sa taille d'origine
            iconHockey.transform.localScale *= 1.3f; // Augmenter l'�chelle de 30%
            if (puzzleSelected)
            {
                iconPuzzle.transform.localScale = iconPuzzle.transform.localScale / 1.3f; // R�tablir Puzzle � sa taille normale
            }
        }
        else if (icon == iconPuzzle && selectedIcon != iconPuzzle)
        {
            puzzleSelected = true;
            // Activer le halo pour Puzzle et d�sactiver celui pour Hockey
            haloPuzzle.SetActive(true);
            haloHockey.SetActive(false);

            // Agrandir Puzzle et ramener Hockey � sa taille d'origine
            iconPuzzle.transform.localScale *= 1.3f; // Augmenter l'�chelle de 30%
            if (hockeySelected)
            {
                iconHockey.transform.localScale = iconHockey.transform.localScale / 1.3f; // R�tablir Hockey � sa taille normale
            }
        }

        // D�finir l'ic�ne s�lectionn�e
        selectedIcon = icon;

        // Rendre le bouton Play interactable
        SetPlayButtonInteractable(true);
    }



    void StartGame()
    {
        if (selectedIcon == iconHockey)
        {
            // Charger la sc�ne de hockey
            SceneManager.LoadScene("HockeyScene"); // Remplacez par le nom exact de la sc�ne
        }
        else if (selectedIcon == iconPuzzle)
        {
            // Charger la sc�ne de puzzle
            SceneManager.LoadScene("puzzleRomain"); // Remplacez par le nom exact de la sc�ne
        }
    }
}
