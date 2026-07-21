using System.Collections.Generic;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    public static HistoryManager Instance { get; private set; }

    [Header("Settings")]
    public int maxHistoryCount = 50; // Save dosyasının şişmemesi için limit

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Event dinleyicilerini (Listener) aç
    private void OnEnable()
    {
        TransportManager.OnTaskCompleted += HandleTaskCompleted;
    }

    // Obje kapanırsa dinleyicileri kapat (Memory Leak önlemi)
    private void OnDisable()
    {
        TransportManager.OnTaskCompleted -= HandleTaskCompleted;
    }

    // TransportManager'dan görev bittiğinde otomatik tetiklenir
    private void HandleTaskCompleted(TransportTask task)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return;

        TaskHistoryData historyItem = new TaskHistoryData(task.routeName, task.assignedVehicleName, task.expectedReward);
        List<TaskHistoryData> historyList = TimeManager.Instance.CurrentData.taskHistory;

        // Yeni kaydı listenin başına ekle (UI tarafında en güncel olan en üstte listelensin diye)
        historyList.Insert(0, historyItem);

        // Limiti aşarsa sondakileri (en eskileri) sil
        if (historyList.Count > maxHistoryCount)
        {
            historyList.RemoveRange(maxHistoryCount, historyList.Count - maxHistoryCount);
        }

        SaveManager.Save(TimeManager.Instance.CurrentData);
        Debug.Log($"[HistoryManager] Geçmişe eklendi: {historyItem.vehicleName} | {historyItem.routeName}. Toplam kayıt: {historyList.Count}");
    }

    // UI ekibi için güvenli okuma API'si
    public List<TaskHistoryData> GetHistory()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return new List<TaskHistoryData>();
        return TimeManager.Instance.CurrentData.taskHistory;
    }
}