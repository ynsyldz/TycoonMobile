using System.Collections.Generic;
using UnityEngine;

public class GarageUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject vehicleUIItemPrefab;
    public Transform contentContainer;

    // Panel her görünür olduğunda listeyi yenile
    private void OnEnable()
    {
        RefreshGarage();
    }

    public void RefreshGarage()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return;

        // Önce listedeki eski görselleri sil (Duplicate olmaması için)
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Oyuncunun sahip olduğu araçları çek ve listele
        List<OwnedVehicleData> ownedVehicles = TimeManager.Instance.CurrentData.ownedVehicles;
        foreach (var vehicle in ownedVehicles)
        {
            GameObject newObj = Instantiate(vehicleUIItemPrefab, contentContainer);
            VehicleUIItem uiItem = newObj.GetComponent<VehicleUIItem>();

            if (uiItem != null)
            {
                uiItem.Setup(vehicle, this);
            }
        }
    }
}