using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentNinja : MonoBehaviour
{
    enum AgentState
    {
        NONE,
        ONRISK,
        ATTACK
    }
    // Start is called before the first frame update
    #region //Serializable
    [SerializeField] float futureSight, futureSightRadius;
    [SerializeField] public LayerMask whatIsEnemy;
    [SerializeField] LayerMask whatIsObs;
    [SerializeField] public Transform baseEnemy;
    #endregion

    #region //Member
    AgentState agentState;
    float timeToDie;
    bool playAudio = false;
    List<Collider2D> attackTowers;
    List<Vector2> gridPath;
    SteeringBehaviors sb;
    PathFinding pf;
    GameObject target;
    Rigidbody2D rb;

    #endregion

    #region //Public
    public float maxSpeed, maxForce, rotateSpeed;
    public int hp, hpMax;
    public int damage, attackRadius;
    public GameObject healer;
    public AudioSource dieAudio;
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

        //Debug.Log("Ninja:" + hp);
    }

    //Todo lo qu tiene que ver con los sentidos, vista, oido, tacto
    void PerceptionManager()
    {
        bool isTarget = false;
        Collider2D[] towers = Physics2D.OverlapCircleAll(transform.position, attackRadius, whatIsEnemy);
        attackTowers = new List<Collider2D>(towers);
        if (attackTowers != null) { 
            foreach (Collider2D tmp in attackTowers)
            {
                target = tmp.gameObject;
                isTarget = true;
            }
            if (!isTarget) {
                target = null;
            }
        }


    }

    void DecisionManager(){
        if (target != null){
            agentState = AgentState.ATTACK;
            if (hp < 50 && !target.CompareTag("Tower")){
                agentState = AgentState.ONRISK;
            }
        }
        else {
            agentState = AgentState.NONE;
        }


    }
    void movementManager(){
        switch (agentState){
            case AgentState.ATTACK:
                try{
                    sb.Seek(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
                }
                catch { }
                break;
            case AgentState.ONRISK:
                try{
                    sb.Flee(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
                }
                catch
                { }
                break;
            case AgentState.NONE:
                try{
                    gridPath.Clear();
                    List<Node> tmp = new List<Node>(pf.GetCurrentPath());
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        gridPath.Add(tmp[i].wordlPosition);
                    }

                    sb.PathFollowing(gridPath.ToArray(), gameObject, maxSpeed, maxForce, rotateSpeed, rb);
                }
                catch { }
                break;
        }
        sb.ObstacleAvoidance(futureSight, gameObject, maxSpeed, maxForce, rotateSpeed, futureSightRadius, rb, whatIsObs);
    }

    void ActionManager(){
        try{
            if (Vector2.Distance(target.transform.position, transform.position) <= attackRadius){

                switch (target.tag){
                    case "Ninja":
                        target.GetComponent<AgentNinja>().hp -= damage;
                        break;
                    case "Tank":
                        target.GetComponent<AgentTank>().hp -= damage;
                        break;
                    case "Miner":
                        target.GetComponent<AgentMiner>().hp -= damage;
                        break;
                    case "Commander":
                        target.GetComponent<AgentCommander>().hp -= damage;
                        break;
                    case "Booster":
                        target.GetComponent<AgentBooster>().hp -= damage;
                        break;
                    case "Cold":
                        
                        target.GetComponent<AgentCold>().hp -= damage;
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
                    case "Archer":
                        target.GetComponent<AgentArcher>().hp -= damage;
                        break;
                    case "Kamikaze":
                        target.GetComponent<AgentKamikaze>().hp -= damage;
                        break;


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
        Gizmos.DrawWireSphere(transform.position, attackRadius);
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
        else if (collision.gameObject.CompareTag("Base") && collision.gameObject.layer != gameObject.layer)
        {
            collision.gameObject.GetComponent<BaseSpawn>().hp -= damage;
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
