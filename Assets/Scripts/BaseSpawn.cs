using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawn : MonoBehaviour
{
    public int hp;

    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("Magic") || collision.gameObject.CompareTag("Arrow")) && collision.gameObject.layer != this.gameObject.layer)
        {
            hp -= collision.gameObject.GetComponent<MagicSpell>().damage;
        }
    }
    // Update is called once per frame
}
