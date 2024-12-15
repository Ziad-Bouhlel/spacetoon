using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class DragAndDrop_ : MonoBehaviour
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

    void Start()
    {
        for (int i = 0; i < 36; i++)
        {
            GameObject.Find("Piece (" + i + ")").transform.Find("Puzzle").GetComponent<SpriteRenderer>().sprite = Levels[PlayerPrefs.GetInt("Level")];
        }
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();

        if (PlacedPieces == 36)
        {
            EndMenu.SetActive(true);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.transform != null && hit.transform.CompareTag("Puzzle"))
            {
                GameObject piece = hit.transform.gameObject;

                // Détection du double-clic
                if (piece == lastClickedObject && Time.time - lastClickTime < doubleClickTime)
                {
                    //RotatePiece(piece);
                    //lastClickedObject = null; // Réinitialiser après le double-clic
                }
                else
                {
                    lastClickTime = Time.time;
                    lastClickedObject = piece;

                    // Sélection pour drag & drop
                    if (!piece.GetComponent<piceseScript>().InRightPosition)
                    {
                        SelectedPiece = piece;
                        SelectedPiece.GetComponent<piceseScript>().Selected = true;
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
                SelectedPiece.GetComponent<piceseScript>().Selected = false;
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
                        if (piece == lastClickedObject && Time.time - lastClickTime < doubleClickTime)
                        {
                            //RotatePiece(piece);
                            //lastClickedObject = null; // Réinitialiser après le double-tap
                        }
                        else
                        {
                            lastClickTime = Time.time;
                            lastClickedObject = piece;

                            // Sélection pour drag & drop
                            if (!piece.GetComponent<piceseScript>().InRightPosition && !activeTouches.ContainsKey(touch.fingerId))
                            {
                                activeTouches[touch.fingerId] = piece;
                                piece.GetComponent<piceseScript>().Selected = true;
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
                        piece.GetComponent<piceseScript>().Selected = false;
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

    public void NextLevel()
    {
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        SceneManager.LoadScene("Game");
    }

    public void BacktoMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
