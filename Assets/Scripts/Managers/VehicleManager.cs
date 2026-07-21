using System;
using System.Collections.Generic; // EKSİK OLAN SATIR BURASI
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance { get; private set; }

    public VehicleDatabaseSO vehicleDatabase;

    public static event Action<VehicleDataSO> OnVehiclePurchased;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool BuyVehicle(string vehicleID)
    {
        VehicleDataSO vehicleToBuy = vehicleDatabase.GetVehicleByID(vehicleID);

        if (vehicleToBuy == null)
        {
            Debug.LogError($"[VehicleManager] Veritabanında araç bulunamadı: {vehicleID}");
            return false;
        }

        // Seviye Kontrolü (Validation)
        int currentLevel = TimeManager.Instance.CurrentData.playerLevel;
        if (currentLevel < vehicleToBuy.unlockLevel)
        {
            Debug.LogWarning($"[VehicleManager] Kilitli Araç! {vehicleToBuy.vehicleName} için Seviye {vehicleToBuy.unlockLevel} gerekiyor. Mevcut Seviyeniz: {currentLevel}");
            return false;
        }

        if (TimeManager.Instance.CurrentData.money >= vehicleToBuy.baseCost)
        {
            TimeManager.Instance.CurrentData.money -= vehicleToBuy.baseCost;

            OwnedVehicleData newVehicle = new OwnedVehicleData(vehicleID);
            TimeManager.Instance.CurrentData.ownedVehicles.Add(newVehicle);

            SaveManager.Save(TimeManager.Instance.CurrentData);

            Debug.Log($"[VehicleManager] Satın alım başarılı! {vehicleToBuy.vehicleName} garaja eklendi.");
            OnVehiclePurchased?.Invoke(vehicleToBuy);
            return true;
        }

        Debug.LogWarning($"[VehicleManager] Yetersiz bakiye! Gerekli: {vehicleToBuy.baseCost:F1}");
        return false;
    }

    public bool UpgradeOwnedVehicle(string vehicleID)
    {
        OwnedVehicleData owned = TimeManager.Instance.CurrentData.ownedVehicles.Find(v => v.vehicleID == vehicleID);
        if (owned == null)
        {
            Debug.LogError($"[VehicleManager] Envanterde bu araç yok: {vehicleID}");
            return false;
        }

        VehicleDataSO data = vehicleDatabase.GetVehicleByID(vehicleID);
        double baseCost = EconomyCalculator.GetUpgradeCost(data.baseUpgradeCost, data.upgradeCostMultiplier, owned.upgradeLevel);

        // YENİ: Garaj seviyesine göre indirim uygula
        double cost = baseCost * FacilityManager.Instance.GetGlobalFuelMultiplier();

        if (TimeManager.Instance.CurrentData.money >= cost)
        {
            TimeManager.Instance.CurrentData.money -= cost;
            owned.upgradeLevel++;

            Debug.Log($"[VehicleManager] {data.vehicleName} Seviye {owned.upgradeLevel} oldu! Kesilen: {cost:F1}, Kalan: {TimeManager.Instance.CurrentData.money:F1}");
            SaveManager.Save(TimeManager.Instance.CurrentData);
            return true;
        }

        Debug.LogWarning($"[VehicleManager] Yetersiz bakiye! Gerekli: {cost:F1}");
        return false;
    }

    // UI geliştiricisinin mağazayı listelerken kullanacağı API
    public List<VehicleDataSO> GetUnlockedVehicles()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return new List<VehicleDataSO>();

        int currentLevel = TimeManager.Instance.CurrentData.playerLevel;
        return vehicleDatabase.allVehicles.FindAll(v => currentLevel >= v.unlockLevel);
    }

    // --- TEST KISMI ---
    [Header("Test Data")]
    public string testBuyVehicleID = "kamyon_01";

    [ContextMenu("Test Araç Satın Al")]
    public void TestBuyVehicle()
    {
        BuyVehicle(testBuyVehicleID);
    }

    [ContextMenu("Test Envanterdeki İlk Aracı Geliştir")]
    public void TestUpgradeFirstVehicle()
    {
        if (TimeManager.Instance.CurrentData.ownedVehicles.Count > 0)
        {
            UpgradeOwnedVehicle(TimeManager.Instance.CurrentData.ownedVehicles[0].vehicleID);
        }
    }

    // İstenilen ID'ye sahip, garajdaki somut aracı bulur
    public OwnedVehicleData GetOwnedVehicleInstance(string instanceID)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return null;
        return TimeManager.Instance.CurrentData.ownedVehicles.Find(v => v.instanceID == instanceID);
    }

    // UI geliştiricisi görev başlatma ekranında sadece bu listeyi gösterecek
    public List<OwnedVehicleData> GetAvailableVehicles()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return new List<OwnedVehicleData>();

        // Sadece meşgul OLMAYAN araçları getir
        return TimeManager.Instance.CurrentData.ownedVehicles.FindAll(v => !v.isBusy);
    }

    // Araca kalıcı olarak şoför atar
    public bool HireDriver(string instanceID)
    {
        OwnedVehicleData owned = GetOwnedVehicleInstance(instanceID);
        if (owned == null) return false;

        if (owned.hasDriver)
        {
            Debug.LogWarning("[VehicleManager] Bu aracın zaten bir şoförü var!");
            return false;
        }

        VehicleDataSO data = vehicleDatabase.GetVehicleByID(owned.vehicleID);

        if (TimeManager.Instance.CurrentData.money >= data.driverHireCost)
        {
            TimeManager.Instance.CurrentData.money -= data.driverHireCost;
            owned.hasDriver = true;

            Debug.Log($"[VehicleManager] Şoför kiralandı! Araç artık otonom çalışabilir. Kesilen: {data.driverHireCost:F1}, Kalan Para: {TimeManager.Instance.CurrentData.money:F1}");
            SaveManager.Save(TimeManager.Instance.CurrentData);
            return true;
        }

        Debug.LogWarning($"[VehicleManager] Şoför için yetersiz bakiye! Gerekli: {data.driverHireCost:F1}");
        return false;
    }

    [ContextMenu("Test İlk Araca Şoför Kirala")]
    public void TestHireDriver()
    {
        List<OwnedVehicleData> vehicles = TimeManager.Instance.CurrentData.ownedVehicles;
        if (vehicles.Count > 0) HireDriver(vehicles[0].instanceID);
        else Debug.LogWarning("Garajda araç yok!");
    }

    // Araca bakım yapar. Kondisyon ne kadar düşükse, maliyet o kadar yüksek olur.
    public bool RepairVehicle(string instanceID)
    {
        OwnedVehicleData owned = GetOwnedVehicleInstance(instanceID);
        if (owned == null) return false;

        if (owned.isBusy)
        {
            Debug.LogWarning("[VehicleManager] Araç şu an görevde, tamir edilemez!");
            return false;
        }

        if (owned.currentCondition >= 100f)
        {
            Debug.LogWarning("[VehicleManager] Araç zaten sapasağlam!");
            return false;
        }

        VehicleDataSO data = vehicleDatabase.GetVehicleByID(owned.vehicleID);

        float damagePercentage = (100f - owned.currentCondition) / 100f;
        double baseRepairCost = data.maxMaintenanceCost * damagePercentage;

        // YENİ: Garaj seviyesine göre tamir indirimi uygula
        double repairCost = baseRepairCost * FacilityManager.Instance.GetGlobalFuelMultiplier();

        if (TimeManager.Instance.CurrentData.money >= repairCost)
        {
            TimeManager.Instance.CurrentData.money -= repairCost;
            owned.currentCondition = 100f;

            Debug.Log($"[VehicleManager] Araç tamir edildi! Ödenen: {repairCost:F1}, Kalan Para: {TimeManager.Instance.CurrentData.money:F1}");
            SaveManager.Save(TimeManager.Instance.CurrentData);
            return true;
        }

        Debug.LogWarning($"[VehicleManager] Tamir için yetersiz bakiye! Gerekli: {repairCost:F1}");
        return false;
    }

    [ContextMenu("Test İlk Aracı Tamir Et")]
    public void TestRepairVehicle()
    {
        List<OwnedVehicleData> vehicles = TimeManager.Instance.CurrentData.ownedVehicles;
        if (vehicles.Count > 0) RepairVehicle(vehicles[0].instanceID);
        else Debug.LogWarning("Garajda araç yok!");
    }
}