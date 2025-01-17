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
        square1.transform.position = square1FinalPosition;
        square2.transform.position = square2FinalPosition;
        square1Script.UpdatePositionPieces(square1Deplacement);
        square2Script.UpdatePositionPieces(square2Deplacement);
        if(!randomizePiece){
            RandomPlacementHorizontalPieces();
            GameObject.Find("vertical"+nom).SetActive(false);
      
        }
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
