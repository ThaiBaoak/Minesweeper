using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AIManager : MonoBehaviour, IBoardAdapter
    {
        
        //Deplay Time
        [SerializeField] private float delayTime = 1f;
        [SerializeField] private History moveHistoryUI;

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
            

            if(moveHistoryUI != null)
            {
                Debug.Log($"[AI] Thêm bước vào lịch sử: {actionData.action} tại ({actionData.x}, {actionData.y})");
                moveHistoryUI.AddHistoryItem(actionData);
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

        public void ToggleAutoSolve()
        {
            if (solveCoroutine != null)
            {
                StopAutoSolve();
            }
            else
            {
                StartAutoSolve();
            }
        }

        public void StartAutoSolve()
        {
            if (solveCoroutine == null && !IsGameOver)
            {
                solveCoroutine = StartCoroutine(AutoSolveRoutine());
            }
        }

        public void StopAutoSolve()
        {
            if (solveCoroutine != null)
            {
                StopCoroutine(solveCoroutine);
                solveCoroutine = null;
            }
        }

        private IEnumerator AutoSolveRoutine()
        {
            while (!IsGameOver)
            {
                BoardInfo currentBoard = GetBoardInfo();
                if (currentBoard == null) break;

                AIActionData nextAction = solver.NextMove(currentBoard);

                // Nếu AI bế tắc  dừng chế độ Auto
                if (nextAction.x < 0 || nextAction.y < 0)
                {
                    Debug.LogWarning("[AIManager] AI dừng chạy tự động do bế tắc logic.");
                    break;
                }

                ExecuteAction(nextAction);

                // Chờ khoảng thời gian delayTime 
                yield return new WaitForSeconds(delayTime);
            }

            solveCoroutine = null;
        }


        private void OnDisable()
        {
            StopAutoSolve(); // Đảm bảo dừng Coroutine khi GameObject bị tắt
        }
    }
}