using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    public int x { get; private set; }
    public int y { get; private set; }
    //Số lượng bom xung quanh
    public int Number;
    public bool IsMine;

    public CellState State;

    private TextMeshProUGUI numberText;
    public Board manager;

    public void info(int x, int y,Board board)
    {
        this.x = x;
        this.y = y;
        this.manager = board;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (State == CellState.Showing || State == CellState.Flag) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            manager.revealCell(x, y);
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            manager.toggleFlag(x, y);
        }
    }
    
}
