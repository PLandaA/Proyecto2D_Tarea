using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentMiner : MonoBehaviour
{
     enum AgentState
     {
        NONE,
        CRAFTER,
        ONRISK,
        BACKTOBASE
     }
    #region //Serializable
    [SerializeField] float crafterRadius, safeRadius;
    [SerializeField] public LayerMask  whatIsEnemy;
    #endregion

    #region //Member
    int resources = 0;
    int resourcesPlayer = 0;
    float timeToDie;
    bool playAudio = false;
    bool fullOfResources = false, nullResources = false;
    AgentState agentState;
    List<Collider2D> evadeTowers;
    Vector2[] pathFollowing;
    SteeringBehaviors sb;
    GameObject  target;
    Rigidbody2D rb;
    GameObject[] mine;
    GameObject  Gm, baseEnemyManager;
    #endregion

    #region //Public

    public float hp, hpMax;
    public float maxSpeed, maxForce, rotateSpeed, slowingRadius, futureMag;
    [HideInInspector] public bool isDead = false, right = false;
    public bool isEvil = false;
    public GameObject alliadeBase;
    public AudioSource dieAudio;
    #endregion
    // Start is called before the first frame update
    void Start(){
        timeToDie = 0.3f;
        if (isEvil){
            mine = GameObject.FindGameObjectsWithTag("EnemyMine");
            baseEnemyManager = GameObject.FindWithTag("Tower");
        }
        else {
            mine = GameObject.FindGameObjectsWithTag("Mine");
            Gm = GameObject.FindWithTag("GameManager");
        }
        if (right){
            resources = mine[0].GetComponent<Mines>().mineHealth;
        }
        else {
            resources = mine[1].GetComponent<Mines>().mineHealth;
        }
        sb = new SteeringBehaviors();
        rb = GetComponent<Rigidbody2D>();
        agentState = AgentState.NONE;

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
    void PerceptionManager(){
        bool isTarget = false;
        Collider2D[] towers = Physics2D.OverlapCircleAll(transform.position, safeRadius, whatIsEnemy);
        evadeTowers = new List<Collider2D>(towers);

        // Hay obstáculos?
        if (evadeTowers != null){
            foreach (Collider2D tmp in evadeTowers){
                target = tmp.gameObject;
                isTarget = true;
            }
            if (!isTarget){
                target = null;
            }
        }


    }

    void DecisionManager() {
        
        if (target != null){
            
              agentState = AgentState.ONRISK;    
            
            // Preguntar si es aliado o enemigo - >
            // Aliado -> target.aganteType == AgentType.Healer -> agentState = AgentState.GETHEAL
        }
        else{
            agentState = AgentState.NONE;
        }
        if (nullResources){
            agentState = AgentState.CRAFTER;
        }
        if (fullOfResources){
            agentState = AgentState.BACKTOBASE;
        }
 

    }
    void movementManager(){
        switch (agentState){
            case AgentState.ONRISK:
                try{
                    sb.Flee(target.transform.position, transform, maxSpeed, maxForce, rotateSpeed, rb);
                }
                catch { }
                break;
            case AgentState.BACKTOBASE:
                try{
                    sb.Arrival(alliadeBase.transform.position, transform, maxSpeed,slowingRadius, maxForce, rotateSpeed, rb);
                }
                catch { }
                break;
            case AgentState.NONE:
                try{
                    if (right){
                        sb.Arrival(mine[0].transform.position, transform, maxSpeed, slowingRadius, maxForce, rotateSpeed, rb);
                    }
                    else{
                        sb.Arrival(mine[1].transform.position, transform, maxSpeed, slowingRadius, maxForce, rotateSpeed, rb);
                    }
                }
                catch { }
                break;
        }

    }

    void ActionManager(){
        try{
            if (right) {
                if (mine[0].GetComponent<Mines>().isTouching) {
                    
                    nullResources = true;
                }
            }
            else {
                if (mine[1].GetComponent<Mines>().isTouching){
                    nullResources = true;
                }
            }
        }
        catch { }
        if (agentState == AgentState.CRAFTER){
            resources -= 1;
            resourcesPlayer++;
        }
        if (resources <= 0){
            resources = 0;
            if (right){
                mine[0].GetComponent<Mines>().isTouching = false;
            }
           else{
                mine[1].GetComponent<Mines>().isTouching = false;
            }
            nullResources = false;
            fullOfResources = true;
        }
        if(Vector2.Distance(transform.position,alliadeBase.transform.position) <= 4f && fullOfResources){
            if (isEvil) { 
                baseEnemyManager.GetComponent<BaseEnemyScript>().resources += resourcesPlayer;
            }
            else{
                Gm.GetComponent<GameManager>().resources += resourcesPlayer;
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        if (hp <= 0){
            WaitForDie();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, crafterRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, safeRadius);
        Gizmos.color = Color.cyan;
      
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


