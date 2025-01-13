using System.Collections.Generic;
using UnityEngine;

public class CheckPiecesScript : MonoBehaviour
{
   
    public List<piceseScriptGrand> objectsToCheck;
    public GameObject square;

   
    public Color defaultColor = Color.white;

    public Color successColor = Color.green;

    public Color defeatColor = Color.red;

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
        if (AllObjectsInRightPosition())
        {
            SetSquareColor(successColor);
        }

    }

    public bool AllObjectsInRightPosition()
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

    public void Lost(){
        SetSquareColor(defeatColor);
    }
}


