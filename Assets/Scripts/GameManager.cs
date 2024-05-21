using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region //Serializable
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI resourcesText;
    [SerializeField]  GameObject pausePanel;
    [SerializeField]  GameObject bottonPause;
    [SerializeField]  public int resources = 300;

    #endregion

    #region //Public
    public GameObject agentMiner, agentTank, agentNinja, agentHealer;
    public GameObject agentBooster, agentCommander, agentCold;
    public GameObject agentSkull, agentMonster, agentArcher;
    public GameObject agentKamikaze, agentMagician, agentConverter;
    public Transform Home;
    public GameObject EnemyBase,PlayerBase,gameOverPanel ,winPanel, botonPanel;
    public AudioSource gameMusic;
    public AudioSource successAudio;
    

    #endregion

    #region //Member
    bool isInstantiate = false, playAudio = false, isLeft = false;
    float gameintTime = 10;
    float count = 2.0f;
    #endregion

    // Start is called before the first frame update
    void Start(){
        gameOverPanel.SetActive(false);
        botonPanel.SetActive(true);
        pausePanel.SetActive(false);

    }

    void Update(){
        if (isInstantiate){
            CoolDownInstantaite();
        }
        if (PlayerBase.GetComponent<BaseSpawn>().hp <= 0 || gameintTime <= 0 ){
            GameOver();
        }else if(EnemyBase.activeSelf == false)
        {
            Win();
        }
        else
                {
            //gameintTime -= Time.deltaTime;
            timeText.text = "Time:" + ((int)gameintTime).ToString();
        }
        resourcesText.text = "Resources: " + resources;
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void GameOver(){
        gameMusic.Pause();
        bottonPause.SetActive(false);
        if (!playAudio){
            successAudio.Play();
            playAudio = true;
        }
        gameOverPanel.SetActive(true);
        botonPanel.SetActive(false);
    }
    void Win()
    {
        gameMusic.Pause();
        bottonPause.SetActive(false);
        if (!playAudio)
        {
            successAudio.Play();
            playAudio = true;
        }
        winPanel.SetActive(true);
        botonPanel.SetActive(false);
    }
    // Update is called once per frame
    public void SpawnMiner(){
        if (!isInstantiate && resources >= 50){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentMiner, Home.position, Quaternion.identity);
                tmp.GetComponent<AgentMiner>().right = false;
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentMiner, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                tmp.GetComponent<AgentMiner>().right = true;
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 50;
        }
    }
    public void SpawnTank(){
        if (!isInstantiate && resources >= 200){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentTank, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentTank, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 200;

        }

    }
    public void SpawnNinja(){
        if (!isInstantiate && resources >= 100 ){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentNinja, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else {
                tmp = Instantiate(agentNinja, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 100;
            
        }

    }
    public void SpawnHealer(){
        if (!isInstantiate && resources >= 150){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentHealer, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentHealer, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 150;
        }

    }

    public void SpawnBooster(){
        if (!isInstantiate && resources >= 200 ){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentBooster, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentBooster, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 200;

        }
    }

    public void SpawnCommander(){
        
        if (!isInstantiate && resources >= 150 ){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentCommander, Home.position, Quaternion.identity);
                isLeft = true;
            }  
            else{
                tmp = Instantiate(agentCommander,new Vector3(Home.position.x + 5, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 150;
        }
    }

    public void SpawnCold(){
        if (!isInstantiate && resources >= 100 ) {
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentCold, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else {
                tmp = Instantiate(agentCold, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 100;
        }
    }

    public void SpawnSkull(){
        if (!isInstantiate && resources >= 50) {
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentSkull, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentSkull, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            if (GameObject.FindGameObjectWithTag("Skeleton") &&
                GameObject.FindGameObjectWithTag("Skeleton") != tmp){ 
                tmp.GetComponent<AgentSkull>().isLeader = false;
            }
            else{
                tmp.GetComponent<AgentSkull>().isLeader = true;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 50;
        }
    }

    public void SpawnMonster(){
        if (!isInstantiate && resources >= 300){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentMonster, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentMonster, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 300;
        }
    }

    public void SpawnArcher(){
        if (!isInstantiate && resources >= 200) {
            GameObject tmp = Instantiate(agentArcher, Home.position, Quaternion.identity);
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 200;

        }
    }

    public void SpawnKamikaze(){
        if (!isInstantiate && resources >= 250){
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentKamikaze, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentKamikaze, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 250;

        }
    }

    public void SpawnMagician(){
        if (!isInstantiate && resources >= 200) {
            GameObject tmp;
            if (!isLeft){
                tmp = Instantiate(agentMagician, Home.position, Quaternion.identity);
                isLeft = true;
            }
            else{
                tmp = Instantiate(agentMagician, new Vector3(Home.position.x + 2, Home.position.y, Home.position.z), Quaternion.identity);
                isLeft = false;
            }
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 200;
        }
    }
    public void SpawnConverter()
    {
        if(!isInstantiate && resources >= 250)
        {
            GameObject tmp = Instantiate(agentConverter, Home.position, Quaternion.identity);
            tmp.gameObject.layer = LayerMask.NameToLayer("Ally");
            isInstantiate = true;
            resources -= 250;
        }
    }

    public void RestartGame(){
        SceneManager.LoadScene("GameScene");
    }

    void CoolDownInstantaite(){
        count -= Time.deltaTime;
        if(count <= 0)
        {
            isInstantiate = false;
            count = 2.0f;
        }
    }
}
