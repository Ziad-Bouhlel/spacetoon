using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckPiecesScript : MonoBehaviour
{
    public List<piceseScriptGrand> objectsToCheck;
    public GameObject square;
    public Color defaultColor = Color.white;
    public Color successColor = new Color(0, 255, 0, 222);
    public Color defeatColor = new Color(255, 0, 0, 222);
    private Renderer squareRenderer;
    public TextMesh textMeshPro;
    public string joueur;
    public DragAndDropGrand dragAndDropGrand;
    private int nbPieceChecked = 0;
    private bool isMoving = false;

    void Start()
    {
        if (square != null)
        {
            squareRenderer = square.GetComponent<Renderer>();
            if (squareRenderer != null)
            {
                squareRenderer.material.color = defaultColor;
            }
            square.SetActive(false);
        }
    }

    void Update()
    {
        if (AllObjectsInRightPosition())
        {
            square.SetActive(true);
            SetSquareColor(successColor);
            textMeshPro.text = "TERMINÉ";
        }
    }

    public bool AllObjectsInRightPosition()
    {
        int tmpChecked = 0;
        foreach (var obj in objectsToCheck)
        {
            if (obj != null && obj.InRightPosition)
            {
                tmpChecked++;
            }
        }
        if (tmpChecked > nbPieceChecked)
        {
            nbPieceChecked = tmpChecked;
            sendPiecesPlayer();
        }
        return nbPieceChecked == objectsToCheck.Count;
    }

    private void sendPiecesPlayer()
    {
        dragAndDropGrand.SendMessageServer(joueur + " : " + (objectsToCheck.Count - nbPieceChecked) + "/" + objectsToCheck.Count);
    }

    private void SetSquareColor(Color color)
    {
        if (squareRenderer != null)
        {
            squareRenderer.material.color = color;
        }
    }

    public void Lost()
    {
        square.SetActive(true);
        SetSquareColor(defeatColor);
        textMeshPro.text = "PERDU";
    }

    public void UpdatePositionPieces(Vector2 deplacement)
    {
        if (!isMoving)
        {
            StartCoroutine(MovePositionsPieces(deplacement));
        }
    }

    private IEnumerator MovePositionsPieces(Vector2 deplacement)
    {
        isMoving = true;
        float elapsedTime = 0f;
        float duration = 2f; // Durée de 2 secondes

        // Sauvegarder les positions initiales et calculer les positions finales
        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        Dictionary<piceseScriptGrand, Vector3> startRightPositions = new Dictionary<piceseScriptGrand, Vector3>();

        foreach (var obj in objectsToCheck)
        {
            if (obj != null)
            {
                startPositions[obj.transform] = obj.transform.position;
                startRightPositions[obj] = obj.GetComponent<piceseScriptGrand>().RightPosition;
            }
        }

        // Animation du mouvement
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            foreach (var obj in objectsToCheck)
            {
                if (obj != null)
                {
                    if (obj.InRightPosition)
                    {
                        // Déplacer la pièce
                        obj.transform.position = Vector3.Lerp(
                            startPositions[obj.transform],
                            startPositions[obj.transform] + new Vector3(deplacement.x, deplacement.y, 0),
                            t
                        );
                    }
                    else
                    {
                        // Mettre à jour la position cible
                        obj.GetComponent<piceseScriptGrand>().RightPosition = Vector3.Lerp(
                            startRightPositions[obj],
                            startRightPositions[obj] + new Vector3(deplacement.x, deplacement.y, 0),
                            t
                        );
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // S'assurer que toutes les pièces sont à leur position finale exacte
        foreach (var obj in objectsToCheck)
        {
            if (obj != null)
            {
                if (obj.InRightPosition)
                {
                    obj.transform.position = startPositions[obj.transform] + new Vector3(deplacement.x, deplacement.y, 0);
                }
                else
                {
                    obj.GetComponent<piceseScriptGrand>().RightPosition = startRightPositions[obj] + new Vector3(deplacement.x, deplacement.y, 0);
                }
            }
        }

        isMoving = false;
    }

    public void RandomPlacementHorizontal(int min, int max)
    {
        foreach (var obj in objectsToCheck)
        {
            if (obj != null && !obj.InRightPosition)
            {
                if (900f <= obj.transform.position.x && obj.transform.position.x <= 1100f)
                {
                    if (obj.transform.position.y > min && obj.transform.position.y < max)
                        obj.GetComponent<piceseScriptGrand>().RandomPlacementHorizontal();
                }
            }
        }
    }
}