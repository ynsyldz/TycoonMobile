using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GÜNCELLENDİ: Artık şehir noktalarını ve rota çizgilerini elle çizmiyor
/// (bunu artık gerçek harita görseli üstleniyor). Tek görevi: aktif
/// TransportTask'lara göre araç ikonlarını haritada başlangıçtan bitişe
/// doğru hareket ettirmek.
/// </summary>
public class MapRenderer : MonoBehaviour
{
    [Header("Harita Referansı")]
    [Tooltip("Gerçek harita görselini taşıyan RectTransform (MapContent). Pin ve araç konumları buna göre hesaplanır.")]
    public RectTransform mapRect;

    [Header("Visual Prefab")]
    [Tooltip("Haritada hareket edecek araç prefab'ı (VehicleUIRenderer script'ini taşımalı)")]
    public GameObject vehicleVisualPrefab;

    // Aktif görevleri ve onlara ait sahnede üretilmiş araç objelerini eşleştirir
    private readonly Dictionary<string, GameObject> activeVehicleVisuals = new Dictionary<string, GameObject>();

    private void Update()
    {
        if (TransportManager.Instance == null) return;

        List<TransportTask> activeTasks = TransportManager.Instance.GetActiveTasks();
        long currentUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 1. Yeni görevleri yakala ve araçları hareket ettir
        foreach (var task in activeTasks)
        {
            if (!activeVehicleVisuals.ContainsKey(task.taskId))
            {
                GameObject newVehicle = Instantiate(vehicleVisualPrefab, mapRect);
                newVehicle.name = $"Visual_{task.assignedVehicleName}";

                VehicleUIRenderer uiRenderer = newVehicle.GetComponent<VehicleUIRenderer>();
                if (uiRenderer != null)
                {
                    uiRenderer.Setup(task);
                }

                activeVehicleVisuals.Add(task.taskId, newVehicle);
            }

            GameObject vehicleObj = activeVehicleVisuals[task.taskId];
            RectTransform vehicleRect = vehicleObj.GetComponent<RectTransform>();
            float progress = task.GetProgress(currentUnixTime);

            // startCoord/endCoord artık normalize (0-1) koordinat olarak yorumlanıyor
            Vector2 startPos = MapCoordinateUtility.NormalizedToAnchoredPosition(mapRect, task.startCoord);
            Vector2 endPos = MapCoordinateUtility.NormalizedToAnchoredPosition(mapRect, task.endCoord);

            vehicleRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
        }

        // 2. Biten veya silinen görevlerin görsellerini temizle
        List<string> tasksToRemove = new List<string>();
        foreach (var kvp in activeVehicleVisuals)
        {
            if (!activeTasks.Exists(t => t.taskId == kvp.Key))
            {
                Destroy(kvp.Value);
                tasksToRemove.Add(kvp.Key);
            }
        }

        foreach (var id in tasksToRemove)
        {
            activeVehicleVisuals.Remove(id);
        }
    }
}