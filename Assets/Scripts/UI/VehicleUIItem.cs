using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI conditionText;
    public TextMeshProUGUI statusText;

    [Header("Buttons")]
    public Button upgradeButton;
    public Button repairButton;
    public Button driverButton;

    private OwnedVehicleData currentData;
    private GarageUIManager parentManager;

    public void Setup(OwnedVehicleData data, GarageUIManager manager)
    {
        currentData = data;
        parentManager = manager;
        Refresh();
    }

    public void Refresh()
    {
        VehicleDataSO dbData = VehicleManager.Instance.vehicleDatabase.GetVehicleByID(currentData.vehicleID);

        nameText.text = dbData.vehicleName;
        levelText.text = $"Seviye {currentData.upgradeLevel}";
        conditionText.text = $"Kondisyon: %{currentData.currentCondition:F0}";

        if (currentData.isBusy)
            statusText.text = "<color=orange>Görevde</color>";
        else if (currentData.hasDriver)
            statusText.text = "<color=green>Otonom (Bekliyor)</color>";
        else
            statusText.text = "<color=white>Müsait</color>";

        // Unity buton eventlerini kod üzerinden dinamik bağlıyoruz
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(OnRepairClicked);

        driverButton.onClick.RemoveAllListeners();
        driverButton.onClick.AddListener(OnDriverClicked);

        // Duruma göre butonları aktif/pasif yapma kuralları
        repairButton.interactable = currentData.currentCondition < 100f && !currentData.isBusy;
        driverButton.interactable = !currentData.hasDriver;
        upgradeButton.interactable = !currentData.isBusy;
    }

    private void OnUpgradeClicked()
    {
        if (VehicleManager.Instance.UpgradeOwnedVehicle(currentData.vehicleID))
            parentManager.RefreshGarage();
    }

    private void OnRepairClicked()
    {
        if (VehicleManager.Instance.RepairVehicle(currentData.instanceID))
            parentManager.RefreshGarage();
    }

    private void OnDriverClicked()
    {
        if (VehicleManager.Instance.HireDriver(currentData.instanceID))
            parentManager.RefreshGarage();
    }
}