using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentEvilNinja : MonoBehaviour
{
    enum AgentState
    {
        NONE,
        ONRISK,
        ATTACK
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float wheelRadius;
    [SerializeField] float displacment, wanderRange;
    [SerializeField] LayerMask whatIsEnemy;
    [SerializeField] public bool isEnemy;

    #endregion

    #region //Member
    AgentState agentState;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> attackAgents;
    SteeringBehaviors sb;
    GameObject target;
    Rigidbody2D rb;

    #endregion

    #region //Public
    public float maxSpeed, maxForce, rotateSpeed;
    public int hp, hpMax;
    public int damage, attackRadius;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        agentState = AgentState.NONE;

    }
    void FixedUpdate()
    {
        PerceptionManager();
        movementManager();
    }
    // Update is called once per frame
    void Update()
    {
        DecisionManager();
        ActionManager();
        //Debug.Log("Ninja:" + hp);
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager()
    {
        bool isTarget = false;
        Collider2D[] agents = Physics2D.OverlapCircleAll(transform.position, attackRadius, whatIsEnemy);
        attackAgents = new List<Collider2D>(agents);

        // Hay obstáculos?
        if (attackAgents != null)
        {
            foreach (Collider2D tmp in attackAgents)
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
        agentState = AgentState.NONE;
        if (target != null)
        {
            agentState = AgentState.ATTACK;
        }

        if (hp < 50)
        {
            agentState = AgentState.ONRISK;
        }
        if (target == null)
        {
            agentState = AgentState.NONE;
        }

    }
    void movementManager()
    {
        switch (agentState)
        {
            case AgentState.ATTACK:
                try
                {
                    sb.Seek(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
                }
                catch { }
                break;
            case AgentState.ONRISK:
                try
                {
                    sb.Flee(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
                }
                catch
                { }
                break;

            case AgentState.NONE:
                try
                {
                    sb.Wander(gameObject, displacment, wheelRadius, wanderRange, maxSpeed, maxForce, rotateSpeed, rb);

                }
                catch { }
                break;
        }
    }

    void ActionManager()
    {
        try
        {
            if (target.CompareTag("Ninja"))
            {
                hp -= target.GetComponent<AgentNinja>().damage;
                target.GetComponent<AgentNinja>().hp -= damage;

            }
            if (target.CompareTag("Tank"))
            {
                hp -= target.GetComponent<AgentTank>().damage;
                target.GetComponent<AgentTank>().hp -= damage;

            }
            if (target.CompareTag("Miner"))
            {
                target.GetComponent<AgentMiner>().hp -= damage;

            }
            if (target.CompareTag("Commander"))
            {
                hp -= target.GetComponent<AgentCommander>().damage;
                target.GetComponent<AgentCommander>().hp -= damage;
            }
            if (target.CompareTag("Booster"))
            {
                target.GetComponent<AgentBooster>().hp -= damage;
            }
            if (target.CompareTag("Cold"))
            {
                hp -= (int)target.GetComponent<AgentCold>().damage;

            }
        }
        catch { }

        if (hp <= 0)
        {
            WaitForDie();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
    void WaitForDie()
    {
        if (!playAudio)
        {
            dieAudio.Play();
            playAudio = true;
        }
        timeToDie -= Time.deltaTime;
        if (timeToDie <= 0)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}

