using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentCommander : MonoBehaviour
{
    enum AgentState
    {
        ONRISK,
        ATTACK,
        ARRIVAL,
        GOTOHEALER,
        SAVEMINER,
        NONE
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius, wheelRadius;
    [SerializeField] float displacment, radio, wanderRange, futureMag;
    [SerializeField] float leaderBehindDist, separationRadius, maxSeparation, slowingRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask whatIsEnemy, whatIsAlly;
    [SerializeField] LayerMask whatIsItem, whatIsObs;
    [SerializeField] public Transform baseAlly, baseEnemy;
    #endregion

    #region //Member
    AgentState agentState; 
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> enemyPercibided, allyPercibed, itemPercibed;
    List<Vector2> gridPath;
    SteeringBehaviors sb;
    public List<GameObject> followersList;

    PathFinding pf;
    Rigidbody2D rb;
    #endregion

    #region //Public
    public float disLeader, maxSpeed, maxForce, rotateSpeed, attackRadius;
    public float hp, hpMax;
    public int damage;
    [HideInInspector] public GameObject targetAlly, targetEnemy;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start(){
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        pf = new PathFinding();
        pf.gridObject = GameObject.Find("A_");
        pf.grid = pf.gridObject.GetComponent<Grid>();
        rb = GetComponent<Rigidbody2D>();
        gridPath = new List<Vector2>();
        agentState = AgentState.NONE;
        followersList = new List<GameObject>();
    }
    void FixedUpdate(){
        PerceptionManager();
        movementManager();
    }
    // Update is called once per frame
    void Update(){
        //Que solo se llame cuando necesite buscar camino
        pf.FindPath(transform.position, baseEnemy.position);
        DecisionManager();
        ActionManager();
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager(){
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
        for (int i = 0; i < heardAgents.Length; i++) { 
            enemyPercibided.Add(heardAgents[i]);
        }
        // Teenemos enemigo
        if (enemyPercibided != null){
            //Buscar que tipo de enemigo es
            foreach (Collider2D enemy in enemyPercibided){
                if (enemy.CompareTag("Tank")){
                    if (enemy.GetComponent<AgentTank>().hp < hp){
                        if (enemy.GetComponent<AgentTank>().damage < damage){
                            targetEnemy = enemy.gameObject;
                            isTarget = true;
                        }
                    }
                }
                if (enemy.CompareTag("Ninja")){
                    
                    //Tengo mas vida que el ninja
                    if (enemy.GetComponent<AgentNinja>().hp < hp) {
                        //Tengo barrio que me respalde
                        if (allyPercibed.Count > 0 && enemyPercibided.Count < allyPercibed.Count){
                            targetEnemy = enemy.gameObject;
                            isTarget = true;
                        }
                    }
                }
                if (enemy.CompareTag("Miner"))
                {
                    targetEnemy = enemy.gameObject;
                    isTarget = true;
                }
                if (enemy.CompareTag("Tower") || enemy.CompareTag("Magician") || enemy.CompareTag("Archer"))
                {
                    targetEnemy = enemy.gameObject;
                }
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
                if (ally.CompareTag("Tank")){
                    targetAlly = ally.gameObject;
                    isTargetAlly = true;
                }
                if (ally.CompareTag("Healer")){
                    if (hp < (hpMax * .6)){
                        targetAlly = ally.gameObject;
                        isTargetAlly = true;
                    }
                }
                if (ally.CompareTag("Miner")){
                    float distance = Vector3.Distance(transform.position, ally.transform.position);
                    if (ally.GetComponent<AgentMiner>().hp < (ally.GetComponent<AgentMiner>().hpMax * .8) &&
                        distance < distance / 2){
                        targetAlly = ally.gameObject;
                        isTargetAlly = true;
                    }
                }
            }
            if (!isTargetAlly){
                targetAlly = null;
            }

        }
    }
    void DecisionManager(){
        if (targetAlly != null){
            if (targetAlly.CompareTag("Tank")){
                agentState = AgentState.ARRIVAL;
            }
            if (targetAlly.CompareTag("Healer")){
                agentState = AgentState.GOTOHEALER;
            }
            if (targetAlly.CompareTag("Miner")){
                agentState = AgentState.SAVEMINER;
            }
        }
        else if (targetEnemy != null){

            if (targetEnemy.CompareTag("Ninja")){
                agentState = AgentState.ATTACK;
            }
            if (targetEnemy.CompareTag("Tower")){
                agentState = AgentState.ATTACK;
            }
        }
        if (targetEnemy == null && targetAlly == null ){
            agentState = AgentState.NONE;
        }

        if (hp < (hpMax * .3) && targetEnemy != null){
            agentState = AgentState.ONRISK;
        }



    }
    void movementManager(){
        if (agentState == AgentState.NONE){
            try{
                gridPath.Clear();
                List<Node> tmp = new List<Node>(pf.GetCurrentPath());

                for (int i = 0; i < tmp.Count; i++){
                    gridPath.Add(tmp[i].wordlPosition);
                }
                sb.PathFollowing(gridPath.ToArray(), gameObject, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.ATTACK){
            try {
                sb.Seek(targetEnemy.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.ARRIVAL)
        {
            try{
                sb.Arrival(targetEnemy.transform.position, transform, maxSpeed,slowingRadius, maxForce, rotateSpeed, rb);    
            }
            catch { }
        }
        if (agentState == AgentState.GOTOHEALER){
            try {
                sb.Arrival(targetAlly.transform.position, transform, slowingRadius, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.SAVEMINER){
            try{
                sb.Pursuit(targetAlly, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.ONRISK){
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
                        break;;
                    case "Magician":
                        targetEnemy.GetComponent<AgentMagician>().hp -= damage;
                        break;
                    case "Archer":
                        targetEnemy.GetComponent<AgentArcher>().hp -= damage;
                        break;
                }
            }
        }
        catch { }
        if (hp <= 0){
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
    private void OnTriggerEnter2D(Collider2D collision) {
        if ((collision.gameObject.CompareTag("Magic") || collision.gameObject.CompareTag("Arrow")) &&
             collision.gameObject.layer != this.gameObject.layer){
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
        if (timeToDie <= 0){
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
