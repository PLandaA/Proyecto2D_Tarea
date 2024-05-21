using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyScript : MonoBehaviour
{
    #region //Serializable
    [SerializeField] float viewRadius;
    [SerializeField] LayerMask whatIsEnemy, whatIsAlly, whatIsTower, wahtIsObs;
    [SerializeField] public int resources = 0;
    #endregion

    #region //Public
    public int hp, hpMax;
    public float checkObjectivesCooldown;
    public GameObject agentEvilMiner, agentEvilNinja, agentEvilTank, agentEvilArcher, agentEvilHealer,
                      agentEvilBooster, agentEvilCommander, agentEvilMagician, agentEvilKamikaze,
                      agentEvilCold, agentEvilSkull, agentEvilMonster;
    public Transform EvilHome;
    [HideInInspector] public bool isDamage = false, isLeft = false, isInstiantiate = false;
    public AudioSource dieAudio;
    #endregion

    #region //Member
    float timeToDie, count;
    bool playAudio = false;
    List<Collider2D> enemies, allys;
    List<string> allysGO;
    GameObject target;
    float dangerAgents;
    float inocentAgets;
    float allysDangerAgents;
    float allysInocentAgets;
    float timeToCheckObjectives;
    ushort layer = 0;
    string cases = "";

    #endregion


    void Start()
    {
        timeToDie = 0.5f;
        timeToCheckObjectives = checkObjectivesCooldown;
        count = 2.0f;
        
    }

    void FixedUpdate()
    {
        PerceptionManager();
    }
    void Update()
    {
        Debug.Log(cases);
        DesitionManager();
        if (hp <= 0)
        {
            WaitForDie();
        }
        if (isInstiantiate)
        {
            CoolDownInstantiate();
        }
        if (checkObjectivesCooldown <= 0)
        {
            ObjectiveManager();
            checkObjectivesCooldown = timeToCheckObjectives;
         
        }
        else
        {
            checkObjectivesCooldown -= Time.deltaTime;
        }
        
    }
    void PerceptionManager()
    {
        allysDangerAgents = 0;
        allysInocentAgets = 0;
        dangerAgents = 0;
        inocentAgets = 0;
        //bool isTarget = false;
        Collider2D[] Enemies = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsEnemy);
        enemies = new List<Collider2D>(Enemies);
        Collider2D[] allysPercibed = Physics2D.OverlapCircleAll(transform.position, viewRadius, whatIsAlly);
        allys = new List<Collider2D>(allysPercibed);
        allysGO = new List<string>();
        if (enemies.Contains(GameObject.FindGameObjectWithTag("Base").GetComponent<Collider2D>()))
        {
            enemies.Remove(GameObject.FindGameObjectWithTag("Base").GetComponent<Collider2D>());
        }
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.tag != "Base")
            {
                if (enemy.tag == "Miner" || enemy.tag == "Cold" || enemy.tag == "Archer" ||
                    enemy.tag == "Healer" || enemy.tag == "Booster")
                {
                    inocentAgets++;
                }
                else
                {
                    dangerAgents++;
                }
            }
        }
        foreach (Collider2D ally in allys)
        {
            if (ally.tag == "Miner" || ally.tag == "Cold" || ally.tag == "Archer" ||
            ally.tag == "Healer" || ally.tag == "Booster")
            {
                allysInocentAgets++;
            }
            else
            {
                allysDangerAgents++;
            }
            allysGO.Add(ally.gameObject.name);
        }

    }

    private void DesitionManager()
    {

        //try
        //{
            if (!isInstiantiate)
            {
                switch (cases)
                {
                    case (""):
                        if ((inocentAgets == 0 && dangerAgents == 0) || dangerAgents == 0 || inocentAgets > allysInocentAgets)
                        {
                            SpawnEvilMiner();
                        }
                        else if ((dangerAgents > 0 && allysDangerAgents == 0) || dangerAgents > allysDangerAgents)
                        {
                            /*if (resources >= 250)
                                SpawnEvilTank();*/
                            if (resources >= 150)
                                SpawnEvilNinja();
                            else if (resources >= 100)
                                SpawnEvilSkull();
                            else
                                SpawnEvilMiner();
                        }
                        break;
                    default:
                        switch (cases)
                        {
                            case ("a"):
                                if (dangerAgents <= allysDangerAgents)
                                    SpawnEvilMiner();
                                else
                                {
                                    if (resources >= 150)
                                        SpawnEvilNinja();
                                    else if (resources >= 100)
                                        SpawnEvilSkull();
                                    else
                                        SpawnEvilMiner();
                                }
                                break;
                            case ("b"):
                                if (resources >= 250)
                                    SpawnEvilTank();
                                else if (resources >= 150)
                                    SpawnEvilNinja();
                                else if (resources >= 100)
                                    SpawnEvilSkull();
                                else
                                    SpawnEvilMiner();
                                break;
                            case ("c"):
                                if (resources >= 300 && !allysGO.Contains("Archer(Clone)"))
                                    SpawnEvilArcher();
                                else if (resources >= 200 && !allysGO.Contains("Cold(Clone)"))
                                    SpawnEvilCold();
                                else if (allysDangerAgents >= dangerAgents)
                                    SpawnEvilMiner();
                                else if (resources >= 150)
                                    SpawnEvilNinja();
                                break;
                            default:
                                switch (cases)
                                {
                                    case ("aa"):
                                        if (allysDangerAgents >= dangerAgents && resources >= 200)
                                            SpawnEvilCommander();
                                        else
                                            SpawnEvilMiner();
                                        break;
                                    case ("ab"):
                                        if (dangerAgents > allysDangerAgents && resources >= 250)
                                            SpawnEvilTank();
                                        else
                                            SpawnEvilMiner();

                                        break;
                                    case ("ac"):
                                        if (dangerAgents > allysDangerAgents && resources >= 250 && !allysGO.Contains("Tank(Clone)"))
                                            SpawnEvilTank();
                                        else if (dangerAgents > allysDangerAgents && resources >= 100 && allysGO.Contains("Tank(Clone)"))
                                            SpawnEvilSkull();
                                        else
                                            SpawnEvilMiner();


                                        break;
                                    case ("ba"):

                                        if (resources >= 300 && !allysGO.Contains("Kamikaze(Clone)"))
                                            SpawnEvilKamikaze();
                                        else if (resources >= 250 && allysGO.Contains("Kamikaze(Clone)"))
                                            SpawnEvilBooster();
                                        else
                                            SpawnEvilMiner();

                                        break;
                                    case ("bb"):
                                        if (resources >= 100)
                                            SpawnEvilSkull();
                                        else
                                            SpawnEvilMiner();
                                        break;
                                    case ("ca"):
                                        if (allysGO.Contains("Archer(Clone)") || allysGO.Contains("Cold(Clone)"))
                                            SpawnEvilMiner();
                                        else if (resources >= 250)
                                            SpawnEvilArcher();
                                        else if (resources >= 150)
                                            SpawnEvilCold();
                                        else
                                            SpawnEvilMiner();
                                        break;
                                    case ("cb"):
                                        if (resources >= 250 && !allysGO.Contains("Tank(Clone)"))
                                            SpawnEvilTank();
                                        else if (resources >= 150 && allysGO.Contains("Tank(Clone)"))
                                            SpawnEvilNinja();
                                        else
                                            SpawnEvilMiner();
                                        break;
                                    default:
                                        switch (cases)
                                        {
                                            case ("aaa"):
                                                if (resources >= 350)
                                                    SpawnEvilMonster();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("aab"):
                                                if (resources >= 250 && !allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilTank();
                                                else if (resources >= 150 && allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilNinja();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("abb"):
                                                if (resources >= 250 && !allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilTank();
                                                else if (resources >= 150 && allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilNinja();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("abc"):
                                                if (resources >= 250 && !allysGO.Contains("Magician(Clone)"))
                                                    SpawnEvilMagician();
                                                else if (resources >= 200 && allysGO.Contains("Magician(Clone)"))
                                                    SpawnEvilHealer();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("aca"):
                                                if (resources >= 250 && !allysGO.Contains("Archer(Clone)"))
                                                    SpawnEvilArcher();
                                                else if (resources >= 200 && allysGO.Contains("Archer(Clone)"))
                                                    SpawnEvilCommander();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("acb"):
                                                if (resources >= 250 && !allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilTank();
                                                else if (resources >= 250 && allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilBooster();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("baa"):
                                                if (resources >= 350 && !allysGO.Contains("Monster(Clone)"))
                                                    SpawnEvilMonster();
                                                else if (resources >= 250 && allysGO.Contains("Monster(Clone)"))
                                                    SpawnEvilMagician();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("bab"):
                                                if (resources >= 150 && !allysGO.Contains("Cold(Clone)"))
                                                    SpawnEvilCold();
                                                else if (resources >= 150)
                                                    SpawnEvilNinja();
                                                else if (resources >= 100)
                                                    SpawnEvilSkull();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("bba"):
                                                if (resources >= 250 && !allysGO.Contains("Magician(Clone)"))
                                                    SpawnEvilMagician();
                                                else if (resources >= 250 && allysGO.Contains("Magician(Clone)"))
                                                    SpawnEvilBooster();
                                                else
                                                    SpawnEvilMiner();

                                                break;
                                            case ("bbb"):
                                                if (resources >= 200 && !allysGO.Contains("Commander(Clone)"))
                                                    SpawnEvilCommander();
                                                else if (resources >= 150 && allysGO.Contains("Commander(Clone)"))
                                                    SpawnEvilCold();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("caa"):
                                                if (resources >= 250 && allysGO.FindAll(delegate (string st) { return st == "Archer(Clone)"; }).Count < 2)
                                                    SpawnEvilArcher();
                                                else if (resources >= 150 && allysGO.FindAll(delegate (string st) { return st == "Archer(Clone)"; }).Count >= 2)
                                                    SpawnEvilNinja();
                                                else
                                                    SpawnEvilMiner();

                                                break;
                                            case ("cab"):
                                                if (resources >= 250 && !allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilTank();
                                                else if (resources >= 150 && allysGO.Contains("Tank(Clone)"))
                                                    SpawnEvilNinja();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("cba"):
                                                if (resources >= 100)
                                                    SpawnEvilSkull();
                                                else
                                                    SpawnEvilMiner();
                                                break;
                                            case ("cbb"):
                                                if (resources >= 250)
                                                    SpawnEvilArcher();
                                                else
                                                    SpawnEvilMiner();
                                                break;


                                        }
                                        break;
                                }
                                break;
                        }
                        break;

                }
            }
        //}
        //catch
        //{

        //}


        }
    void ObjectiveManager()
    {
        //try
        //{
        //Debug.Log(allysGO.FindAll(delegate (string st) { return st == "Miner(Clone)"; }).Count + "Con delegado") ;
        //Debug.Log(allysGO.FindAll(miner => GameObject.Find("Miner(Clone)")).Count + "Con GameObject.Find") ;
            switch (cases)
            {
                case (""):
                    if (allysGO.FindAll(delegate (string st) { return st == "Miner(Clone)"; }).Count>= 2 && inocentAgets > allysInocentAgets  /*resources > 500*/)
                    {
                        
                        cases += "a";
                    }
                    else if (allysGO.Contains("Miner(Clone)") 
                        && allysGO.Contains("Ninja(Clone)") /*allysGO.Contains(G("Skeleton(Clone)"))*/ )
                    {

                                cases += "b";
                       
                    }
                    else if (allysDangerAgents == 0 && dangerAgents == 0 || allysDangerAgents > dangerAgents)
                    {

                        cases += "c";
                    }
                    break;
                default:
                    switch (cases)
                    {
                        case ("a"):
                            if (resources >= 500)
                            {
                                if (dangerAgents < allysDangerAgents || dangerAgents == 0)
                                    cases += "a";
                                else if (dangerAgents > allysDangerAgents + 2)
                                    cases += "c";
                                else
                                    cases += "b";
                            }
                            break;
                        case ("b"):
                            if (dangerAgents <= allysDangerAgents && resources >= 300)
                                cases += "a";
                            else if (dangerAgents > allysDangerAgents && resources >= 250)
                                cases += "b";
                            break;
                        case ("c"):
                            if (allysGO.Contains("Archer(Clone)") || allysGO.Contains("Cold(Clone)"))
                            {
                                if (allysDangerAgents >= dangerAgents)
                                    cases += "a";
                                else
                                    cases += "b";
                            }
                            break;
                        default:
                            switch (cases)
                            {
                                case ("aa"):
                                    if (allysDangerAgents >= dangerAgents && resources >= 500)
                                        cases += "a";
                                    else if (allysDangerAgents < dangerAgents && resources >= 300)
                                        cases += "b";
                                    break;
                                case ("ab"):
                                    if (dangerAgents >= allysDangerAgents + 2 && resources >= 300)
                                        cases += "b";
                                    else if ((dangerAgents <= allysDangerAgents || dangerAgents > allysDangerAgents) && resources >= 300)
                                        cases += "c";
                                    break;
                                case ("ac"):
                                    if (dangerAgents > allysDangerAgents && resources >= 300)
                                        cases += "a";
                                    else if (allysDangerAgents >= dangerAgents && resources >= 300)
                                        cases += "b";
                                    break;
                                case ("ba"):
                                    if (dangerAgents > allysDangerAgents && resources >= 400)
                                        cases += "a";
                                    else if (allysDangerAgents <= dangerAgents && resources >= 350)
                                        cases += "b";
                                    break;
                                case ("bb"):
                                    if (allysDangerAgents <= dangerAgents && resources >= 400)
                                        cases += "a";
                                    else if (dangerAgents > allysDangerAgents && resources >= 400)
                                        cases += "b";
                                    break;
                                case ("ca"):
                                    if (allysGO.FindAll(delegate (string st) { return st == "Miner(Clone)"; }).Count >= 3)
                                        cases += "a";
                                    else if (dangerAgents > allysDangerAgents + 2)
                                        cases += "b";
                                    break;
                                case ("cb"):
                                    if (allysGO.Contains("Tank(Clone)") && resources >= 400)
                                        cases += "a";
                                    else if (allysGO.Contains("Ninja(Clone)") && resources >= 400)
                                        cases += "b";
                                    break;
                            }
                            break;
                    }
                    break;

            }
        //}
        //catch
        //{

        //}

    }


    void OnTriggerEnter2D(Collider2D collision){
        if ((collision.gameObject.tag == "Magic" || collision.gameObject.tag == "Arrow") && collision.gameObject.layer != this.gameObject.layer){
            hp -= collision.gameObject.GetComponent<MagicSpell>().damage;
        }

    }
    void SpawnEvilMiner()
    {
        if (!isInstiantiate && resources >= 50)
        {
            SpriteRenderer sprite;
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilMiner, EvilHome.position, Quaternion.identity);
                tmp.GetComponent<AgentMiner>().right = false;
                isLeft = true;

            }
            else
            {
                tmp = Instantiate(agentEvilMiner, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                tmp.GetComponent<AgentMiner>().right = true;
                isLeft = false;

            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentMiner>().alliadeBase = gameObject;
            tmp.GetComponent<AgentMiner>().whatIsEnemy = whatIsEnemy;
            tmp.GetComponent<AgentMiner>().isEvil = true;
            isInstiantiate = true;

            resources -= 50;

        }

    }
    void SpawnEvilNinja()
    {
        if (!isInstiantiate && resources >= 100)
        {
            SpriteRenderer sprite;
            GameObject baseEnem = GameObject.Find("Base");
            GameObject tmp;

            if (!isLeft)
            {
                tmp = Instantiate(agentEvilNinja, EvilHome.position, Quaternion.identity);
                isLeft = true;

            }
            else
            {
                tmp = Instantiate(agentEvilNinja, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentNinja>().baseEnemy = baseEnem.transform;
            tmp.GetComponent<AgentNinja>().whatIsEnemy = whatIsEnemy;
            isInstiantiate = true;

            resources -= 100;

        }
    }

    void SpawnEvilTank()
    {
        if (!isInstiantiate && resources >= 200)
        {
            SpriteRenderer sprite;
            GameObject baseEnem = GameObject.Find("Base");
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilTank, EvilHome.position, Quaternion.identity);

                isLeft = true;

            }
            else
            {
                tmp = Instantiate(agentEvilTank, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            //tmp.GetComponent<AgentTank>().whatIsObs = wahtIsObs;
            tmp.GetComponent<AgentTank>().baseEnemy = baseEnem.transform;
            tmp.GetComponent<AgentTank>().whatIsTower = whatIsEnemy;
            isInstiantiate = true;
            resources -= 200;

        }
    }

    void SpawnEvilArcher()
    {
        if (!isInstiantiate && resources >= 200)
        {
            SpriteRenderer sprite;
            GameObject tmp = Instantiate(agentEvilArcher, EvilHome.position, Quaternion.identity);
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentArcher>().whatIsEnemy = whatIsEnemy;
            isInstiantiate = true;

            resources -= 200;
        }
    }

    void SpawnEvilSkull()
    {
        if (!isInstiantiate && resources >= 50)
        {

            SpriteRenderer sprite;
            GameObject baseEnem = GameObject.Find("Base");
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilSkull, EvilHome.position, Quaternion.identity);

                isLeft = true;

            }
            else
            {
                tmp = Instantiate(agentEvilSkull, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;

            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentSkull>().whatIsAlly = whatIsAlly;
            tmp.GetComponent<AgentSkull>().baseEnemy = baseEnem.transform;
            tmp.GetComponent<AgentSkull>().whatIsEnemy = whatIsEnemy;
            isInstiantiate = true;
            resources -= 50;
        }
    }

    void SpawnEvilMonster()
    {
        if (!isInstiantiate && resources >= 300)
        {
            SpriteRenderer sprite;
            GameObject baseEnem = GameObject.Find("Base");
            GameObject tmp;

            if (!isLeft)
            {
                tmp = Instantiate(agentEvilMonster, EvilHome.position, Quaternion.identity);
                isLeft = true;
            }
            else
            {
                tmp = Instantiate(agentEvilMonster, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentMonster>().baseEnemy = baseEnem.transform;
            tmp.GetComponent<AgentMonster>().whatIsEnemy = whatIsEnemy;
            isInstiantiate = true;
            resources -= 300;
        }
    }

    void SpawnEvilCommander()
    {
        if (!isInstiantiate && resources >= 150)
        {
            SpriteRenderer sprite;
            GameObject baseEnem = GameObject.Find("Base");
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilCommander, EvilHome.position, Quaternion.identity);

                isLeft = true;

            }
            else
            {
                tmp = Instantiate(agentEvilCommander, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);

                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentCommander>().baseEnemy = baseEnem.transform;
            tmp.GetComponent<AgentCommander>().whatIsEnemy = whatIsEnemy;
            tmp.GetComponent<AgentCommander>().whatIsAlly = whatIsAlly;
            isInstiantiate = true;
            resources -= 150;
        }
    }


    void SpawnEvilMagician()
    {
        if (!isInstiantiate && resources >= 200)
        {
            SpriteRenderer sprite;
            GameObject baseEnemy = GameObject.Find("Base");
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilMagician, EvilHome.position, Quaternion.identity);
                isLeft = true;

            }
            else
            {
                tmp = Instantiate(agentEvilMagician, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentMagician>().baseEnemy = baseEnemy.transform;
            tmp.GetComponent<AgentMagician>().whatIsEnemy = whatIsEnemy;
            tmp.GetComponent<AgentMagician>().whatIsAlly = whatIsAlly;
            tmp.GetComponent<AgentMagician>().whatIsTower = whatIsTower;
            isInstiantiate = true;
            resources -= 200;
        }

    }

    void SpawnEvilCold()
    {
        if (!isInstiantiate && resources >= 100)
        {
            SpriteRenderer sprite;
            GameObject tmp = Instantiate(agentEvilCold, EvilHome.position, Quaternion.identity);
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            GameObject baseEnemy = GameObject.Find("Base");
            tmp.GetComponent<AgentCold>().baseEnemy = baseEnemy.transform;
            tmp.GetComponent<AgentCold>().whatIsEnemy = whatIsEnemy;
            tmp.GetComponent<AgentCold>().whatIsAlly = whatIsAlly;
            isInstiantiate = true;
            resources -= 100;
        }
    }


    void SpawnEvilBooster()
    {
        if (!isInstiantiate && resources >= 200)
        {
            SpriteRenderer sprite;
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilBooster, EvilHome.position, Quaternion.identity);
                isLeft = true;
            }
            else
            {
                tmp = Instantiate(agentEvilBooster, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentBooster>().whatIsEnemy = whatIsEnemy;
            tmp.GetComponent<AgentBooster>().whatIsAlly = whatIsAlly;
            isInstiantiate = true;
            resources -= 200;
        }
    }

    void SpawnEvilHealer()
    {
        if (!isInstiantiate && resources >= 150)
        {
            SpriteRenderer sprite;
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilHealer, EvilHome.position, Quaternion.identity);
                isLeft = true;
            }
            else
            {
                tmp = Instantiate(agentEvilHealer, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);
                isLeft = false;

            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentHealer>().whatIsTower = whatIsEnemy;
            tmp.GetComponent<AgentHealer>().whatIsAlly = whatIsAlly;
            isInstiantiate = true;
            resources -= 150;
        }
    }

    void SpawnEvilKamikaze()
    {
        if (!isInstiantiate && resources >= 250)
        {

            SpriteRenderer sprite;
            GameObject baseEnemy = GameObject.Find("Base");
            GameObject tmp;
            if (!isLeft)
            {
                tmp = Instantiate(agentEvilKamikaze, EvilHome.position, Quaternion.identity);

                isLeft = true;
            }
            else
            {
                tmp = Instantiate(agentEvilKamikaze, new Vector3(EvilHome.position.x + 2, EvilHome.position.y, EvilHome.position.z), Quaternion.identity);

                isLeft = false;
            }
            sprite = tmp.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1, 0, 0, 1);
            tmp.gameObject.layer = LayerMask.NameToLayer("Enemy");
            tmp.GetComponent<AgentKamikaze>().enemyBase = baseEnemy.transform;
            tmp.GetComponent<AgentKamikaze>().whatIsEnemy = whatIsEnemy;
            tmp.GetComponent<AgentKamikaze>().whatIsAlly = whatIsAlly;
            isInstiantiate = true;
            resources -= 250;
        }
    }

    void CoolDownInstantiate(){
        count -= Time.deltaTime;
        if (count <= 0)
        {
            isInstiantiate = false;
            count = 2.0f;
        }
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

    }
    void WaitForDie(){
        if (!playAudio){
            dieAudio.Play();
            playAudio = true;
        }
        timeToDie -= Time.deltaTime;
        if (timeToDie <= 0){
            gameObject.SetActive(false);
        }
    }
}