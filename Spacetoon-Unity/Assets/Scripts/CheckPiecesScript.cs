using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckPiecesScript : MonoBehaviour
{
   
    public List<piceseScriptGrand> objectsToCheck;
    public GameObject square;

   
    public Color defaultColor = Color.white;

    public Color successColor = new Color(0,255,0,222);

    public Color defeatColor = new Color(255,0,0,222);

    private Renderer squareRenderer;

    public TextMesh textMeshPro;

    public string joueur;

    public DragAndDropGrand dragAndDropGrand;

    private int nbPieceChecked = 0;


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
            square.SetActive(false);
        }
      //  sendPiecesPlayer();
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
        if(tmpChecked > nbPieceChecked){
            nbPieceChecked = tmpChecked;
            sendPiecesPlayer();
        }
        return nbPieceChecked == objectsToCheck.Count;
    }
    private void sendPiecesPlayer(){
  dragAndDropGrand.SendMessageServer(joueur +" : " + (objectsToCheck.Count-nbPieceChecked) +"/" + objectsToCheck.Count);
 
    }
    private void SetSquareColor(Color color)
    {
        if (squareRenderer != null)
        {
            squareRenderer.material.color = color;
            
        }
    }

    public void Lost(){
        square.SetActive(true);
        SetSquareColor(defeatColor);
        textMeshPro.text = "PERDU";
        
    }

    public void UpdatePositionPieces(Vector2 deplacement){
        foreach (var obj in objectsToCheck)
        {
            if (obj != null && obj.InRightPosition)
            {
                obj.transform.position += new Vector3(deplacement.x,deplacement.y,0); 
            }
              if (obj != null && !obj.InRightPosition)
            {
                obj.GetComponent<piceseScriptGrand>().RightPosition +=  new Vector3(deplacement.x,deplacement.y,0);
            }
        }
    }

    public void RandomPlacementHorizontal(int min,int max){
         foreach (var obj in objectsToCheck)
        {
            if (obj != null && !obj.InRightPosition)
            {
                if( 900f<= obj.transform.position.x && obj.transform.position.x<= 1100f ){    
                    if(obj.transform.position.y>min && obj.transform.position.y<max)
                    obj.GetComponent<piceseScriptGrand>().RandomPlacementHorizontal();
                }
            }
        }
          
    }
}


