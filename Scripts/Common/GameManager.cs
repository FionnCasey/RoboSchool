using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public Button start, quit, robo, cart;
    public GameObject startPanel, mainPanel;

    public void StartPress()
    {
        startPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void LoadRobo()
    {
        SceneManager.LoadScene("PPO", LoadSceneMode.Single);
    }

    public void LoadCart()
    {
        SceneManager.LoadScene("CartPole", LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
