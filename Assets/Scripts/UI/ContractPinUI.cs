using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Haritada görünen tek bir dinamik ihale (Contract) pinini yönetir.
/// ContractUIItem'daki "SetupDynamic" ile aynı kabul mantığını kullanır,
/// böylece oyuncu ihaleyi ister liste panelinden ister doğrudan haritadan
/// tıklayarak kabul edebilir.
/// </summary>
public class ContractPinUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI rewardText;
    public Image timerFillImage; // Opsiyonel: dairesel geri sayım göstergesi

    [Header("Buttons")]
    public Button acceptButton;

    private Contract boundContract;
    private Action<Contract> onAcceptedCallback;

    public void Setup(Contract contract, Action<Contract> onAccepted)
    {
        boundContract = contract;
        onAcceptedCallback = onAccepted;

        if (rewardText != null)
            rewardText.text = $"x{contract.bonusMultiplier}";

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(HandleClick);

        transform.localScale = Vector3.zero;
        StartCoroutine(PopIn());
    }

    private System.Collections.IEnumerator PopIn()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
    }

    private void Update()
    {
        if (boundContract == null) return;

        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (timerFillImage != null)
        {
            // ContractManager'daki contractLifetimeSeconds'a göre normalize ediyoruz
            long remaining = boundContract.GetRemainingTime(currentUnixTime);
            long total = ContractManager.Instance != null ? ContractManager.Instance.contractLifetimeSeconds : 60;
            timerFillImage.fillAmount = total > 0 ? Mathf.Clamp01((float)remaining / total) : 0f;
        }
    }

    private void HandleClick()
    {
        var availableVehicles = VehicleManager.Instance.GetAvailableVehicles();

        if (availableVehicles.Count == 0)
        {
            Debug.LogWarning("[Contract Pin] Göreve gönderilecek müsait araç yok!");
            return;
        }

        // ContractUIItem.OnAcceptClicked ile birebir aynı kabul mantığı
        TransportManager.Instance.StartTask(availableVehicles[0], boundContract.route, boundContract.bonusMultiplier);
        ContractManager.Instance.activeContracts.Remove(boundContract);

        Debug.Log("[Contract Pin] Haritadan ihale kabul edildi!");
        onAcceptedCallback?.Invoke(boundContract);
    }
}