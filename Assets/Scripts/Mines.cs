using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mines : MonoBehaviour
{
    #region //Member
    public int mineHealth = 100;
    public bool isTouching = false;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D miner)
    {
        if (miner.gameObject.tag == "Miner")
        {
            isTouching = true;
        }
    }


    // Update is called once per frame

}
