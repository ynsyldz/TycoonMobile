using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Available Routes in Game")]
    public List<RouteDataSO> allRoutes; // Oyundaki tüm rotalar burada tanımlanacak

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // İleride UI tarafı bu metodu çağırıp sadece oyuncunun seviyesine uygun rotaları listeleyecek
    public List<RouteDataSO> GetUnlockedRoutes(int playerLevel)
    {
        List<RouteDataSO> unlockedRoutes = new List<RouteDataSO>();
        foreach (var route in allRoutes)
        {
            if (playerLevel >= route.requiredLevel)
            {
                unlockedRoutes.Add(route);
            }
        }
        return unlockedRoutes;
    }

    // UI geliştiricisinin haritadaki açık rotaları listelerken kullanacağı API
    public List<RouteDataSO> GetAvailableRoutes()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return new List<RouteDataSO>();

        int currentLevel = TimeManager.Instance.CurrentData.playerLevel;
        return GetUnlockedRoutes(currentLevel); // Mevcut metodu kullanarak filtrele
    }
}