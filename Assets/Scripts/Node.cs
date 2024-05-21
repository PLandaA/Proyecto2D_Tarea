using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public bool walkable;
    public Vector2 wordlPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node parent;
    public Node(bool p_walkable, Vector2 p_worldPos, int p_gridX, int p_gridY){
        walkable = p_walkable;
        wordlPosition = p_worldPos;
        gridX = p_gridX;
        gridY = p_gridY;
    }

    public int fCost {
        get { return gCost * hCost; }
    }
}
