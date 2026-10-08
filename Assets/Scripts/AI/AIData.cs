using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public enum AIAction
    {
        Reveal,
        Flag
    }

    public struct AIActionData
    {
        public int x,y;
        public AIAction action;

        public AIActionData(int x, int y, AIAction action)
        {
            this.x = x;
            this.y = y;
            this.action = action;
        }
    }

    public class BoardInfo
    {
        public int width, height;
        public int[,] CellStates; // 0: empty, 1-8: number, -1: Chưa mở, -2: flagged
        public BoardInfo(int width, int height, int[,] cellStates)
        {
            this.width = width;
            this.height = height;
            this.CellStates = cellStates;
        }
    }

    public interface IBoardAdapter
    {
        BoardInfo GetBoardInfo();
        bool ExecuteAction(AIActionData actionData);
        bool IsGameOver { get; }
    }


}


