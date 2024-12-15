using System.Collections.Generic;
using UnityEngine;

public class CheckPiecesScript : MonoBehaviour
{
    // Liste des objets ayant le script PiecesScriptGrand
    public List<piceseScriptGrand> objectsToCheck;

    // Carré à colorer en vert si tous les objets sont en bonne position
    public GameObject square;

    // Couleur par défaut du carré (si besoin)
    public Color defaultColor = Color.red;

    // Couleur verte quand tous les objets sont en bonne position
    public Color successColor = Color.green;

    private Renderer squareRenderer;

    void Start()
    {
        if (square != null)
        {
            squareRenderer = square.GetComponent<Renderer>();

            // Assure que le carré a une couleur par défaut
            if (squareRenderer != null)
            {
                squareRenderer.material.color = defaultColor;
            }
        }
    }

    void Update()
    {
        // Vérifie si tous les objets de la liste sont dans la bonne position
        if (AllObjectsInRightPosition())
        {
            SetSquareColor(successColor);
        }
        else
        {
            SetSquareColor(defaultColor);
        }
    }

    private bool AllObjectsInRightPosition()
    {
        foreach (var obj in objectsToCheck)
        {
            if (obj == null || !obj.InRightPosition)
            {
                return false;
            }
        }
        return true;
    }

    private void SetSquareColor(Color color)
    {
        if (squareRenderer != null)
        {
            squareRenderer.material.color = color;
        }
    }
}


