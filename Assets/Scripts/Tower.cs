using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    enum AgentState
    {
        NONE,
        ATTACK
    }
    #region //Serializable
    [SerializeField] float attackRadius;
    [SerializeField] LayerMask whatIsEnemy;
    #endregion

    #region //Member
    AgentState agentState;
    List<Collider2D> enemies;
    GameObject target;
    #endregion

    #region //Public
    [HideInInspector] public int hp = 500;
    public int damage;
    [HideInInspector] public bool isDamage = false, attacking = false;
    #endregion

    void Start()
    {
    }
    void FixedUpdate()
    {
        PerceptionManager();
    }
    void Update()
    {
        DecisionManager();
        ActionManager();

    }
    void PerceptionManager()
    {
        bool isTarget = false;
        Collider2D[] Enemies = Physics2D.OverlapCircleAll(transform.position, attackRadius, whatIsEnemy);
        enemies = new List<Collider2D>(Enemies);

        // Hay obstáculos?
        if (enemies != null)
        {
            foreach (Collider2D tmp in enemies)
            {
                target = tmp.gameObject;
                isTarget = true;
            }
            if (!isTarget)
            {
                target = null;
            }
        }

    }
    void DecisionManager()
    {
        
        if (target != null)
        {
           agentState = AgentState.ATTACK;
        }
        else
        {
            agentState = AgentState.NONE;
        }


    }
    void ActionManager()
    {
        if(agentState == AgentState.ATTACK)
        {
            attacking = true;
            
        }
        else
        {
            attacking = false;
        }
        if (isDamage)
        {
            try
            {
                if (target.tag == "Ninja")
                {
                    hp -= target.GetComponent<AgentNinja>().damage;
                    Debug.Log("Tower: " + hp);
                }
                if(target.tag == "Tank")
                {
                    if (Vector3.Distance(transform.position, target.transform.position)/2 <= target.GetComponent<AgentTank>().attackRadius)
                    {
                        hp -= target.GetComponent<AgentTank>().damage;
                        //Debug.Log("Tower: " + hp);
                    }
                }
            }
            catch { }
        }
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ninja" || collision.gameObject.tag == "Tank")
        {
            isDamage = true;
        }

    }

}
