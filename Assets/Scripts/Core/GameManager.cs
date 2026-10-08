using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static event Action<bool> OnGameEnd;
    public static GameManager Instance;
    public GameState Gamestate;
    public int[,] CellStates;
    [SerializeField] private Board board ;
    public int Width => board.dimensions.x;
    public int Height => board.dimensions.y;
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
        board.revealAll();
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

    public int[,] getCellStates()
    {
        int width = board.dimensions.x;
        int height = board.dimensions.y;
        CellStates = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = board.boardCell[x, y];
                if(cell.State == CellState.Hidden)
                {
                    CellStates[x, y] = -1;
                }
                else if(cell.State == CellState.Flag)
                {
                    CellStates[x, y] = -2;
                }
                else
                {
                    CellStates[x, y] = cell.Number;
                }
            }
        }
        return CellStates;
    }

    public void RevalCell(int x, int y)
    {
        board.revealCell(x, y);
    }

    public void ToggleFlag(int x, int y)
    {
        board.toggleFlag(x, y);
    }
    
}
