using UnityEngine;
using System.Collections.Generic;

public class FacilitiesUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject facilityItemPrefab;
    public Transform contentContainer;

    private List<FacilityUIItem> spawnedItems = new List<FacilityUIItem>();

    private void OnEnable()
    {
        FacilityManager.OnFacilityUpgraded += HandleFacilityUpgraded;
        InitializeList();
    }

    private void OnDisable()
    {
        FacilityManager.OnFacilityUpgraded -= HandleFacilityUpgraded;
    }

    private void HandleFacilityUpgraded(FacilityDataSO data, int newLevel)
    {
        RefreshAllItems();
    }

    private void InitializeList()
    {
        if (FacilityManager.Instance == null || FacilityManager.Instance.allFacilities == null) return;

        // Temizle
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedItems.Clear();

        // Veritabanındaki tüm tesisleri listele
        foreach (FacilityDataSO facilitySO in FacilityManager.Instance.allFacilities)
        {
            GameObject go = Instantiate(facilityItemPrefab, contentContainer);
            FacilityUIItem uiItem = go.GetComponent<FacilityUIItem>();

            if (uiItem != null)
            {
                uiItem.Setup(facilitySO, this);
                spawnedItems.Add(uiItem);
            }
        }
    }

    // Harici bakiyeler değiştiğinde (örn: görev bittiğinde) paneli tazelemek için açık tutulan public metot
    public void RefreshAllItems()
    {
        foreach (var item in spawnedItems)
        {
            item.RefreshUI();
        }
    }
}