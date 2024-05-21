using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCold : MonoBehaviour
{
    enum AgentState
    {
        ATTACK,
        GETOUT,
        FINDHEALER,
        ENGAGE,
        NONE
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius;
    [SerializeField] float futureMag;
    [SerializeField] float slowingRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public float hp, hpMax, attackRadius;
    [SerializeField] public LayerMask whatIsEnemy, whatIsAlly;
    [SerializeField] LayerMask whatIsObs;
    [SerializeField] public Transform baseEnemy; 
    #endregion

    #region //Member
    AgentState agentState;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> enemyPercibided, allyPercibed, itemPercibed;
    SteeringBehaviors sb;
    Rigidbody2D rb;
    Vector2[] pathFollowing;



    #endregion

    #region //Public
    public int damage;
    public float maxSpeed, maxForce, rotateSpeed;
    [HideInInspector] public GameObject targetAlly, targetEnemy;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start(){
        timeToDie = 0.5f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        agentState = AgentState.NONE;
        pathFollowing = new Vector2[] { new Vector2(-6.49f, 0.7f), new Vector2(-6.49f, -1.15f), new Vector2(-6.49f, -3.65f)};

    }
    void FixedUpdate(){
        PerceptionManager();
        movementManager();
    }
    // Update is called once per frame
    void Update(){

        DecisionManager();
        ActionManager();
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager()
    {
        enemyPerception();
        allyPerception();

    }
    private void enemyPerception(){
        bool isTarget = false;
        //Detectar agentes por medio de la vista 
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        enemyPercibided = new List<Collider2D>(viewAgents);
        //Detectar agentes por medio del oido
        Collider2D[] heardAgents = Physics2D.OverlapCircleAll(transform.position, heardRadius, whatIsEnemy);
        //Recorre la lista de collidersa para agregarlo a la litsa de enemigos percibidos 
        for (int i = 0; i < heardAgents.Length; i++){
            enemyPercibided.Add(heardAgents[i]);
        }
        // Teenemos enemigo
        if (enemyPercibided != null){
            //Buscar que tipo de enemigo es
            foreach (Collider2D enemy in enemyPercibided)
            {
                
                targetEnemy = enemy.gameObject;
                isTarget = true;
              
            }
            if (!isTarget){
                targetEnemy = null;
            }
        }
    }
    private void allyPerception(){
        bool isTargetAlly = false;
        //Detectar agentes por medio de la vista 
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsAlly);
        allyPercibed = new List<Collider2D>(viewAgents);
        //Detectar agentes por medio del oido
        Collider2D[] heardAgents = Physics2D.OverlapCircleAll(transform.position, heardRadius, whatIsAlly);
        //Recorre la lista de collidersa para agregarlo a la litsa de enemigos percibidos 
        for (int i = 0; i < heardAgents.Length; i++){
            allyPercibed.Add(heardAgents[i]);
        }
        //SI si estoy percibiendo 
        if (allyPercibed != null){
            foreach (Collider2D ally in allyPercibed){
                targetAlly = ally.gameObject;
                isTargetAlly = true;
            }
            if (!isTargetAlly){
                targetAlly = null;
            }

        }
    }
    void DecisionManager()
    {
        agentState = AgentState.NONE;
        if (targetAlly != null){
            if (targetAlly.CompareTag("Tank") || targetAlly.CompareTag("Ninja")){ 
            
                if(targetEnemy != null){
                    agentState = AgentState.ATTACK;
                }
            }
            if (targetAlly.CompareTag("Healer")){
                agentState = AgentState.FINDHEALER;
            }
            if (targetAlly.CompareTag("Commander")) {
                agentState = AgentState.ENGAGE;
            }

        }
        else if (targetEnemy != null && targetAlly == null){
            agentState = AgentState.GETOUT;

        }
        if (targetEnemy == null && targetAlly == null){
            agentState = AgentState.NONE;
        }

    }
    void movementManager(){
        if (agentState == AgentState.NONE){
            try{
                sb.PathFollowing(pathFollowing, gameObject, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.ENGAGE){
            try{
                sb.Seek(baseEnemy.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);

            }
            catch { }
        }
        if (agentState == AgentState.FINDHEALER){
            try{
                sb.Pursuit(targetAlly, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }

        if (agentState == AgentState.ATTACK) {
            try{
                sb.Seek(targetEnemy.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.GETOUT){
            try {
                sb.Flee(targetEnemy.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }

    void ActionManager(){
        try
        {
            if (Vector2.Distance(targetEnemy.transform.position, transform.position) <= attackRadius)
            {

                switch (targetEnemy.tag)
                {
                    case "Ninja":
                        targetEnemy.GetComponent<AgentNinja>().hp -= damage;
                        break;
                    case "Tank":
                        targetEnemy.GetComponent<AgentTank>().hp -= damage;
                        break;
                    case "Miner":
                        targetEnemy.GetComponent<AgentMiner>().hp -= damage;
                        break;
                    case "Commander":
                        targetEnemy.GetComponent<AgentCommander>().hp -= damage;
                        break;
                    case "Booster":
                        targetEnemy.GetComponent<AgentBooster>().hp -= damage;
                        break;
                    case "Cold":
                        targetEnemy.GetComponent<AgentCold>().hp -= damage;
                        break;
                    case "Monster":
                        targetEnemy.GetComponent<AgentMonster>().hp -= damage;
                        break;
                    case "Skeleton":
                        targetEnemy.GetComponent<AgentSkull>().hp -= damage;
                        break;
                    case "Magician":
                        targetEnemy.GetComponent<AgentMagician>().hp -= damage;
                        break;
                    case "Archer":
                        targetEnemy.GetComponent<AgentArcher>().hp -= damage;
                        break;
                    case "Kamikaze":
                        targetEnemy.GetComponent<AgentKamikaze>().hp -= damage;
                        break;
                    case "Enrique":
                        break;

                }
            }
        }
        catch { }
        if (hp <= 0){
            WaitForDie();
        }

    }
    private void OnDrawGizmos(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, heardRadius);
    }
    private void OnTriggerEnter2D(Collider2D collision){
        if ((collision.gameObject.CompareTag("Magic") || collision.gameObject.CompareTag("Arrow")) && collision.gameObject.layer != this.gameObject.layer){
            hp -= collision.gameObject.GetComponent<MagicSpell>().damage;
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Tower"))
        {
            collision.gameObject.GetComponent<BaseEnemyScript>().hp -= damage;
        }
        else if (collision.gameObject.CompareTag("Base") && collision.gameObject.layer != gameObject.layer)
        {
            collision.gameObject.GetComponent<BaseSpawn>().hp -= damage;
        }
    }
    void WaitForDie(){
        if (!playAudio){
            dieAudio.Play();
            playAudio = true;
        }
        timeToDie -= Time.deltaTime;
        if (timeToDie <= 0) {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}