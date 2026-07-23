using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GÜNCELLENDİ: Artık world-space Transform.position yerine, gerçek harita
/// görselinin RectTransform'una göre normalize koordinat sistemi kullanıyor
/// (MapRenderer ile birebir aynı yöntem, MapCoordinateUtility üzerinden).
///
/// ContractManager.activeContracts listesindeki her dinamik ihale için
/// haritada (route.startLocation.mapCoordinates konumunda) bir pin
/// oluşturur/kaldırır.
/// </summary>
public class ContractPinRenderer : MonoBehaviour
{
    [Header("Harita Referansı")]
    [Tooltip("MapRenderer'daki ile aynı RectTransform (MapContent) olmalı.")]
    public RectTransform mapRect;

    [Header("Visual Prefab")]
    [Tooltip("Contract için haritada gösterilecek tıklanabilir pin prefabı (ContractPinUI script'ini taşımalı)")]
    public GameObject contractPinPrefab;

    private readonly Dictionary<string, GameObject> activePinVisuals = new Dictionary<string, GameObject>();

    private void Update()
    {
        if (ContractManager.Instance == null) return;

        List<Contract> activeContracts = ContractManager.Instance.activeContracts;

        foreach (var contract in activeContracts)
        {
            if (!activePinVisuals.ContainsKey(contract.contractID))
            {
                SpawnPin(contract);
            }
        }

        List<string> idsToRemove = new List<string>();
        foreach (var kvp in activePinVisuals)
        {
            bool stillActive = activeContracts.Exists(c => c.contractID == kvp.Key);
            if (!stillActive)
            {
                Destroy(kvp.Value);
                idsToRemove.Add(kvp.Key);
            }
        }
        foreach (var id in idsToRemove)
        {
            activePinVisuals.Remove(id);
        }
    }

    private void SpawnPin(Contract contract)
    {
        if (contract.route == null || contract.route.startLocation == null)
        {
            Debug.LogWarning("[Contract Pin Renderer] İhalenin rotasında başlangıç konumu tanımlı değil, pin oluşturulamadı.");
            return;
        }

        GameObject pinObj = Instantiate(contractPinPrefab, mapRect);
        pinObj.name = $"ContractPin_{contract.route.GetRouteName()}";

        RectTransform pinRect = pinObj.GetComponent<RectTransform>();
        pinRect.anchoredPosition = MapCoordinateUtility.NormalizedToAnchoredPosition(
            mapRect,
            contract.route.startLocation.mapCoordinates
        );

        ContractPinUI pinUI = pinObj.GetComponent<ContractPinUI>();
        if (pinUI != null)
        {
            pinUI.Setup(contract, OnPinAccepted);
        }

        activePinVisuals.Add(contract.contractID, pinObj);
    }

    private void OnPinAccepted(Contract contract)
    {
        // Pin kendi objesini silmiyor; Update() bir sonraki karede
        // activeContracts listesinden kalktığını görüp temizleyecek.
    }
}