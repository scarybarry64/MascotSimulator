using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {  get; private set; }

    public KidType DeathType {  get; private set; }

    public const string PLAYER_TAG = "Player";

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
        DeathType = type;
        SceneManager.LoadScene("GameOverScene");
    }
}
