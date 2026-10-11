using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEndModal : MonoBehaviour
{
    public static GameEndModal Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image modalImagePanel; // Image nền của bảng kết quả
    [SerializeField] private TextMeshProUGUI txtCombinedResult; // TextMeshPro hiển thị toàn bộ thông tin

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tự động ẩn bảng thông báo khi bắt đầu game
        if (modalImagePanel != null)
        {
            modalImagePanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hiển thị bảng kết quả Win/Lose kèm thời gian có sẵn
    /// </summary>
    /// <param name="isWin">true: Thắng, false: Thua</param>
    /// <param name="playedTimeSeconds">Thời gian chơi (tính bằng giây) có sẵn từ GameManager/Timer</param>
    public void ShowGameEnd(bool isWin, float playedTimeSeconds)
    {
        // Lấy thông số cấu hình bàn cờ hiện tại từ GameManager
        int width = 9;
        int height = 9;
        int mines = 10;

        if (GameManager.Instance != null)
        {
            width = GameManager.Instance.Width;
            height = GameManager.Instance.Height;
            mines = GameManager.Instance.MineCount;
        }

        // Định dạng tiêu đề và câu thông báo
        string titleText = isWin ? "<color=#00FF88><b>VICTORY!</b></color>" : "<color=red><b>GAME OVER!</b></color>";
        string statusText = isWin ? "Bạn đã dọn sạch toàn bộ mìn!" : "Bạn đã giậm phải mìn!";

        // Quy đổi thời gian từ giây sang định dạng Phút : Giây (mm:ss)
        int minutes = Mathf.FloorToInt(playedTimeSeconds / 60f);
        int seconds = Mathf.FloorToInt(playedTimeSeconds % 60f);
        string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Gộp toàn bộ thông tin hiển thị lên 1 TextMeshPro
        string reportText = $"<size=130%>{titleText}</size>\n" +
                            $"<size=85%>{statusText}</size>\n" +
                            $"-----------------------------------------\n" +
                            $"• Kích thước: <b>{width} x {height}</b>\n" +
                            $"• Số mìn: <b>{mines} mìn</b>\n" +
                            $"• Thời gian chơi: <color=#FFD700><b>{timeFormatted}</b></color> ({playedTimeSeconds:F1}s)";

        if (txtCombinedResult != null)
        {
            txtCombinedResult.text = reportText;
        }

        // Bật Modal Image
        if (modalImagePanel != null)
        {
            modalImagePanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Gắn hàm này vào Nút "Chơi Lại" hoặc "Đóng" trên Modal UI
    /// </summary>
    public void OnClickRestartOrClose()
    {
        if (modalImagePanel != null)
        {
            modalImagePanel.gameObject.SetActive(false);
        }

        // Gọi Restart game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}