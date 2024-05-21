using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public GameObject Panel;
    public GameObject Start;
   public void showCredits()
    {
        Panel.SetActive(true);
        Start.SetActive(false);
    }

    public void returnToStart()
    {
        Panel.SetActive(false);
        Start.SetActive(true);
    }

    public void changeScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
}
