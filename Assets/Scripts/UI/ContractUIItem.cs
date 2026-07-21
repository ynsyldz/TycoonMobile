using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContractUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI routeNameText;
    public TextMeshProUGUI detailsText;
    public TextMeshProUGUI timerText;

    [Header("Buttons")]
    public Button acceptButton;

    private Contract dynamicContract;
    private RouteDataSO standardRoute;
    private ContractsUIManager parentManager;

    private bool isDynamic;

    // --- 1. DİNAMİK İHALE KURULUMU ---
    public void SetupDynamic(Contract contract, ContractsUIManager manager)
    {
        isDynamic = true;
        dynamicContract = contract;
        parentManager = manager;

        routeNameText.text = contract.route.GetRouteName();
        detailsText.text = $"Mesafe: {contract.route.routeDistance} km | <color=#FFA500>Kazanç: x{contract.bonusMultiplier}</color>";

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(OnAcceptClicked);
    }

    // --- 2. STANDART ROTA KURULUMU ---
    public void SetupStandard(RouteDataSO route, ContractsUIManager manager)
    {
        isDynamic = false;
        standardRoute = route;
        parentManager = manager;

        routeNameText.text = route.GetRouteName();
        detailsText.text = $"Mesafe: {route.routeDistance} km | <color=#FFFFFF>Kazanç: Standart</color>";
        timerText.text = "<color=#A9A9A9>Sabit Rota</color>"; // Sayaç yerine sabit yazı

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(OnAcceptClicked);
    }

    private void Update()
    {
        // Eğer sabit rotaysa sayaca gerek yok
        if (!isDynamic || dynamicContract == null) return;

        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (dynamicContract.IsExpired(currentUnixTime))
        {
            timerText.text = "<color=red>Süresi Doldu</color>";
            acceptButton.interactable = false;
            return;
        }

        long timeRemaining = dynamicContract.GetRemainingTime(currentUnixTime);
        timerText.text = $"Kalan: {timeRemaining} sn";
    }

    private void OnAcceptClicked()
    {
        var availableVehicles = VehicleManager.Instance.GetAvailableVehicles();

        if (availableVehicles.Count > 0)
        {
            if (isDynamic)
            {
                // Dinamik: Çarpanı gönder ve listeden sil
                TransportManager.Instance.StartTask(availableVehicles[0], dynamicContract.route, dynamicContract.bonusMultiplier);
                ContractManager.Instance.activeContracts.Remove(dynamicContract);
                Debug.Log($"[Contracts UI] Dinamik ihale kabul edildi!");
            }
            else
            {
                // Standart: Sadece başlat, listeden silme (Sabit kalır)
                TransportManager.Instance.StartTask(availableVehicles[0], standardRoute, 1.0f);
                Debug.Log($"[Contracts UI] Standart rota kabul edildi!");
            }

            parentManager.RefreshContracts();
        }
        else
        {
            Debug.LogWarning("[Contracts UI] Göreve gönderilecek müsait veya hasarsız araç yok!");
        }
    }
}