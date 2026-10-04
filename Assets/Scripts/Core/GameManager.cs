using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static event Action<bool> OnGameEnd;
    public static GameManager Instance;
    public GameState Gamestate;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        Gamestate = GameState.Waiting;
    }

    public void StartGame()
    {
        Gamestate = GameState.Playing;
    }

    public void GameEnd()
    {
        if (Gamestate == GameState.Win)
        {
            Debug.Log("You Win");
            OnGameEnd?.Invoke(true);
        }
        else if (Gamestate == GameState.Lose)
        {
            Debug.Log("You Lose");
            OnGameEnd?.Invoke(false);
        }
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
