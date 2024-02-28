using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public const float KID_STUN_DURATION = 5f;


    public static GameManager instance {  get; private set; }
    public Player Player { get; private set; }
    public KidType KillerType {  get; private set; }

    public static LevelConnection StartLevelConnection { get; private set;  }
    public static LevelConnection LevelConnection { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        LevelConnection = Instantiate(StartLevelConnection);
        Player = FindAnyObjectByType<Player>();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void WinGame()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void QuitGame()
    {
        Application.Quit();

        //Application.ExternalEval("window.open('about:blank','_self')");
    }

    public void DoGameOver(KidType type)
    {
        KillerType = type;
        SceneManager.LoadScene("GameOverScene");
    }
}
