using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    //[SerializeField] private GameManager GameManager;
    //[SerializeField] private Button button;

    
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private int hard;
    private int mine;
    public Vector2Int dimensions = Vector2Int.zero;
    private float cellWidth;
    private float cellHeight;
    public Cell[,] boardCell;
    public RectTransform holder;
    private static readonly Vector2Int[] direction = new Vector2Int[]{
        new(-1, 0), new(1, 0), new(0, -1), new(0, 1),
        new(-1, -1), new(1, 1), new(1, -1), new(-1, 1)
    };

    [SerializeField] private Sprite mineSprite;
    [SerializeField] private Sprite flagSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite[] numberSprites;


    private void Awake()
    {
        holder = GetComponent<RectTransform>();
        setup(holder, hard);
        boardCell = new Cell[dimensions.x, dimensions.y];
        creatCells();
        placeMine();
        calculate();
        PrintBoard();
    }
    private void setup(RectTransform rt, int hard)
    {
        if (rt.rect.width > rt.rect.height)
        {
            dimensions.y = hard * 2;
            dimensions.x = Mathf.FloorToInt(dimensions.y * (rt.rect.width / rt.rect.height));
        }
        else
        {
            dimensions.x = hard * 2;
            dimensions.y = Mathf.FloorToInt(dimensions.x * (rt.rect.height / rt.rect.width));
        }

        //Tính số Mine
        int total = dimensions.x * dimensions.y;
        float ratio = 0.05f + 0.1f * Mathf.Sqrt(hard / (float)10);
        mine = (int)(total * ratio);
        //Tính kích thước cell
        cellWidth = rt.rect.width / dimensions.x;
        cellHeight = rt.rect.height / dimensions.y;
        Debug.Log(total);
        Debug.Log(mine);
    }

    private void creatCells()
    {
        for (int col = 0; col < dimensions.x; col++)
        {
            for (int row = 0; row < dimensions.y; row++)
            {
                Cell Cell = Instantiate(cellPrefab, holder);
                Cell.manager = this;
                Cell.info(col, row, this);
                RectTransform rt = Cell.GetComponent<RectTransform>();

                //Đặt Anchor và pivot
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                //Tính toán vị trí
                float x = (-cellWidth * dimensions.x / 2f) + (col + 0.5f) * cellWidth;
                float y = (-cellHeight * dimensions.y / 2f) + (row + 0.5f) * cellHeight;
                rt.localPosition = new Vector3(x, y, 0);
                rt.sizeDelta = new Vector2(cellWidth, cellHeight);
                rt.localScale = Vector3.one;

                boardCell[col, row] = Cell;
                Cell.name = $"Cell ({col},{row})";
                //Debug.Log($"{col},{row} - size: {rt.rect.size} - pos: {rt.localPosition}");
            }
        }
    }

    private void placeMine()
    {
        int placed = 0;
        while (placed < mine)
        {
            int x = Random.Range(0, dimensions.x);
            int y = Random.Range(0, dimensions.y);
            if (!boardCell[x, y].IsMine)
            {
                boardCell[x, y].IsMine = true;
                placed++;
            }
        }
    }

    public void PrintBoard()
    {
        string board = "";
        for (int row = dimensions.y - 1; row >= 0; row--) // từ hàng trên cùng xuống
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                board += boardCell[col, row].IsMine ? "* " : $"{boardCell[col, row].Number} ";
            }
            board += "\n"; // xuống dòng sau mỗi hàng
        }
        Debug.Log(board);
        Debug.Log(dimensions.x);
        Debug.Log(dimensions.y);
    }
    // Tính số mìn xung quanh của ô
    private void calculate()
    {
        for (int col = 0; col < dimensions.x; col++)
        {
            for (int row = 0; row < dimensions.y; row++)
            {
                if (boardCell[col, row].IsMine) continue;
                int n = 0;
                foreach (var dir in direction)
                {
                    int nx = col + dir.x;
                    int ny = row + dir.y;
                    if (isBound(nx, ny) && boardCell[nx, ny].IsMine)
                    {
                        n++;
                    }
                }
                boardCell[col, row].Number = n;
            }
        }
    }

    public void revealCell(int x, int y)
    {
        if (!isBound(x, y)) return;
        Cell c = boardCell[x, y];
        if (c.State == CellState.Showing || c.State == CellState.Flag) return;
        
        //Thay đổi sprite của ô dựa trên trạng thái
        if(c.IsMine)
        {
            c.GetComponent<UnityEngine.UI.Image>().sprite = mineSprite;
        }
        else if (c.Number > 0)
        {
            c.GetComponent<UnityEngine.UI.Image>().sprite = numberSprites[c.Number - 1];
        }
        else
        {
            c.GetComponent<UnityEngine.UI.Image>().sprite = emptySprite;
        }
        //
        if (GameManager.Instance.Gamestate != GameState.Playing) return;
        if (c.IsMine) { GameManager.Instance.Gamestate = GameState.Lose; GameManager.Instance.GameEnd();return;}
        c.State = CellState.Showing;
        if (c.Number == 0)
        {
            foreach (var dir in direction)
            {
                revealCell(x + dir.x, y + dir.y);
            }
        }
        checkWin();
    }
    
    private bool isBound(int x, int y)
    {
        return x >= 0 && x < dimensions.x && y >= 0 && y < dimensions.y;
    }

    private void checkWin()
    {
        for (int col = 0; col < dimensions.x; col++)
        {
            for (int row = 0; row < dimensions.y; row++)
            {
                Cell c = boardCell[col, row];
                if (!c.IsMine && c.State != CellState.Showing)
                {
                    return;
                }
            }
        }
        GameManager.Instance.Gamestate = GameState.Win;
        GameManager.Instance.GameEnd();
    }


    public void revealAll()
    {
        for (int col = 0; col < dimensions.x; col++)
        {
            for (int row = 0; row < dimensions.y; row++)
            {
                Cell c = boardCell[col, row];
                if (c.State != CellState.Showing)
                {
                    revealCell(col, row);
                }
            }
        }
    }

    public void toggleFlag(int x, int y)
    {
        if (!isBound(x, y)) return;
        Cell c = boardCell[x, y];
        if (c.State == CellState.Showing) return;

        if (c.State == CellState.Hidden)
        {
            c.State = CellState.Flag;
            c.GetComponent<UnityEngine.UI.Image>().sprite = flagSprite;
        }
        else if (c.State == CellState.Flag)
        {
            c.State = CellState.Hidden;
            c.GetComponent<UnityEngine.UI.Image>().sprite = normalSprite;
            Debug.Log($"Cell {x},{y} toggled back to Hidden");
        }
    }
}
