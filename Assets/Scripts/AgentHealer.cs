using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentHealer : MonoBehaviour
{
    enum AgentState
    {
        NONE,
        ONRISK,
        HEALER
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float evadeRadius, healerRadius, wheelRadius;
    [SerializeField] float futureSight, futureSightRadius;

    [SerializeField] float displacment, wanderRange, liveTime;
    [SerializeField] public LayerMask whatIsTower, whatIsAlly;
    [SerializeField] LayerMask whatIsObs;

    #endregion

    #region //Member
    float timeToDie;
    bool playAudio = false;
    AgentState agentState;
    List<Collider2D> objects;
    SteeringBehaviors sb;
    GameObject target;
    Rigidbody2D rb;
 

    #endregion

    #region //Public
    public float maxSpeed, maxForce, rotateSpeed, futureMag;
    public int healerPoints;
    [HideInInspector] public bool heal;
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
        liveTime -= Time.deltaTime;
        DecisionManager();
        ActionManager();
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager()
    {
        bool isTarget = false;
        Collider2D[] towers = Physics2D.OverlapCircleAll(transform.position, evadeRadius, whatIsTower);
        objects = new List<Collider2D>(towers);
        Collider2D[] allides = Physics2D.OverlapCircleAll(transform.position, healerRadius, whatIsAlly);
        for (int i = 0; i < allides.Length; i++) {
            objects.Add(allides[i]);
        }

        // Hay obstáculos?
        if (objects != null)
        {
            foreach (Collider2D tmp in objects)
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
            if (target.gameObject.tag == "Tower")
            {
                if (!heal)
                {
                    agentState = AgentState.ONRISK;
                }
                agentState = AgentState.ONRISK;

            }
            if (target.gameObject.tag == "Ninja" || target.gameObject.tag == "Tank" || target.gameObject.tag == "Commander"
                || target.gameObject.tag == "Cold" || target.gameObject.tag == "Archer" || target.gameObject.tag == "Magician")
            {
                agentState = AgentState.HEALER;

            }
        }
        else
        {
            agentState = AgentState.NONE;
        }


    }
    void movementManager()
    {
        switch (agentState)
        {
            case AgentState.HEALER:
                try
                {
                    sb.Pursuit(target, transform,futureMag, maxSpeed, maxForce, rotateSpeed, rb);
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

        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);

    }

    void ActionManager()
    {
        try { 
            if (target.gameObject.tag == "Ninja") {
                if (target.GetComponent<AgentNinja>().hp < 50)
                {
                    target.GetComponent<AgentNinja>().hp += healerPoints;
                }
            } 
            if(target.gameObject.tag == "Tank")
            {
                if (target.GetComponent<AgentTank>().hp < 40)
                {
                    target.GetComponent<AgentTank>().hp += healerPoints;
                }
                
            }
            if (target.gameObject.tag == "Commander")
            {
                if (target.GetComponent<AgentCommander>().hp < 50)
                {
                    target.GetComponent<AgentCommander>().hp += healerPoints;
                }
            }
            if (target.gameObject.tag == "Cold")
            {
                if (target.GetComponent<AgentCold>().hp < 50)
                {
                    target.GetComponent<AgentCold>().hp += healerPoints;
                }
            }
            if (target.gameObject.tag == "Archer")
            {
                if (target.GetComponent<AgentArcher>().hp < 70)
                {
                    target.GetComponent<AgentArcher>().hp += healerPoints;
                }
            }
            if (target.gameObject.tag == "Magician")
            {
                if (target.GetComponent<AgentMagician>().hp < 80)
                {
                    target.GetComponent<AgentMagician>().hp += healerPoints;
                }
            }
        }
        catch {}
        if (liveTime<=0)
        {
            WaitForDie();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, healerRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);
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
