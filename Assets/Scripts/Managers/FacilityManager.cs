using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityManager : MonoBehaviour
{
    public static FacilityManager Instance { get; private set; }

    [Header("Facility Database")]
    public List<FacilityDataSO> allFacilities;

    // UI tarafı tesis geliştiğinde ekranı güncellemek için bu event'i dinleyecek
    public static event Action<FacilityDataSO, int> OnFacilityUpgraded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool UpgradeFacility(string facilityID)
    {
        FacilityDataSO facilityData = allFacilities.Find(f => f.facilityID == facilityID);
        if (facilityData == null)
        {
            Debug.LogError($"[FacilityManager] Tesis bulunamadı: {facilityID}");
            return false;
        }

        // Tesis önceden satın alınmış mı kontrol et
        OwnedFacilityData owned = TimeManager.Instance.CurrentData.ownedFacilities.Find(f => f.facilityID == facilityID);

        // Hiç alınmadıysa 0. seviye sayıyoruz
        int currentLevel = owned != null ? owned.level : 0;

        // Yükseltme maliyeti formülü: Baz Maliyet * (Çarpan ^ Mevcut Seviye)
        double upgradeCost = facilityData.baseCost * Mathf.Pow(facilityData.costMultiplier, currentLevel);

        if (TimeManager.Instance.CurrentData.money >= upgradeCost)
        {
            TimeManager.Instance.CurrentData.money -= upgradeCost;

            if (owned == null)
            {
                owned = new OwnedFacilityData(facilityID);
                TimeManager.Instance.CurrentData.ownedFacilities.Add(owned);
                Debug.Log($"[FacilityManager] {facilityData.facilityName} inşa edildi!");
            }
            else
            {
                owned.level++;
                Debug.Log($"[FacilityManager] {facilityData.facilityName} Seviye {owned.level} oldu!");
            }

            SaveManager.Save(TimeManager.Instance.CurrentData);
            Debug.Log($"[FacilityManager] Kalan Para: {TimeManager.Instance.CurrentData.money}");

            OnFacilityUpgraded?.Invoke(facilityData, owned.level);
            return true;
        }
        else
        {
            Debug.LogWarning($"[FacilityManager] Yetersiz bakiye! Gerekli: {upgradeCost}, Mevcut: {TimeManager.Instance.CurrentData.money}");
            return false;
        }
    }

    // Belirtilen türdeki tesisin güncel seviyesini bulur
    public int GetFacilityLevel(FacilityType type)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return 0;

        FacilityDataSO dataSO = allFacilities.Find(f => f.type == type);
        if (dataSO == null) return 0;

        OwnedFacilityData owned = TimeManager.Instance.CurrentData.ownedFacilities.Find(f => f.facilityID == dataSO.facilityID);
        return owned != null ? owned.level : 0;
    }

    // HQ Seviyesine göre global gelir çarpanı (Örn: Seviye 1 = 1.05x, Seviye 2 = 1.10x)
    public float GetGlobalIncomeMultiplier()
    {
        int hqLevel = GetFacilityLevel(FacilityType.HQ);
        return 1f + (hqLevel * 0.05f); // Her seviye %5 ekstra kazanç
    }

    // Garaj Seviyesine göre yakıt maliyeti çarpanı (Örn: Seviye 1 = 0.95x, Seviye 2 = 0.90x)
    public float GetGlobalFuelMultiplier()
    {
        int garageLevel = GetFacilityLevel(FacilityType.Garage);
        float discount = garageLevel * 0.05f; // Her seviye %5 indirim

        // Yakıt indirimi %50'yi geçmesin (Oyun dengesi için hard-cap)
        if (discount > 0.5f) discount = 0.5f;

        return 1f - discount;
    }

    // --- TEST KISMI ---
    [Header("Test Data")]
    public string testFacilityID = "hq_01";

    [ContextMenu("Test Tesis Geliştir")]
    public void TestUpgradeFacility()
    {
        UpgradeFacility(testFacilityID);
    }
}