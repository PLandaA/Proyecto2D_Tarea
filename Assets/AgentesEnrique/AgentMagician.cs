using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMagician : MonoBehaviour
{
    enum AgentState
    {
        NONE,
        ATTACKING,
        DISTANCING,

    }
    #region //Serializable
    [SerializeField] float viewRadius, heardRadius;
    [SerializeField] float displacment, radio, futureMag;
    [SerializeField] float slowingRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask whatIsEnemy, whatIsAlly, whatIsTower;
    [SerializeField] LayerMask whatIsObs;
    [SerializeField] public Transform baseEnemy;
    [SerializeField] GameObject proyectile;
    #endregion
    #region //Member
    AgentState agentState;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> enemyPercibided;
    SteeringBehaviors sb;
    List<Vector2> gridPath;
    PathFinding pf;
    Rigidbody2D rb;
    float timeToShoot;
    #endregion
    #region //Public
    public float maxSpeed, maxForce, rotateSpeed, attackRadius, distanceRadius;
    public float hp, hpMax, shootCooldown;
    [HideInInspector] public GameObject target;
    public AudioSource dieAudio;
    #endregion
    void Start()
    {
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        pf = new PathFinding();
        pf.gridObject = GameObject.Find("A_");
        pf.grid = pf.gridObject.GetComponent<Grid>();
        gridPath = new List<Vector2>();
        agentState = AgentState.NONE;
        timeToShoot = shootCooldown;
        shootCooldown = 0;
    }

    void Update(){
        pf.FindPath(transform.position, baseEnemy.position);
        DecisionManager();
        ActionManager();
    }
    void FixedUpdate(){
        PerceptionManager();
        movementManager();
    }
    void PerceptionManager(){
        bool isTarget = false;
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        enemyPercibided = new List<Collider2D>(viewAgents);
        Collider2D[] heardAgents = Physics2D.OverlapCircleAll(transform.position, heardRadius, whatIsEnemy);
        enemyPercibided.AddRange(heardAgents);
        Collider2D[] towers = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsTower);
        enemyPercibided.AddRange(towers);
        if (enemyPercibided.Count != 0){
            foreach(Collider2D posibleTarget in enemyPercibided) {
                if (posibleTarget.CompareTag("Tower")){
                    target = posibleTarget.gameObject;
                    isTarget = true;
                    break;
                }
                else{
                    target = posibleTarget.gameObject;
                    isTarget = true;
                }
            }
            if (!isTarget){
                target = null;
            }
        }
    }
    void DecisionManager(){
        agentState = AgentState.NONE;
        if (target){
            if((target.transform.position - transform.position).magnitude <= attackRadius && (target.transform.position - transform.position).magnitude >= distanceRadius){
                agentState = AgentState.ATTACKING;
            }
            else{
                agentState = AgentState.DISTANCING;
            }
        }
    }
    void movementManager()
    {
        if(agentState == AgentState.NONE){
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
        else if(agentState == AgentState.ATTACKING){
            Shoot();
        }
        else if(agentState == AgentState.DISTANCING){
            Vector3 attackIn = (target.transform.position - ((target.transform.position - transform.position).normalized * attackRadius)) - transform.position;
            attackIn += transform.position;
            Debug.DrawLine(transform.position, attackIn);
            sb.Seek(attackIn, transform, maxSpeed, maxForce, rotateSpeed, rb);
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }
    void ActionManager()
    {
        if(shootCooldown >= 0){
            shootCooldown -= Time.deltaTime;
        }
        if (hp <= 0){
            WaitForDie();
        }
    }
    void Shoot(){
        if(shootCooldown <= 0){
            GameObject tmp = Instantiate(proyectile, transform.position, transform.rotation);
            tmp.layer = this.gameObject.layer;
            tmp.GetComponent<MagicSpell>().target = target.transform.position;
            shootCooldown = timeToShoot;
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, heardRadius);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, distanceRadius);
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
    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.tag == "Magic" && collision.gameObject.layer != this.gameObject.layer){
            hp -= collision.gameObject.GetComponent<MagicSpell>().damage;
        }
    }
}
