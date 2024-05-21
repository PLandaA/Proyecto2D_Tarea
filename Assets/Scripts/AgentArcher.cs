using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentArcher : MonoBehaviour
{
    enum AgentState
    {
        ATTACK,
        RETURNTOBASE,
        FINDBOOSTER,
        STAYWITHALLY,
        FINDHEALER,
        NONE
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius;
    [SerializeField] float futureMag, leaderBehindDist;
    [SerializeField] float slowingRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask whatIsEnemy;
    [SerializeField] LayerMask whatIsAlly, whatIsObs;
    [SerializeField] Transform baseAlly;
    [SerializeField] GameObject proyectile;

    #endregion

    #region //Member
    AgentState agentState;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> enemyPercibided, allyPercibed;
    SteeringBehaviors sb;
    Rigidbody2D rb;
    Vector2[] pathFollowing;

    float timeToShoot;

    #endregion

    #region //Public
    public float maxSpeed, maxForce, rotateSpeed, attackRadius;
    public float hp, hpMax, shootCooldown;
    public int damage;
    [HideInInspector] public GameObject targetAlly, targetEnemy;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        agentState = AgentState.NONE;
        pathFollowing = new Vector2[] { new Vector2(5.74f, -3.47f), new Vector2(5.74f, -0.36f), new Vector2(5.74f, 2.22f) };
        timeToShoot = shootCooldown;
        shootCooldown = 0;

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

                if (enemy.CompareTag("Ninja")) {
                    targetEnemy = enemy.gameObject;
                    isTarget = true;
                }

                if (enemy.CompareTag("Tower")){
                    if (allyPercibed.Count <= 1)
                    {
                        targetEnemy = enemy.gameObject;
                        isTarget = true;

                    }
                }
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
                if (ally.gameObject != gameObject)
                {
                    if (ally.CompareTag("Commander"))
                    {
                        targetAlly = ally.gameObject;
                        isTargetAlly = true;
                    }
                    if (ally.CompareTag("Healer"))
                    {
                        if (hp < (hpMax * .6))
                        {
                            targetAlly = ally.gameObject;
                            isTargetAlly = true;
                        }
                    }
                    if (ally.CompareTag("Booster"))
                    {

                        if (hp < (hpMax * .9))
                        {

                            targetAlly = ally.gameObject;
                            isTargetAlly = true;
                        }

                    }
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
            if (targetAlly.CompareTag("Commander"))
            {
                agentState = AgentState.STAYWITHALLY;
            }
            if (targetAlly.CompareTag("Healer"))
            {
                agentState = AgentState.FINDHEALER;
            }
            if (targetAlly.CompareTag("Booster"))
            {
                agentState = AgentState.FINDBOOSTER;
            }

        }
        else if (targetEnemy != null)
        {

            if (targetEnemy.CompareTag("Ninja"))
            {
                agentState = AgentState.ATTACK;
            }
            if (targetEnemy.CompareTag("Tower"))
            {
                Debug.Log("ola");
                agentState = AgentState.ATTACK;
            }
        }
        if (targetEnemy == null && targetAlly == null)
        {
            agentState = AgentState.NONE;
        }

        if (hp < (hpMax * .1))
        {
            agentState = AgentState.RETURNTOBASE;
        }



    }
    void movementManager()
    {
        if (agentState == AgentState.NONE)
        {
            try
            {
                sb.PathFollowing(pathFollowing, gameObject, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.ATTACK)
        {

            try
            {
                sb.Arrival(targetEnemy.transform.position, transform, slowingRadius, maxSpeed, maxForce, rotateSpeed, rb);
                Shoot();
            }
            catch { }

            


        }
        if (agentState == AgentState.FINDBOOSTER)
        {
            try
            {
                sb.Pursuit(targetAlly, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.FINDHEALER)
        {
            try
            {
                sb.Pursuit(targetAlly, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.STAYWITHALLY)
        {
            try
            {
                sb.LeaderFollowing(gameObject, targetAlly, targetAlly.GetComponent<AgentCommander>().followersList, leaderBehindDist, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.RETURNTOBASE)
        {
            try
            {
                sb.Seek(baseAlly.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }

    void ActionManager()
    {
            if (shootCooldown >= 0)
            {
                shootCooldown -= Time.deltaTime;
            }
 
        if (hp <= 0)
        {
          WaitForDie();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
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

    void Shoot()
    {
        if (shootCooldown <= 0)
        {
            GameObject tmp = Instantiate(proyectile, transform.position, transform.rotation);
            tmp.layer = this.gameObject.layer;
            tmp.GetComponent<MagicSpell>().target = targetEnemy.transform.position;
            shootCooldown = timeToShoot;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "Magic" || collision.gameObject.tag == "Arrow") && collision.gameObject.layer != this.gameObject.layer)
        {

            hp -= collision.gameObject.GetComponent<MagicSpell>().damage;
        }
    }
}
