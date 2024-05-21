using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentKamikaze : MonoBehaviour
{
    enum AgentState
    {
        ATTACK,
        TRYTOEVADE,
        SEARCHFORBOOSTER,
        NONE
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius;
    [SerializeField] float futureSight, futureSightRadius, futureMag;
    [SerializeField] public LayerMask whatIsEnemy, whatIsAlly;
    [SerializeField] LayerMask whatIsObs;
    [SerializeField] public Transform enemyBase;
    #endregion

    #region //Member
    AgentState agentState;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> enemyPercibided, allyPercibed;
    SteeringBehaviors sb;
    List<Vector2> gridPath;
    PathFinding pf;
    Rigidbody2D rb;
    #endregion

    #region //Public
    public float  maxSpeed, maxForce, rotateSpeed, attackRadius;
    public float hp;
    public int damage;
    [HideInInspector] public GameObject targetAlly, targetEnemy;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start(){
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        pf = new PathFinding();
        pf.gridObject = GameObject.Find("A_");
        pf.grid = pf.gridObject.GetComponent<Grid>();
        gridPath = new List<Vector2>();
        agentState = AgentState.ATTACK;
    }
    void FixedUpdate() {
        PerceptionManager();
        movementManager();
    }
    // Update is called once per frame
    void Update(){
        pf.FindPath(transform.position, enemyBase.position);
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
        for (int i = 0; i < heardAgents.Length; i++){
            enemyPercibided.Add(heardAgents[i]);
        }
        // Teenemos enemigo
        if (enemyPercibided != null){
            //Buscar que tipo de enemigo es
            foreach (Collider2D enemy in enemyPercibided){
                if (enemy.CompareTag("Tower")){
                    try{ 
                        if(allyPercibed.Count <= 0){
                            targetEnemy = enemy.gameObject;
                        }
                    }
                    catch { }
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
            foreach (Collider2D ally in allyPercibed) {
                if (ally.CompareTag("Booster")){
                    targetAlly = ally.gameObject;
                    isTargetAlly = true;
                }
                if (ally.CompareTag("Ninja")){
                    targetAlly = ally.gameObject;
                    isTargetAlly = true;
                }
            }
            if (!isTargetAlly){
                targetAlly = null;
            }

        }
    }
    void DecisionManager(){
       // agentState = AgentState.ATTACK;
        if (targetAlly != null){
            if (targetAlly.CompareTag("Booster")){
                agentState = AgentState.SEARCHFORBOOSTER;
            }

            if (targetAlly.CompareTag("Ninja")){
                agentState = AgentState.TRYTOEVADE;
            }
        }
        else if (targetEnemy != null){
            if (targetEnemy.CompareTag("Tower")){
                agentState = AgentState.ATTACK;
            }
        }
        if (targetEnemy == null && targetAlly == null){
            //agentState = AgentState.ATTACK;
            agentState = AgentState.NONE;
        }
    }
    void movementManager(){
        if (agentState == AgentState.ATTACK){
            try{
                sb.Seek(enemyBase.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.SEARCHFORBOOSTER) {
            try{
                sb.Pursuit(targetAlly, transform, futureMag, maxSpeed, maxForce, rotateSpeed, rb);

            }
            catch { }
        }
        if (agentState == AgentState.TRYTOEVADE){
            try{
                sb.Flee(targetAlly.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
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
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }

    void ActionManager() {
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
            WaitForDie();
        }
        else if (collision.gameObject.CompareTag("Base") && collision.gameObject.layer != gameObject.layer)
        {
            collision.gameObject.GetComponent<BaseSpawn>().hp -= damage;
            WaitForDie();
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
