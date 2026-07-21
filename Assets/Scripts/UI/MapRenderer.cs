using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [Header("Core References")]
    public MapManager mapManager;

    [Header("Visual Prefabs")]
    public GameObject nodePrefab;
    public GameObject linePrefab;
    [Tooltip("Haritada hareket edecek araç prefab'ı")]
    public GameObject vehicleVisualPrefab;

    [Header("Hierarchy")]
    public Transform mapContainer;

    // Aktif görevleri ve onlara ait sahnede üretilmiş araç objelerini eşleştirir
    private Dictionary<string, GameObject> activeVehicleVisuals = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (mapManager == null) mapManager = MapManager.Instance;
        DrawMap();
    }

    private void Update()
    {
        if (TransportManager.Instance == null) return;

        List<TransportTask> activeTasks = TransportManager.Instance.GetActiveTasks();
        long currentUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 1. Yeni görevleri yakala ve araçları hareket ettir
        foreach (var task in activeTasks)
        {
            // Görev için henüz bir görsel üretilmediyse üret
            if (!activeVehicleVisuals.ContainsKey(task.taskId))
            {
                GameObject newVehicle = Instantiate(vehicleVisualPrefab, mapContainer);
                newVehicle.name = $"Visual_{task.assignedVehicleName}";

                // YENİ: UI bileşenine Task verisini gönder
                VehicleUIRenderer uiRenderer = newVehicle.GetComponent<VehicleUIRenderer>();
                if (uiRenderer != null)
                {
                    uiRenderer.Setup(task);
                }

                activeVehicleVisuals.Add(task.taskId, newVehicle);
            }

            GameObject vehicleObj = activeVehicleVisuals[task.taskId];
            float progress = task.GetProgress(currentUnixTime);

            // Başlangıç ve bitiş noktalarını 3D vektöre çevir (Z ekseni -0.2f yapıldı ki çizgilerin üstünde dursun)
            Vector3 startPos = new Vector3(task.startCoord.x, task.startCoord.y, -0.2f);
            Vector3 endPos = new Vector3(task.endCoord.x, task.endCoord.y, -0.2f);

            // Aracı progress (0.0 ile 1.0 arası) değerine göre yolda kaydır
            vehicleObj.transform.position = Vector3.Lerp(startPos, endPos, progress);
        }

        // 2. Biten veya silinen görevlerin görsellerini temizle
        List<string> tasksToRemove = new List<string>();
        foreach (var kvp in activeVehicleVisuals)
        {
            if (!activeTasks.Exists(t => t.taskId == kvp.Key))
            {
                Destroy(kvp.Value); // Sahneden sil
                tasksToRemove.Add(kvp.Key); // Listeden çıkarmak için işaretle
            }
        }

        foreach (var id in tasksToRemove)
        {
            activeVehicleVisuals.Remove(id);
        }
    }

    // ... (DrawMap, DrawRouteLine ve DrawLocationNode metotları öncekiyle tamamen aynı kalacak) ...

    private void DrawMap()
    {
        if (mapManager.allRoutes == null || mapManager.allRoutes.Count == 0)
        {
            Debug.LogWarning("[MapRenderer] Çizilecek rota bulunamadı!");
            return;
        }

        // 1. Önce yolları (çizgileri) çiziyoruz ki pinlerin altında kalsınlar
        foreach (var route in mapManager.allRoutes)
        {
            if (route.startLocation != null && route.endLocation != null)
            {
                DrawRouteLine(route);
            }
        }

        // 2. Noktaları (şehirleri) çiziyoruz. Aynı şehri iki kez çizmemek için HashSet kullanıyoruz.
        HashSet<LocationDataSO> uniqueLocations = new HashSet<LocationDataSO>();
        foreach (var route in mapManager.allRoutes)
        {
            if (route.startLocation != null) uniqueLocations.Add(route.startLocation);
            if (route.endLocation != null) uniqueLocations.Add(route.endLocation);
        }

        foreach (var location in uniqueLocations)
        {
            DrawLocationNode(location);
        }

        Debug.Log($"[MapRenderer] Harita çizildi: {uniqueLocations.Count} Lokasyon, {mapManager.allRoutes.Count} Rota.");
    }

    private void DrawRouteLine(RouteDataSO route)
    {
        GameObject lineObj = Instantiate(linePrefab, mapContainer);
        lineObj.name = $"RouteLine_{route.GetRouteName()}";

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = 2; // Başlangıç ve Bitiş

        // Z eksenini 0'da tutarak 2D görünümü koruyoruz
        Vector3 startPos = new Vector3(route.startLocation.mapCoordinates.x, route.startLocation.mapCoordinates.y, 0f);
        Vector3 endPos = new Vector3(route.endLocation.mapCoordinates.x, route.endLocation.mapCoordinates.y, 0f);

        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
    }

    private void DrawLocationNode(LocationDataSO location)
    {
        Vector3 spawnPos = new Vector3(location.mapCoordinates.x, location.mapCoordinates.y, -0.1f); // Çizgilerin bir tık üstünde dursun
        GameObject nodeObj = Instantiate(nodePrefab, spawnPos, Quaternion.identity, mapContainer);
        nodeObj.name = $"Node_{location.locationName}";
    }
}