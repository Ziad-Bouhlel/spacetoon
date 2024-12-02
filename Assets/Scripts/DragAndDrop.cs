using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public GameObject SelectedPiece;

    void Start()
    {
    }

    void Update()
    {
        // Support pour la souris (PC)
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectedPiece = null;
        }

        // Support pour le tactile (Tablette/Smartphone)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                HandleInput(touchPosition);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                SelectedPiece = null;
            }
            else if (touch.phase == TouchPhase.Moved && SelectedPiece != null)
            {
                MovePiece(touchPosition);
            }
        }

        // Déplacement pour la souris
        if (SelectedPiece != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MovePiece(mousePosition);
        }
    }

    private void HandleInput(Vector3 inputPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(inputPosition, Vector2.zero);

        if (hit.transform != null && hit.transform.CompareTag("Puzzle"))
        {
            SelectedPiece = hit.transform.gameObject;
        }
    }

    private void MovePiece(Vector3 inputPosition)
    {
        SelectedPiece.transform.position = new Vector3(inputPosition.x, inputPosition.y, 0);
    }
}
