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
    private Vector2Int dimensions = Vector2Int.zero;
    private float cellWidth;
    private float cellHeight;
    private Cell[,] boardCell;
    private List<Cell> listCell = new List<Cell>();
    public RectTransform holder;

    private void Awake()
    {
        holder = GetComponent<RectTransform>();
        setup(holder, hard);
        boardCell = new Cell[dimensions.x, dimensions.y];
        creatCells();
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
                listCell.Add(Cell);
                Cell.name = $"Cell ({col},{row})";
                //Debug.Log($"{col},{row} - size: {rt.rect.size} - pos: {rt.localPosition}");
            }
        }
    }
}
