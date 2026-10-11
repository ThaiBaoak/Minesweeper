using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AI
{
    public class FastAIBenchmark : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image resultImagePanel; // Khung Image nền chứa thông báo
        [SerializeField] private TextMeshProUGUI txtCombinedResult; // TextMeshPro hiển thị toàn bộ kết quả

        [Header("Benchmark Settings")]

        private AISolve solver = new AISolve();

        private void Start()
        {
            if (resultImagePanel != null)
            {
                resultImagePanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Gắn hàm này vào Nút "Run Benchmark 100" trên UI
        /// </summary>
        public void RunFastBenchmark()
        {
            // 1. Đồng bộ kích thước & số mìn trực tiếp từ GameManager Settings
            int width = 9;
            int height = 9;
            int mineCount = 10;
            int totalRuns = 100;

            if (GameManager.Instance != null)
            {
                width = GameManager.Instance.Width;
                height = GameManager.Instance.Height;
                mineCount = GameManager.Instance.MineCount;
                totalRuns = GameManager.Instance.BenchmarkTotalRuns;
            }

            int wins = 0;
            int losses = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // 2. Chạy mô phỏng 100 ván thuần túy trên RAM
            for (int i = 0; i < totalRuns; i++)
            {
                if (SimulateSingleGame(width, height, mineCount))
                {
                    wins++;
                }
                else
                {
                    losses++;
                }
            }

            sw.Stop();

            // 3. Hiển thị báo cáo gộp kết quả có kích thước & số mìn
            ShowCombinedResult(width, height, mineCount, totalRuns, wins, losses, sw.ElapsedMilliseconds);
        }

        private bool SimulateSingleGame(int w, int h, int mines)
        {
            int[,] realBoard = GenerateRandomBoard(w, h, mines);
            int[,] visibleBoard = new int[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    visibleBoard[x, y] = -1; // -1: Hidden
                }
            }

            bool isDead = false;
            int revealedSafeCells = 0;
            int targetSafeCells = (w * h) - mines;

            while (!isDead && revealedSafeCells < targetSafeCells)
            {
                BoardInfo info = new BoardInfo(w, h, mines, visibleBoard);
                AIActionData action = solver.NextMove(info);

                if (action.x < 0 || action.y < 0) break; // AI bế tắc logic

                if (action.action == AIAction.Flag)
                {
                    visibleBoard[action.x, action.y] = -2;
                }
                else if (action.action == AIAction.Reveal)
                {
                    if (realBoard[action.x, action.y] == -1)
                    {
                        isDead = true; // Trúng mìn -> Thua
                        break;
                    }

                    if (visibleBoard[action.x, action.y] == -1)
                    {
                        visibleBoard[action.x, action.y] = realBoard[action.x, action.y];
                        revealedSafeCells++;

                        // Loang ô số 0 trên RAM
                        if (realBoard[action.x, action.y] == 0)
                        {
                            FloodFillVirtual(realBoard, visibleBoard, action.x, action.y, w, h, ref revealedSafeCells);
                        }
                    }
                }
            }

            return !isDead && revealedSafeCells >= targetSafeCells;
        }

        private void ShowCombinedResult(int w, int h, int mines, int total, int wins, int losses, long totalMs)
        {
            float totalSeconds = totalMs / 1000f;
            float winRate = ((float)wins / total) * 100f;
            float avgMs = totalMs / (float)total;

            string fullReport = $"<size=120%><b>THỐNG KÊ BENCHMARK AI</b></size>\n" +
                                $"-----------------------------------------\n" +
                                $"• Kích thước bàn cờ: <b>{w} x {h}</b>\n" +
                                $"• Số lượng mìn: <b>{mines} mìn</b>\n" +
                                $"• Tổng số ván mô phỏng: <b>{total}</b>\n" +
                                $"• Thắng / Thua: <color=green><b>{wins}</b></color> / <color=red><b>{losses}</b></color>\n" +
                                $"• Tỷ lệ thắng: <color=#00FF88><b>{winRate:F1}%</b></color>\n" +
                                $"• Tổng thời gian: <b>{totalSeconds:F3}s</b>\n" +
                                $"• Thời gian trung bình: <b>{avgMs:F2} ms/ván</b>";

            if (txtCombinedResult != null)
            {
                txtCombinedResult.text = fullReport;
            }

            if (resultImagePanel != null)
            {
                resultImagePanel.gameObject.SetActive(true);
            }
        }

        public void CloseResultImage()
        {
            if (resultImagePanel != null)
            {
                resultImagePanel.gameObject.SetActive(false);
            }
        }

        #region Helper Methods (Virtual Board Generation)

        private int[,] GenerateRandomBoard(int w, int h, int mines)
        {
            int[,] board = new int[w, h];
            System.Random rand = new System.Random();

            int placed = 0;
            while (placed < mines)
            {
                int rx = rand.Next(0, w);
                int ry = rand.Next(0, h);
                if (board[rx, ry] != -1)
                {
                    board[rx, ry] = -1;
                    placed++;
                }
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (board[x, y] == -1) continue;

                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < w && ny >= 0 && ny < h && board[nx, ny] == -1)
                            {
                                count++;
                            }
                        }
                    }
                    board[x, y] = count;
                }
            }
            return board;
        }

        private void FloodFillVirtual(int[,] real, int[,] visible, int x, int y, int w, int h, ref int revealedCount)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    {
                        if (visible[nx, ny] == -1)
                        {
                            visible[nx, ny] = real[nx, ny];
                            revealedCount++;
                            if (real[nx, ny] == 0)
                            {
                                FloodFillVirtual(real, visible, nx, ny, w, h, ref revealedCount);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}