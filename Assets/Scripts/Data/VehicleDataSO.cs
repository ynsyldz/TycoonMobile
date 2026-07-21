using UnityEngine;

// Araç türlerini tanımlıyoruz
public enum VehicleType
{
    Minivan,
    Kamyonet,
    Kamyon,
    Tir
}

[CreateAssetMenu(fileName = "NewVehicleData", menuName = "LogisticsTycoon/Vehicle Data", order = 1)]
public class VehicleDataSO : ScriptableObject
{
    [Header("General Info")]
    public string vehicleID;
    public string vehicleName;
    public VehicleType vehicleType;
    public Sprite vehicleIcon; // İleride UI veya harita üzerinde kullanılacak 2D görsel

    [Header("Core Stats")]
    [Tooltip("Taşıyabileceği maksimum yük miktarı")]
    public float capacity;

    [Tooltip("Haritadaki hareket hızı (Birim/Saniye)")]
    public float speed;

    [Tooltip("Birim mesafe başına düşen maliyet çarpanı (Yakıt verimliliği)")]
    public float fuelEfficiency;

    [Header("Economy")]
    [Tooltip("Aracın satın alma maliyeti")]
    public double baseCost;

    [Header("Maintenance")]
    [Tooltip("Her 1 birim mesafede kondisyon kaç puan düşer? (Örn: 0.05)")]
    public float conditionLossPerDistance = 0.05f;

    [Tooltip("Kondisyonu %0'dan %100'e çıkarmanın maksimum maliyeti")]
    public double maxMaintenanceCost = 500;

    [Header("Automation")]
    [Tooltip("Bu araca şoför atamanın (otomasyon) tek seferlik maliyeti")]
    public double driverHireCost = 2500;

    [Tooltip("Aracın hangi seviyeden sonra satın alınabileceği")]
    public int unlockLevel;

    [Header("Upgrade Settings")]
    [Tooltip("İlk seviye atlatma maliyeti")]
    public double baseUpgradeCost = 1000;
    [Tooltip("Her seviyede upgrade maliyeti ne kadar katlanacak?")]
    public float upgradeCostMultiplier = 1.2f;
    [Tooltip("Her seviyede kapasite ne kadar artacak? (Örn: 0.1 = %10)")]
    public float capacityGrowthRate = 0.1f;
    [Tooltip("Her seviyede hız ne kadar artacak? (Örn: 0.05 = %5)")]
    public float speedGrowthRate = 0.05f;
}