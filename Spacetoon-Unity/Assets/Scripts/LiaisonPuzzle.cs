using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiaisonPuzzle : MonoBehaviour
{
     [System.Serializable]
    public class PiecePair
    {
        public piceseScriptGrand piece1;
        public piceseScriptGrand piece2;
    }

    public List<PiecePair> piecePairs; 
    public GameObject square1;
    public GameObject square2;

    public CheckPiecesScript square1Script;
    public CheckPiecesScript square2Script;

        public CheckPiecesScript square3Script;
    public CheckPiecesScript square4Script;
    private Vector2 square1InitialPosition;
    private Vector2 square2InitialPosition;
    private Vector2 square1Deplacement;
    private Vector2 square2Deplacement;
    public Vector2 square1FinalPosition;
    public Vector2 square2FinalPosition;
    private bool pairFound = false;
    private bool randomizePiece = false;
    public LiaisonPuzzle otherLiaison;
    public string nom;

    [SerializeField] private AudioSource placementAudioSource;
    void Start(){
        square1InitialPosition = square1.transform.position;
        square2InitialPosition = square2.transform.position;
        square1Deplacement = square1FinalPosition - square1InitialPosition;
        square2Deplacement = square2FinalPosition - square2InitialPosition;
    }
    void Update()
    {
        if(!pairFound){
            foreach (var pair in piecePairs)
            {
                if (pair.piece1.InRightPosition && pair.piece2.InRightPosition)
                {
                    LierPuzzle(pair);
                    pairFound = true;
                    break; 
                }
            }
        }
    }

    private void LierPuzzle(PiecePair pair)
{
    Debug.Log($"Paire liée : {pair.piece1.name} et {pair.piece2.name}");

    // Start the linking animation with a coroutine
    StartCoroutine(LinkPiecesOverTime());
}

private IEnumerator LinkPiecesOverTime()
{
    float duration = 2f;  // Duration of the animation in seconds
    float elapsedTime = 0f;

    Vector2 initialPosition1 = square1.transform.position;
    Vector2 initialPosition2 = square2.transform.position;

        if (!randomizePiece)
        {
            RandomPlacementHorizontalPieces();
            GameObject.Find("vertical" + nom).SetActive(false);
        }
        // Move the pieces gradually over time
        while (elapsedTime < duration)
    {
        float t = elapsedTime / duration;
        
        // Interpolate between initial and final positions
        square1.transform.position = Vector2.Lerp(initialPosition1, square1FinalPosition, t);
        square2.transform.position = Vector2.Lerp(initialPosition2, square2FinalPosition, t);

        square1Script.UpdatePositionPieces(square1Deplacement);
        square2Script.UpdatePositionPieces(square2Deplacement);

            elapsedTime += Time.deltaTime;
        yield return null;  // Wait for the next frame
    }

    // Ensure the final position is set at the end
    square1.transform.position = square1FinalPosition;
    square2.transform.position = square2FinalPosition;

    // Call the scripts to update positions after animation

    placementAudioSource.Play();
}

    public void RandomPlacementHorizontalPieces(){
        if(!randomizePiece){
            if(nom == "Haut"){
            square1Script.RandomPlacementHorizontal(650,1200);
            square2Script.RandomPlacementHorizontal(650,1200);
            square3Script.RandomPlacementHorizontal(650,1200);
            square4Script.RandomPlacementHorizontal(650,1200);
            }else{
            square1Script.RandomPlacementHorizontal(-100,430);
            square2Script.RandomPlacementHorizontal(-100,430);
            square3Script.RandomPlacementHorizontal(-100,430);
            square4Script.RandomPlacementHorizontal(-100,430);
            }
    
        randomizePiece = true;
        }
       
    }
}
