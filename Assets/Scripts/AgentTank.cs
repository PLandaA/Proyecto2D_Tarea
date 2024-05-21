using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AgentTank : MonoBehaviour
{
    enum AgentState
    {
        ONRISK,
        ATTACK,
        NONE
    }

    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask  whatIsObs;
    [SerializeField] public Transform baseEnemy;
    #endregion

    #region //Member
    float timeToDie;
    bool playAudio = false;
    AgentState agentState;
    List<Collider2D> attackTowers;
    List<Vector2> gridPath;
    SteeringBehaviors sb;
    PathFinding pf;
    GameObject target;
    Rigidbody2D rb;
    #endregion

    #region //Public
    public float disLeader,maxSpeed, maxForce, slowingRadius, rotateSpeed, attackRadius;
    public int hp, hpMax;
    public int damage;
    public GameObject healer;
    public AudioSource dieAudio;
    public LayerMask whatIsTower;
    #endregion
    // Start is called before the first frame update
    void Start(){
        pf = new PathFinding();
        pf.gridObject = GameObject.Find("A_");
        pf.grid = pf.gridObject.GetComponent<Grid>();
        timeToDie = 0.3f;
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        gridPath = new List<Vector2>();
        agentState = AgentState.NONE;
    }
    void FixedUpdate(){
        PerceptionManager();
        movementManager();
    }
    // Update is called once per frame
    void Update(){
        pf.FindPath(transform.position, baseEnemy.position);
        DecisionManager();
        ActionManager();
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager(){
        bool isTarget = false;
        Collider2D[] towers = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsTower);
        attackTowers = new List<Collider2D>(towers);

        // Hay obstáculos?
        if (attackTowers != null) {
            foreach (Collider2D tmp in attackTowers){
                target = tmp.gameObject;
                isTarget = true;
            }
            if (!isTarget){
                target = null;
            }
        }
    }

    void DecisionManager(){
        if (target != null){
            if (target.gameObject.tag == "Tower"){
                agentState = AgentState.ATTACK;
            }
        }
        else{
            agentState = AgentState.NONE;
        }
    }
    void movementManager(){
        if(agentState == AgentState.ATTACK){
            try{
                sb.Seek(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if(agentState == AgentState.NONE) {
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

    void ActionManager(){
        if (healer.GetComponent<AgentHealer>().heal){
            hp += healer.GetComponent<AgentHealer>().healerPoints;
        }
        if (hp <= 0){
            WaitForDie();
        }

    }
    private void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
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

        if(collision.gameObject.CompareTag("Tank") && collision.gameObject.layer != gameObject.layer) {
            collision.gameObject.GetComponent<AgentTank>().hp -= damage;
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
