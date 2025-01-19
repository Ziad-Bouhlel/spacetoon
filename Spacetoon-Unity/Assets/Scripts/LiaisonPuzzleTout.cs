using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiaisonPuzzleTout : MonoBehaviour
{
    public GameObject square1;
    public GameObject square2;
    public GameObject square3;
    public GameObject square4;

    public GameObject squareVide1;
    public GameObject squareVide2;
    public GameObject squareVide3;
    public GameObject squareVide4;

    public GameObject horizontal;
    public Button buttonEnd1;
    public Button buttonEnd2;

    public CheckPiecesScript square1Script;
    public CheckPiecesScript square2Script;
    public CheckPiecesScript square3Script;
    public CheckPiecesScript square4Script;

    private Vector2 square1InitialPosition;
    private Vector2 square2InitialPosition;
    private Vector2 square3InitialPosition;
    private Vector2 square4InitialPosition;

    private Quaternion squareVide3InitialRotation;
    private Quaternion squareVide4InitialRotation;

    private Quaternion squareVide3FinalRotation;
    private Quaternion squareVide4FinalRotation;

    public Vector2 square1FinalPosition;
    public Vector2 square2FinalPosition;
    public Vector2 square3FinalPosition;
    public Vector2 square4FinalPosition;

    private bool allSquaresLinked = false; // Nouvelle variable
    private float transitionTime = 5f; // Temps total de la transition
    private float elapsedTime = 0f; // Temps �coul� pendant la transition
    private bool transitionComplete = false; // V�rifie si la transition est termin�e

    public string nom;

    [SerializeField] private AudioSource placementAudioSource;

    void Start()
    {
        buttonEnd1.gameObject.SetActive(false);
        buttonEnd2.gameObject.SetActive(false);
    }

    void Position()
    {
        square1InitialPosition = square1.transform.position;
        square2InitialPosition = square2.transform.position;
        square3InitialPosition = square3.transform.position;
        square4InitialPosition = square4.transform.position;

        squareVide3InitialRotation = squareVide3.transform.rotation;
        squareVide4InitialRotation = squareVide4.transform.rotation;

        squareVide3FinalRotation = Quaternion.Euler(0, 0, 180);
        squareVide4FinalRotation = Quaternion.Euler(0, 0, 180);
    }

    void Update()
    {
        if (!allSquaresLinked && AreAllSquaresCompleted())
        {
            Position();
            allSquaresLinked = true; // Marque la liaison comme commenc�e
            elapsedTime = 0f; // R�initialise le temps pour la transition
        }

        if (allSquaresLinked && elapsedTime <= transitionTime)
        {
            horizontal.SetActive(false);
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / transitionTime); // Normalise le temps �coul�

            squareVide3.transform.rotation = Quaternion.Slerp(squareVide3InitialRotation, squareVide3FinalRotation, t);
            squareVide4.transform.rotation = Quaternion.Slerp(squareVide4InitialRotation, squareVide4FinalRotation, t);

            // Interpolation des carr�s vides
            squareVide1.transform.position = Vector2.Lerp(square1InitialPosition, square1FinalPosition, t);
            squareVide2.transform.position = Vector2.Lerp(square2InitialPosition, square2FinalPosition, t);
            squareVide3.transform.position = Vector2.Lerp(square3InitialPosition, square3FinalPosition, t);
            squareVide4.transform.position = Vector2.Lerp(square4InitialPosition, square4FinalPosition, t);

            placementAudioSource.Play(); // Jouer l'audio une fois
            if (t >= 1f)
            {
                CompleteTransition();
            }
        }
    }

    private void CompleteTransition()
    {
        // Positionner les boutons
        buttonEnd1.gameObject.SetActive(true);
        buttonEnd2.gameObject.SetActive(true);

        // Marquer la transition comme termin�e
        transitionComplete = true;
    }

    private bool AreAllSquaresCompleted()
    {
        return square1Script.AllObjectsInRightPosition() &&
               square2Script.AllObjectsInRightPosition() &&
               square3Script.AllObjectsInRightPosition() &&
               square4Script.AllObjectsInRightPosition();
    }
}
