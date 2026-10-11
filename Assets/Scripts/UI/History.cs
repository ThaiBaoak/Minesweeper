using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace AI{
public class History : MonoBehaviour
{
    [SerializeField] private GameObject historyItemPrefab;
    [SerializeField] private Transform historyContent;
    [SerializeField] private ScrollRect scrollRect;
    private int historyCount = 0;

    public void AddHistoryItem(AIActionData actionData)
    {
        if (historyItemPrefab == null || historyContent == null)
        {
            Debug.LogWarning("History item prefab or content is not assigned.");
            return;
        }

        historyCount++;
        GameObject newItem = Instantiate(historyItemPrefab, historyContent);

        string actionText = actionData.action == AIAction.Reveal ? "<color=green>Mở</color>" : "<color=yellow>Cắm cờ</color>";
        string logText = $"Bước {historyCount}: {actionText} tại ({actionData.x}, {actionData.y})";
        TextMeshProUGUI textComponent = newItem.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = logText;
        }
            else
            {
                Text textComp = newItem.GetComponentInChildren<Text>();
                if (textComp != null)
                {
                    textComp.text = logText;
                }
                else
                {
                    Debug.LogWarning("No Text or TextMeshProUGUI component found in the history item prefab.");
                }
            }

        if(scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void ClearHistory()
    {
        foreach (Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }
        historyCount = 0;
    }
}
}
