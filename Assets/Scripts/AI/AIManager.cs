using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AIManager : MonoBehaviour, IBoardAdapter
    {
        
        //Deplay Time
        //[SerializeField] private float delayTime = 1f;

        private AISolve solver = new AISolve();
        private Coroutine solveCoroutine;
        public bool IsGameOver => GameManager.Instance.Gamestate == GameState.Win || GameManager.Instance.Gamestate == GameState.Lose;
        public BoardInfo GetBoardInfo()
        {
            return new BoardInfo(GameManager.Instance.Width, GameManager.Instance.Height, GameManager.Instance.getCellStates());
        }

        public bool ExecuteAction(AIActionData actionData)
        {
            if(IsGameOver)
            {
                return false;
            }

            if(actionData.action == AIAction.Reveal)
            {
                GameManager.Instance.RevealCell(actionData.x, actionData.y);
            }
            else if(actionData.action == AIAction.Flag)
            {
                GameManager.Instance.ToggleFlag(actionData.x, actionData.y);
            }
            return true;
        }

        public void MakeSingleStep()
        {
            if(IsGameOver)
            {
                return;
            }
            BoardInfo boardInfo = GetBoardInfo();
            AIActionData nextAction = solver.NextMove(boardInfo);

            if(nextAction.x >= 0 && nextAction.y >= 0)
            {
                ExecuteAction(nextAction);
            }
            else
            {
                Debug.LogWarning("[AI] Không tìm thấy nước đi hợp lệ tiếp theo.");
            }
        }
    }
}