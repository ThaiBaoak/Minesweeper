using UnityEngine;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private GameObject panelGameObject;
    [SerializeField] private TMP_Dropdown dropdownDifficulty;
    [SerializeField] private TMP_InputField inputAIDelay;
    [SerializeField] private TMP_InputField inputBenchmarkRuns;

    private const string PREF_DIFFICULTY = "SelectedDifficulty";

    private int tempDifficulty;
    private float tempAIDelay;
    private int tempBenchmarkRuns;
    private bool isInitializing = false;

    private void Start()
    {
        SyncDropdownToSavedValue();
    }

    private void OnEnable()
    {
        SyncDropdownToSavedValue();
    }

    public void SyncDropdownToSavedValue()
    {
        isInitializing = true;

        int savedDifficulty = PlayerPrefs.GetInt(PREF_DIFFICULTY, 0);
        tempDifficulty = savedDifficulty;

        if (dropdownDifficulty != null)
        {
            dropdownDifficulty.value = savedDifficulty;
            dropdownDifficulty.RefreshShownValue();
        }

        if (GameManager.Instance != null)
        {
            tempAIDelay = GameManager.Instance.DelayTime;
            tempBenchmarkRuns = GameManager.Instance.BenchmarkTotalRuns;

            if (inputAIDelay != null)
                inputAIDelay.text = tempAIDelay.ToString("F2");

            if (inputBenchmarkRuns != null)
                inputBenchmarkRuns.text = tempBenchmarkRuns.ToString(); // Đưa đúng số ván hiện tại lên ô Input
        }

        isInitializing = false;
        Debug.Log($"<color=cyan>[SettingsPanel] Đã gán Dropdown Value = {savedDifficulty}</color>");
    }

    public void OnDifficultyChanged(int difficultyIndex)
    {
        if (isInitializing) return;
        tempDifficulty = difficultyIndex;
    }

    public void OnAIDelayChanged(string value)
    {
        if (isInitializing) return;
        if (float.TryParse(value, out float delay))
        {
            tempAIDelay = delay;
        }
    }

    public void OnBenchmarkRunsChanged(string value)
    {
        if (isInitializing) return;
        if (int.TryParse(value, out int runs))
        {
            tempBenchmarkRuns = runs;
        }
    }

    public void OnClickApplyAndRestart()
    {
        if (GameManager.Instance != null)
        {
            // Cập nhật toàn bộ giá trị tạm vào GameManager & PlayerPrefs
            GameManager.Instance.SetDifficulty(tempDifficulty);
            GameManager.Instance.SetDelayTime(tempAIDelay);
            GameManager.Instance.SetBenchmarkTotalRuns(tempBenchmarkRuns);

            ClosePanel();
            GameManager.Instance.RestartGame();
        }
    }

    public void OpenPanel()
    {
        
        if (panelGameObject != null) panelGameObject.SetActive(true);
        SyncDropdownToSavedValue();
    }

    public void ClosePanel()
    {
        if (panelGameObject != null) panelGameObject.SetActive(false);
    }
}