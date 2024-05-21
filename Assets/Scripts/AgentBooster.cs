using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBooster : MonoBehaviour
{
    enum AgentState
    {
        ONRISK,
        ARRIVEONCOMMANDER,
        BOOSTER,
        BACKTOBASE,
        NONE
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius, wheelRadius;
    [SerializeField] float displacment, radio, wanderRange, futureMag;
    [SerializeField] float leaderBehindDist, separationRadius, maxSeparation, slowingRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public float hp, hpMax, boostSpeed;
    [SerializeField] int boostDamage, maxDamage;
    [SerializeField] public LayerMask whatIsEnemy, whatIsAlly;
    [SerializeField] LayerMask whatIsObs;
    #endregion

    #region //Member
    AgentState agentState;
    bool isBoosted;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> enemyPercibided, allyPercibed, itemPercibed;
    SteeringBehaviors sb;
    Rigidbody2D rb;

    #endregion

    #region //Public
    public float disLeader, maxSpeed, maxForce, rotateSpeed, boosterRadius;
    [HideInInspector] public GameObject targetAlly, targetEnemy;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        timeToDie = 0.5f;
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
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager()
    {
        enemyPerception();
        allyPerception();

    }
    private void enemyPerception()
    {
        bool isTarget = false;
        //Detectar agentes por medio de la vista 
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        enemyPercibided = new List<Collider2D>(viewAgents);
        //Detectar agentes por medio del oido
        Collider2D[] heardAgents = Physics2D.OverlapCircleAll(transform.position, heardRadius, whatIsEnemy);
        //Recorre la lista de collidersa para agregarlo a la litsa de enemigos percibidos 
        for (int i = 0; i < heardAgents.Length; i++)
        {
            enemyPercibided.Add(heardAgents[i]);
        }
        // Teenemos enemigo
        if (enemyPercibided != null)
        {
            //Buscar que tipo de enemigo es
            foreach (Collider2D enemy in enemyPercibided)
            {
                targetEnemy = enemy.gameObject;
                isTarget = true;


            }
            if (!isTarget)
            {
                targetEnemy = null;
            }

        }

    }
    private void allyPerception()
    {
        bool isTargetAlly = false;
        //Detectar agentes por medio de la vista 
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsAlly);
        allyPercibed = new List<Collider2D>(viewAgents);
        //Detectar agentes por medio del oido
        Collider2D[] heardAgents = Physics2D.OverlapCircleAll(transform.position, heardRadius, whatIsAlly);
        //Recorre la lista de collidersa para agregarlo a la litsa de enemigos percibidos 
        for (int i = 0; i < heardAgents.Length; i++)
        {
            allyPercibed.Add(heardAgents[i]);
        }
        //SI si estoy percibiendo 
        if (allyPercibed != null)
        {
            foreach (Collider2D ally in allyPercibed)
            {
                if (ally.CompareTag("Tank") || ally.CompareTag("Ninja") || ally.CompareTag("Commander")
                    || ally.CompareTag("Kamikaze") || ally.CompareTag("Archer") || ally.CompareTag("Magician") )
                {
                    targetAlly = ally.gameObject;
                    isTargetAlly = true;

                }

            }
            if (!isTargetAlly)
            {
                targetAlly = null;
            }

        }
    }
    void DecisionManager()
    {
        agentState = AgentState.NONE;
        if (targetAlly != null)
        {
            if (targetAlly.CompareTag("Tank") || targetAlly.CompareTag("Ninja") || targetAlly.CompareTag("Kamikaze"))
            {
                agentState = AgentState.BOOSTER;
            }
            if (targetAlly.CompareTag("Commander"))
            {
                agentState = AgentState.ARRIVEONCOMMANDER;
            }

        }
        else if (targetEnemy != null)
        {

            agentState = AgentState.ONRISK;
            isBoosted = false;

        }
        if (targetEnemy == null && targetAlly == null)
        {
            agentState = AgentState.NONE;
        }



    }
    void movementManager()
    {
        if (agentState == AgentState.NONE)
        {
            try
            {
                sb.Wander(gameObject, displacment, radio, wanderRange, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.ARRIVEONCOMMANDER)
        {
            try
            {
                sb.Arrival(targetAlly.transform.position, transform, maxSpeed, slowingRadius, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.BOOSTER)
        {

            try
            {
                sb.Pursuit(targetAlly, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }


        if (agentState == AgentState.ONRISK)
        {
            try
            {
                sb.Evade(targetEnemy, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }

    void ActionManager()
    {
        try
        {

            if (Vector3.Distance(transform.position, targetAlly.transform.position) < boosterRadius + 2.0f)
            {
                if (targetAlly.CompareTag("Tank") && !isBoosted)
                {
                    if (targetAlly.GetComponent<AgentTank>().hp < (targetAlly.GetComponent<AgentTank>().hpMax * .6))
                    {
                        isBoosted = true;
                        targetAlly.GetComponent<AgentTank>().hp = targetAlly.GetComponent<AgentTank>().hpMax;
                    }
                    else if (targetAlly.GetComponent<AgentTank>().hp == targetAlly.GetComponent<AgentTank>().hpMax)
                    {
                        isBoosted = true;
                        targetAlly.GetComponent<AgentTank>().damage += boostDamage;
                    }

                }
                if (targetAlly.CompareTag("Ninja") && !isBoosted)
                {

                    if (targetAlly.GetComponent<AgentNinja>().hp < (targetAlly.GetComponent<AgentNinja>().hpMax * .8))
                    {
                        isBoosted = true;
                        targetAlly.GetComponent<AgentNinja>().hp = targetAlly.GetComponent<AgentNinja>().hpMax;

                    }
                    else if (targetAlly.GetComponent<AgentNinja>().hp == targetAlly.GetComponent<AgentNinja>().hpMax)
                    {
                        isBoosted = true;
                        targetAlly.GetComponent<AgentNinja>().maxSpeed += boostSpeed;
                        Debug.Log(targetAlly.GetComponent<AgentNinja>().maxSpeed);
                    }

                }
                if (targetAlly.CompareTag("Commander"))
                {

                    if (targetAlly.GetComponent<AgentCommander>().hp < (targetAlly.GetComponent<AgentCommander>().hpMax * .8))
                    {
                        targetAlly.GetComponent<AgentCommander>().hp = targetAlly.GetComponent<AgentCommander>().hpMax;

                    }
                    else if (targetAlly.GetComponent<AgentCommander>().hp <= (targetAlly.GetComponent<AgentCommander>().hpMax * .2))
                    {
                        targetAlly.GetComponent<AgentCommander>().maxSpeed += boostSpeed;

                    }
                }
                if (targetAlly.CompareTag("Kamikaze"))
                {

                    targetAlly.GetComponent<AgentKamikaze>().damage += boostDamage;
                    if(targetAlly.GetComponent<AgentKamikaze>().damage >= maxDamage)
                    {
                        targetAlly.GetComponent<AgentKamikaze>().damage = maxDamage;
                    }
                   
                }
                if (targetAlly.CompareTag("Archer"))
                {

                    if (targetAlly.GetComponent<AgentArcher>().hp < (targetAlly.GetComponent<AgentArcher>().hpMax * .8))
                    {
                        targetAlly.GetComponent<AgentArcher>().hp = targetAlly.GetComponent<AgentArcher>().hpMax;

                    }
                    else if (targetAlly.GetComponent<AgentArcher>().hp <= (targetAlly.GetComponent<AgentArcher>().hpMax * .2))
                    {
                        targetAlly.GetComponent<AgentArcher>().maxSpeed += boostSpeed;

                    }
                }
                if (targetAlly.CompareTag("Magician"))
                {

                    if (targetAlly.GetComponent<AgentMagician>().hp < (targetAlly.GetComponent<AgentMagician>().hpMax * .8))
                    {
                        targetAlly.GetComponent<AgentMagician>().hp = targetAlly.GetComponent<AgentMagician>().hpMax;

                    }
                    else if (targetAlly.GetComponent<AgentMagician>().hp <= (targetAlly.GetComponent<AgentMagician>().hpMax * .2))
                    {
                        targetAlly.GetComponent<AgentMagician>().maxSpeed += boostSpeed;

                    }
                }
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
        Gizmos.DrawWireSphere(transform.position, boosterRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, heardRadius);
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