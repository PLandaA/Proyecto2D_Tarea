using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentConverter : MonoBehaviour
{
    enum AgentState
    {
        FOLLOW,
        CONVERTING,
        NONE
    }

    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, futureMag;
    [SerializeField] float displacement, radio, wanderRange;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask whatIsObs;
    #endregion

    #region //Member
    float timeToConvert;
    float timeToDie;
    bool playAudio = false;
    AgentState agentState;
    List<Collider2D> attackEnemies;
    SteeringBehaviors sb;
    GameObject target;
    Rigidbody2D rb;
    #endregion

    #region //Public
    public Transform baseEnemy ,baseAlly;
    public float convertTimeCooldown;
    public float maxSpeed, maxForce, slowingRadius, rotateSpeed;
    public int hp, hpMax;
    public int damage;
    public AudioSource dieAudio;
    public LayerMask whatIsEnemy,whatIsAlly;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        agentState = AgentState.NONE;
        timeToConvert = convertTimeCooldown;
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
        bool isTarget = false;
        Collider2D[] towers = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        attackEnemies = new List<Collider2D>(towers);

        // Hay obstáculos?
        if (attackEnemies != null)
        {
            foreach (Collider2D tmp in attackEnemies)
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
            if ((target.transform.position - transform.position).magnitude > slowingRadius)
                agentState = AgentState.FOLLOW;
            else
                agentState = AgentState.CONVERTING;
        }
        else
        {
            agentState = AgentState.NONE;
        }

    }
    void movementManager()
    {
        if (agentState == AgentState.FOLLOW)
        {
            try
            {
                sb.Pursuit(target, transform,futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.NONE)
        {
            try
            {
                sb.Wander(gameObject, displacement, radio, wanderRange, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if(agentState == AgentState.CONVERTING)
        {
            try
            {
                sb.Arrival(target.transform.position, transform, maxSpeed, slowingRadius, maxForce, rotateSpeed, rb);
            }
            catch
            {}
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }

    void ActionManager()
    {
        if (agentState == AgentState.CONVERTING)
        {
            convertTimeCooldown -= Time.deltaTime;
            if (convertTimeCooldown <= 0)
            {
                target.layer = gameObject.layer;
                target.GetComponent<SpriteRenderer>().color = gameObject.GetComponent<SpriteRenderer>().color;

                switch (target.tag)
                {
                    case "Ninja":
                        target.GetComponent<AgentNinja>().whatIsEnemy = whatIsEnemy;
                        target.GetComponent<AgentNinja>().baseEnemy = baseEnemy;
                        break;
                    case "Tank":
                        target.GetComponent<AgentTank>().baseEnemy = baseEnemy;
                        target.GetComponent<AgentTank>().whatIsTower = whatIsEnemy;
                        break;
                    case "Commander":
                        target.GetComponent<AgentCommander>().whatIsEnemy = whatIsEnemy;
                        target.GetComponent<AgentCommander>().baseEnemy = baseEnemy;
                        target.GetComponent<AgentCommander>().baseAlly = baseAlly;
                        break;
                    case "Booster":
                        target.GetComponent<AgentBooster>().whatIsAlly = whatIsAlly;
                        target.GetComponent<AgentBooster>().whatIsEnemy = whatIsEnemy;

                        break;
                    case "Cold":

                        target.GetComponent<AgentCold>().whatIsAlly = whatIsAlly;
                        target.GetComponent<AgentCold>().whatIsEnemy = whatIsEnemy;
                        break;
                    case "Monster":
                        target.GetComponent<AgentMonster>().baseEnemy = baseEnemy;
                        target.GetComponent<AgentMonster>().whatIsEnemy = whatIsEnemy;
                        break;
                    case "Skeleton":
                        target.GetComponent<AgentSkull>().whatIsEnemy = whatIsEnemy;
                        target.GetComponent<AgentSkull>().baseEnemy = baseEnemy;
                        break;
                    case "Magician":
                        target.GetComponent<AgentMagician>().baseEnemy = baseEnemy;
                        target.GetComponent<AgentMagician>().whatIsAlly = whatIsAlly;
                        target.GetComponent<AgentMagician>().whatIsEnemy = whatIsEnemy;
                        break;
                    case "Archer":
                        target.GetComponent<AgentArcher>().whatIsEnemy = whatIsEnemy;
                        break;
                    case "Kamikaze":
                        target.GetComponent<AgentKamikaze>().enemyBase = baseEnemy;
                        target.GetComponent<AgentKamikaze>().whatIsAlly = whatIsAlly;
                        target.GetComponent<AgentKamikaze>().whatIsEnemy = whatIsEnemy;
                        break;
                }
                convertTimeCooldown = timeToConvert;
            }
        }
        else
        {
            convertTimeCooldown = timeToConvert;
        }
        if (hp <= 0)
        {
            WaitForDie();
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slowingRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("Magic") || collision.gameObject.CompareTag("Arrow")) && collision.gameObject.layer != this.gameObject.layer)
        {
            hp -= collision.gameObject.GetComponent<MagicSpell>().damage;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Tower"))
        {
            collision.gameObject.GetComponent<BaseEnemyScript>().hp -= damage;
        }
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

