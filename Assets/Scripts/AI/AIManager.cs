using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AIManager : MonoBehaviour, IBoardAdapter
    {
        
        //Deplay Time
        [SerializeField] private float delayTime = 1f;

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
                GameManager.Instance.RevalCell(actionData.x, actionData.y);
            }
            else if(actionData.action == AIAction.Flag)
            {
                GameManager.Instance.ToggleFlag(actionData.x, actionData.y);
            }
            return true;
        }
    }
}