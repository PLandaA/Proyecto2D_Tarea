using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public List<GameObject> towers;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        checkForTowers();
        
    }
    private void checkForTowers()
    {
        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i].activeSelf == false)
            {
                towers.RemoveAt(i);
            }
        }
        if (towers.Count <= 0)
        {
            //GameOver();
        }
    }

}
