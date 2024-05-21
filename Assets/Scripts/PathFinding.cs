using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding 
{

    public Transform seeker, target;
    public GameObject gridObject;
    public Grid grid;
    float count = 0.0f;
    bool isCalculated = false;
    List<Node> currentPath = new List<Node>();

    public  void FindPath(Vector2 seekerPos, Vector2 targetPos) {
        Node starNode = grid.NodeFromWorldPoint(seekerPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closeSet = new HashSet<Node>();
        openSet.Add(starNode);

        while (openSet.Count > 0) {
            Node currentNode = openSet[0];
            for(int i =1; i < openSet.Count; i++) {
                if (openSet[i].fCost < currentNode.fCost ||
                    openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            closeSet.Add(currentNode);

            if (currentNode == targetNode) {
                RetracePath(starNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode)) { 
                if(!neighbour.walkable || closeSet.Contains(neighbour)){
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost ||
                    !openSet.Contains(neighbour)) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

    }

    void RetracePath(Node starNode, Node endNode) {
        List<Node> path = new List<Node>();

        Node currentNode = endNode;

        while (currentNode != starNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        currentPath = path;
        grid.path = path;
        
    }

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY) {
            return 14 * dstY + 10 * (dstX - dstY); 
        }
        return 14 * dstY + 10 * (dstY - dstX);
    }

    public int GetGridPathSize()
    {
        return grid.path.Count;
    }

    public List<Node> GetGridPath()
    {

        return grid.path;
    }
    public List<Node> GetCurrentPath()
    {

        return currentPath;
    }

    void PathFindingCooldown()
    {
        count -= Time.deltaTime;
        if (count <=  0)
        {
            isCalculated = false;
            count = 0.0f;
        }

    }
}
