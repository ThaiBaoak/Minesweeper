using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static event Action<bool> OnGameEnd;
    public static GameManager Instance;
    public GameState Gamestate;
    public int[,] CellStates;
    [SerializeField] private Board board ;
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private int mineCount = 10;
    [SerializeField] private int benchmarkTotalRuns = 100;
    [SerializeField] private float delayTime = 1f;
    public int Width => width;
    public int Height => height;
    public int MineCount => mineCount;
    public int BenchmarkTotalRuns => benchmarkTotalRuns;

    public float DelayTime => delayTime;

    private const string PREF_DIFFICULTY = "SelectedDifficulty";
    private const string PREF_BENCHMARK_RUNS = "BenchmarkTotalRuns";
    private const string PREF_AI_DELAY = "AIAutoPlayDelay";
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null) Instance = this;
        else {Destroy(gameObject);return;}

        int savedDifficulty = PlayerPrefs.GetInt(PREF_DIFFICULTY, 1);
        Debug.Log($"[GameManager] Load Scene thành công! Độ khó đã nạp: {savedDifficulty} ({width}x{height} - {mineCount} mìn)");
        ApplyDifficultyConfig(savedDifficulty);
        benchmarkTotalRuns = PlayerPrefs.GetInt(PREF_BENCHMARK_RUNS, 100);
        delayTime = PlayerPrefs.GetFloat(PREF_AI_DELAY, 0.2f);
        
    }

    public void SetDifficulty(int difficultyIndex)
    {
        Debug.Log($"<color=red>[2. SetDifficulty Dược Gọi] Giá trị mới gán vào: {difficultyIndex}</color>");
        PlayerPrefs.SetInt(PREF_DIFFICULTY, difficultyIndex);
        PlayerPrefs.Save();

    }
    private void ApplyDifficultyConfig(int index)
    {
        switch (index)
        {
            case 0: // Easy
                width = 5; height = 5; mineCount = 3;
                break;
            case 1: // Medium
                width = 9; height = 9; mineCount = 10;
                break;
            case 2: // Hard
                width = 16; height = 16; mineCount = 40;
                break;
        }
    }

    public void SetDelayTime(float delay)
    {
        delayTime = Mathf.Max(0.01f, delay);
        PlayerPrefs.SetFloat(PREF_AI_DELAY, delayTime);
        PlayerPrefs.Save();
    }

    public void SetBenchmarkTotalRuns(int runs)
    {
        benchmarkTotalRuns = Mathf.Max(1, runs); // Đảm bảo tối thiểu là 1 ván
        PlayerPrefs.SetInt(PREF_BENCHMARK_RUNS, benchmarkTotalRuns);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] Đã lưu Benchmark Runs mới: {benchmarkTotalRuns}");
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
            if(GameEndModal.Instance != null)
            {
                GameEndModal.Instance.ShowGameEnd(true, Clock.time);
            }
            OnGameEnd?.Invoke(true);
        }
        else if (Gamestate == GameState.Lose)
        {
            Debug.Log("You Lose");
            if(GameEndModal.Instance != null)
            {
                GameEndModal.Instance.ShowGameEnd(false, Clock.time);
            }
            OnGameEnd?.Invoke(false);
        }
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void RestartGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log($"[Restart] Dang restart voi Width = {width}, Height = {height}");
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

    public void RevealCell(int x, int y)
    {
        board.revealCell(x, y);
    }

    public void ToggleFlag(int x, int y)
    {
        board.toggleFlag(x, y);
    }
    
}
