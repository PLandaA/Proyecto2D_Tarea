using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AgentSkull : MonoBehaviour
{
    enum AgentState
    {
        ATTACK,
        DESTROYTOWER,
        FOLLOWALLY,
        ONRISK,
        NONE
    }

    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] float leaderBehindDist, separationRadius, maxSeparation;
    [SerializeField] public LayerMask whatIsEnemy, whatIsObs;
    [SerializeField] public LayerMask whatIsAlly;
    [SerializeField] public Transform baseEnemy;
    #endregion

    #region //Member
    float timeToDie;
    bool playAudio = false;
    AgentState agentState;
    List<Collider2D> attackAgents, allyPercibed;
    SteeringBehaviors sb;
    GameObject EnemyTarget, AllyTarget;
    Rigidbody2D rb;
    List<GameObject> followersList;
    List<Vector2> gridPath;
    PathFinding pf;
    #endregion

    #region //Public
    public float disLeader, maxSpeed, maxForce, slowingRadius, rotateSpeed;
    public int hp, hpMax;
    public int damage;
    public AudioSource dieAudio;
    public bool isLeader;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        pf = new PathFinding();
        pf.gridObject = GameObject.Find("A_");
        pf.grid = pf.gridObject.GetComponent<Grid>();
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        followersList = new List<GameObject>();
        gridPath = new List<Vector2>();
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
        pf.FindPath(transform.position, baseEnemy.position);
        DecisionManager();
        ActionManager();
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager()
    {
        EnemyPerception();
        AllyPerception();
    }
    private void EnemyPerception()
    {
        bool isTarget = false;
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        attackAgents = new List<Collider2D>(viewAgents);

        // Hay obstáculos?
        if (attackAgents != null){
            foreach (Collider2D enemy in attackAgents){
                if (enemy.CompareTag("Tower")){
                    EnemyTarget = enemy.gameObject;
                    isTarget = true;
                }
                else{
                    EnemyTarget = enemy.gameObject;
                    isTarget = true;
                }
            }
            if (!isTarget) { 
                EnemyTarget = null;
            }
        }
    }

    private void AllyPerception()
    {
        bool isTargetAlly = false;
        //Detectar agentes por medio de la vista 
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsAlly);
        allyPercibed = new List<Collider2D>(viewAgents);
        //Detectar agentes por medio del oido
        Collider2D[] heardAgents = Physics2D.OverlapCircleAll(transform.position, heardRadius, whatIsAlly);
        //Recorre la lista de collidersa para agregarlo a la litsa de aliados percibidos 
        for (int i = 0; i < heardAgents.Length; i++){
            allyPercibed.Add(heardAgents[i]);
        }
        //SI si estoy percibiendo 
        if (allyPercibed != null){
            foreach (Collider2D ally in allyPercibed){
                if (ally.CompareTag("Skeleton")){
                    if (ally.gameObject != gameObject && ally.GetComponent<AgentSkull>().isLeader){
                        AllyTarget = ally.gameObject;
                        isTargetAlly = true;
                    }
                }
            }
            if (!isTargetAlly){
                AllyTarget = null;
            }
        }  
    }

    void DecisionManager()
    {
        if (EnemyTarget != null){
            if (EnemyTarget.tag == "Tower"){
                agentState = AgentState.DESTROYTOWER;
            }
            else{
                agentState = AgentState.ATTACK;
                if (hp < (hpMax * .3))
                {
                    agentState = AgentState.ONRISK;
                }
            }
        }
        else if (AllyTarget != null){
            agentState = AgentState.FOLLOWALLY;
        }

        if (EnemyTarget == null && AllyTarget == null){
            agentState = AgentState.NONE;
        }

       /* if (hp < (hpMax * .3) && EnemyTarget != null){
            agentState = AgentState.ONRISK;
        }*/
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
            try{
                sb.Seek(EnemyTarget.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.DESTROYTOWER){
            try{
                
                sb.Seek(EnemyTarget.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.FOLLOWALLY){
            try{
                sb.LeaderFollowing(gameObject, AllyTarget, AllyTarget.GetComponent<AgentSkull>().followersList, leaderBehindDist, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        else {
            try{
                if (AllyTarget.GetComponent<AgentSkull>().followersList.Contains(this.gameObject)){
                    AllyTarget.GetComponent<AgentSkull>().followersList.Remove(this.gameObject);
                }
            }
            catch{ }
        }
        if (agentState == AgentState.ONRISK){
            try{
                
                sb.Flee(EnemyTarget.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);

    }

    void ActionManager(){

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
        if(collision.gameObject.CompareTag("Ninja") && collision.gameObject.layer != gameObject.layer)
        {
            collision.gameObject.GetComponent<AgentNinja>().hp -= damage;
        }
        if (collision.gameObject.CompareTag("Skeleton") && collision.gameObject.layer != gameObject.layer)
        {
            collision.gameObject.GetComponent<AgentSkull>().hp -= damage;
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
