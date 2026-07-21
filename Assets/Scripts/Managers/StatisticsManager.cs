using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // İlgili Manager'ların Event'lerine abone ol
    private void OnEnable()
    {
        TransportManager.OnTaskCompleted += HandleTaskCompleted;
        VehicleManager.OnVehiclePurchased += HandleVehiclePurchased;
    }

    private void OnDisable()
    {
        TransportManager.OnTaskCompleted -= HandleTaskCompleted;
        VehicleManager.OnVehiclePurchased -= HandleVehiclePurchased;
    }

    private void HandleTaskCompleted(TransportTask task)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return;

        StatisticsData currentStats = TimeManager.Instance.CurrentData.stats;

        currentStats.totalTasksCompleted++;
        currentStats.totalDistanceTraveled += task.distance;

        // Sadece pozitif net kazancı toplam gelire ekle
        if (task.expectedReward > 0)
        {
            currentStats.totalMoneyEarned += task.expectedReward;
        }

        SaveManager.Save(TimeManager.Instance.CurrentData);
        Debug.Log($"[Statistics] İstatistikler güncellendi. Toplam Sefer: {currentStats.totalTasksCompleted}, Toplam Mesafe: {currentStats.totalDistanceTraveled:F1}");
    }

    private void HandleVehiclePurchased(VehicleDataSO vehicle)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return;

        TimeManager.Instance.CurrentData.stats.totalVehiclesBought++;
        SaveManager.Save(TimeManager.Instance.CurrentData);
    }
}