using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solution : MonoBehaviour
{
  private bool found = false;

  public void setFound(bool found2){
        found = found2;
  }
  public bool isFound(){return found;}
}
