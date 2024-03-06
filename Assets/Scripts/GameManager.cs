using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;


public class GameManager : MonoBehaviour
{
    public const float KID_STUN_DURATION = 5f;


    public static GameManager Instance {  get; private set; }
    public Player Player { get; private set; }
    public ProjectileManager Projectiles { get; private set; }

    public KidType DeathType {  get; private set; }

    public static LevelConnection StartLevelConnection { get; private set;  }
    public static LevelConnection LevelConnection { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.Player = FindAnyObjectByType<Player>();
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        LevelConnection._entrance_name = "";
        Player = FindAnyObjectByType<Player>();
        Instance.Player = Player;
        Projectiles = FindAnyObjectByType<ProjectileManager>();
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
        Debug.Log("Quitting...");
        EditorApplication.isPlaying = false;
        Application.Quit();
        //Application.ExternalEval("window.open('about:blank','_self')");
    }

    public void DoGameOver(KidType type)
    {
        DeathType = type;
        SceneManager.LoadScene("GameOverScene");
    }


}
