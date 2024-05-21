using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AgentMonster : MonoBehaviour
{
    enum AgentState
    {
        ATTACK,
        DestroyTower,
        NONE
    }

    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float viewRadius;
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask whatIsEnemy;
    [SerializeField] LayerMask whatIsObs;
    [SerializeField] public Transform baseEnemy;
    #endregion

    #region //Member
    float timeToDie;
    bool playAudio = false;
    AgentState agentState;
    List<Collider2D> attackAgents;
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
    #endregion
    // Start is called before the first frame update
    void Start() {
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
        Collider2D[] viewAgents = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        attackAgents = new List<Collider2D>(viewAgents);

        // Hay obstáculos?
        if (attackAgents != null){
            foreach (Collider2D enemy in attackAgents){
                if (enemy.CompareTag("Ninja") && attackAgents.Count > 2) {
                    target = enemy.gameObject;
                    isTarget = true;
                }
                if(enemy.CompareTag("Tower") || enemy.CompareTag("Monster")){
                    target = enemy.gameObject;
                    isTarget = true;
                }
                if(enemy.CompareTag("Booster") || enemy.CompareTag("Healer")){
                    target = enemy.gameObject;
                    isTarget = true;
                }
                if (enemy.CompareTag("Skeleton") || enemy.CompareTag("Magician"))
                {
                    target = enemy.gameObject;
                    isTarget = true;
                }


            }
            if (!isTarget){
                target = null;
            }
        }
    }

    void DecisionManager() {
        if (target != null){
            switch (target.gameObject.tag){
                case "Monster":
                    agentState = AgentState.ATTACK;
                    break;
                case "Booster":
                    agentState = AgentState.ATTACK;
                    break;
                case "Healer":
                    agentState = AgentState.ATTACK;
                    break;
                case "Ninja":
                    agentState = AgentState.ATTACK;
                    break;
                case "Skeleton":
                    agentState = AgentState.ATTACK;
                    break;
                case "Magician":
                    agentState = AgentState.ATTACK;
                    break;
                case "Tower":
                    agentState = AgentState.DestroyTower;
                    break;
            }
        }
        if (target == null){
            agentState = AgentState.NONE;
        }

    }
    void movementManager() {
        if(agentState == AgentState.ATTACK){
            try{
               
                sb.Seek(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.DestroyTower){
            try{
                sb.Seek(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
            }
            catch { }
        }
        if (agentState == AgentState.NONE) {
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

        try
        {
            if (Vector2.Distance(target.transform.position, transform.position) <= attackRadius)
            {

                switch (target.tag)
                {
                    case "Ninja":
                        target.GetComponent<AgentNinja>().hp -= damage;
                        break;
                   
                    case "Booster":
                        target.GetComponent<AgentBooster>().hp -= damage;
                        break;
                    case "Monster":
                        target.GetComponent<AgentMonster>().hp -= damage;
                        break;
                    case "Skeleton":
                        target.GetComponent<AgentSkull>().hp -= damage;
                        break;
                    case "Magician":
                        target.GetComponent<AgentMagician>().hp -= damage;
                        break;
                   

                }
            }
        }
        catch { }

    }
    private void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if ((collision.gameObject.CompareTag("Magic") || collision.gameObject.CompareTag("Arrow")) && collision.gameObject.layer != this.gameObject.layer) {
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
