using System;
using System.Collections.Generic;
using UnityEngine;

public class TransportManager : MonoBehaviour
{
    public static TransportManager Instance { get; private set; }

    public static event Action<TransportTask> OnTaskStarted;
    public static event Action<TransportTask> OnTaskCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        EvaluateOfflineProgress();
    }

    private void Update()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return;

        List<TransportTask> tasks = TimeManager.Instance.CurrentData.activeTasks;
        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            TransportTask task = tasks[i];

            if (!task.isCompleted && task.GetRemainingTime(currentUnixTime) <= 0)
            {
                CompleteTask(task);

                if (task.isAutomated)
                {
                    task.startTimeUnix = currentUnixTime;
                    task.isCompleted = false;
                    Debug.Log($"[Transport] Otomatik Görev Yeniden Başladı: {task.assignedVehicleName}");
                    SaveManager.Save(TimeManager.Instance.CurrentData);
                }
                else
                {
                    tasks.RemoveAt(i);
                }
            }
        }
    }

    private void EvaluateOfflineProgress()
    {
        long offlineSeconds = TimeManager.Instance.OfflineSecondsPassed;
        if (offlineSeconds <= 0) return;

        List<TransportTask> tasks = TimeManager.Instance.CurrentData.activeTasks;
        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        double totalOfflineEarnings = 0;

        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            TransportTask task = tasks[i];

            if (task.GetRemainingTime(currentUnixTime) <= 0)
            {
                // Göreve atanmış aracı ve veritabanı bilgilerini bul
                OwnedVehicleData vehicle = VehicleManager.Instance.GetOwnedVehicleInstance(task.vehicleInstanceID);
                VehicleDataSO vData = vehicle != null ? VehicleManager.Instance.vehicleDatabase.GetVehicleByID(vehicle.vehicleID) : null;

                if (task.isAutomated)
                {
                    long timeSinceStart = currentUnixTime - task.startTimeUnix;
                    int completedCycles = (int)(timeSinceStart / task.durationSeconds);

                    if (completedCycles > 0)
                    {
                        // Offline otomasyon döngüsünde her tur için parayı ver ve kondisyonu düşür
                        for (int cycle = 0; cycle < completedCycles; cycle++)
                        {
                            totalOfflineEarnings += task.expectedReward;

                            if (vehicle != null && vData != null)
                            {
                                vehicle.currentCondition -= (task.distance * vData.conditionLossPerDistance);

                                // Kondisyon %10 veya altına düştüyse arıza riski vardır, otomasyonu durdur
                                if (vehicle.currentCondition <= 10f)
                                {
                                    vehicle.currentCondition = Mathf.Max(0f, vehicle.currentCondition);
                                    task.isAutomated = false;
                                    vehicle.isBusy = false; // Aracı serbest bırak
                                    tasks.RemoveAt(i); // Görevi listeden sil
                                    Debug.LogWarning($"[Offline] {task.assignedVehicleName} arızalandı. Offline otomasyon durduruldu.");
                                    break; // Kalan turları iptal et
                                }
                            }
                        }

                        // Eğer araç tüm turları sağ salim bitirdiyse ve otomasyon devam ediyorsa, başlangıç zamanını ileri sar
                        if (task.isAutomated)
                        {
                            task.startTimeUnix += (long)(completedCycles * task.durationSeconds);
                        }
                    }
                }
                else
                {
                    // Standart (Otomasyonsuz) Görev Tamamlanması
                    totalOfflineEarnings += task.expectedReward;
                    task.isCompleted = true;

                    // Aracı serbest bırak ve kondisyonunu düşür
                    if (vehicle != null && vData != null)
                    {
                        vehicle.currentCondition -= (task.distance * vData.conditionLossPerDistance);
                        vehicle.currentCondition = Mathf.Max(0f, vehicle.currentCondition);
                        vehicle.isBusy = false; // KRİTİK DÜZELTME: Araç araftan kurtarıldı
                    }

                    tasks.RemoveAt(i);
                }
            }
        }

        if (totalOfflineEarnings > 0)
        {
            TimeManager.Instance.CurrentData.money += totalOfflineEarnings;
            int offlineXP = Mathf.RoundToInt((float)totalOfflineEarnings * 0.1f);
            ProgressionManager.Instance.AddXP(offlineXP);

            Debug.Log($"[Offline Progress] Oyun kapalıyken kazanılan toplam para: {totalOfflineEarnings:F1}, XP: {offlineXP}");
            SaveManager.Save(TimeManager.Instance.CurrentData);
        }
    }

    // GÜNCELLENMİŞ STARTTASK METODU (contractMultiplier eklendi)
    public void StartTask(OwnedVehicleData ownedVehicle, RouteDataSO route, float contractMultiplier = 1.0f)
    {
        if (ownedVehicle.isBusy)
        {
            Debug.LogWarning($"[Transport] Bu araç zaten görevde! İşlem iptal edildi.");
            return;
        }

        // YENİ: Kondisyon Kontrolü
        if (ownedVehicle.currentCondition <= 10f)
        {
            Debug.LogWarning($"[Transport] Aracın kondisyonu çok düşük (%{ownedVehicle.currentCondition:F1}). Göreve çıkmadan önce tamir edilmeli!");
            // Şoförü varsa bile otomasyonu durdur
            ownedVehicle.isBusy = false;
            return;
        }

        VehicleDataSO vehicleData = VehicleManager.Instance.vehicleDatabase.GetVehicleByID(ownedVehicle.vehicleID);
        bool isAutomated = ownedVehicle.hasDriver;

        TransportTask newTask = new TransportTask(ownedVehicle, vehicleData, route, isAutomated, contractMultiplier);
        TimeManager.Instance.CurrentData.activeTasks.Add(newTask);

        ownedVehicle.isBusy = true;

        Debug.Log($"[Transport] Görev Başladı! Araç: {vehicleData.vehicleName}, Rota: {newTask.routeName}, Kondisyon: %{ownedVehicle.currentCondition:F1}");

        OnTaskStarted?.Invoke(newTask);
        SaveManager.Save(TimeManager.Instance.CurrentData);
    }

    // TransportManager.cs içindeki CompleteTask metodunu bununla değiştir
    private void CompleteTask(TransportTask task)
    {
        task.isCompleted = true;

        // YENİ: HQ Seviyesine göre global gelir çarpanını uygula
        double incomeMultiplier = FacilityManager.Instance.GetGlobalIncomeMultiplier();
        double finalReward = task.expectedReward * incomeMultiplier;

        TimeManager.Instance.CurrentData.money += finalReward;
        int xpEarned = Mathf.RoundToInt((float)finalReward * 0.1f);

        OwnedVehicleData vehicle = VehicleManager.Instance.GetOwnedVehicleInstance(task.vehicleInstanceID);
        if (vehicle != null)
        {
            VehicleDataSO vData = VehicleManager.Instance.vehicleDatabase.GetVehicleByID(vehicle.vehicleID);
            vehicle.currentCondition -= (task.distance * vData.conditionLossPerDistance);
            if (vehicle.currentCondition < 0f) vehicle.currentCondition = 0f;

            if (!task.isAutomated || vehicle.currentCondition <= 10f)
            {
                if (task.isAutomated && vehicle.currentCondition <= 10f)
                {
                    Debug.LogWarning($"[Transport] {task.assignedVehicleName} arızalanmak üzere! Şoför döngüyü durdurdu.");
                    task.isAutomated = false;
                }
                vehicle.isBusy = false;
            }
        }

        ProgressionManager.Instance.AddXP(xpEarned);
        Debug.Log($"[Transport] {task.routeName} Teslimatı Tamamlandı! Kazanılan: {finalReward:F1} (Bonus: x{incomeMultiplier:F2}). Yeni Bakiye: {TimeManager.Instance.CurrentData.money:F1}");

        OnTaskCompleted?.Invoke(task);
        SaveManager.Save(TimeManager.Instance.CurrentData);
    }

    // --- TEST KISMI ---
    [Header("Test Data")]
    public VehicleDataSO testVehicle;
    public RouteDataSO testRoute;

    [ContextMenu("Test Görev Başlat (Müsait İlk Araçla)")]
    public void TestStartTask()
    {
        if (testRoute != null)
        {
            List<OwnedVehicleData> availableVehicles = VehicleManager.Instance.GetAvailableVehicles();
            if (availableVehicles.Count > 0)
            {
                // Artık manuel true/false göndermiyoruz
                StartTask(availableVehicles[0], testRoute);
            }
            else
            {
                Debug.LogWarning("[Transport Test] Garajda müsait araç yok!");
            }
        }
        else Debug.LogError("Eksik Test Rotası!");
    }

    // UI geliştiricisinin ekrandaki aktif görevleri listelerken kullanacağı güvenli API
    public List<TransportTask> GetActiveTasks()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null)
            return new List<TransportTask>();

        return TimeManager.Instance.CurrentData.activeTasks;
    }
}