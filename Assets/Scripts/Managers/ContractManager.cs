using System;
using System.Collections.Generic;
using UnityEngine;

public class ContractManager : MonoBehaviour
{
    public static ContractManager Instance { get; private set; }

    public List<Contract> activeContracts = new List<Contract>();

    [Header("Settings")]
    public float spawnCheckInterval = 10f; // Her 10 saniyede bir ihale çıkma ihtimalini kontrol et
    [Range(0f, 1f)]
    public float spawnChance = 0.3f; // %30 ihtimalle çıkar
    public int contractLifetimeSeconds = 60; // İhale 1 dakika boyunca kabul edilebilir kalır
    public float contractBonusMultiplier = 2.5f; // İhaleler normalden 2.5 kat fazla kazandırır

    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 1. Süresi dolan ihaleleri temizle (Listeyi kaydırmamak için sondan başa)
        for (int i = activeContracts.Count - 1; i >= 0; i--)
        {
            if (activeContracts[i].IsExpired(currentUnixTime))
            {
                Debug.Log($"[ContractManager] İhale kaçırıldı! Rota: {activeContracts[i].route.GetRouteName()}");
                activeContracts.RemoveAt(i);
            }
        }

        // 2. Yeni ihale spawn mekanizması
        timer += Time.deltaTime;
        if (timer >= spawnCheckInterval)
        {
            timer = 0;
            if (UnityEngine.Random.value <= spawnChance)
            {
                TrySpawnContract();
            }
        }
    }

    private void TrySpawnContract()
    {
        if (MapManager.Instance == null || MapManager.Instance.allRoutes.Count == 0) return;

        // Haritadaki rastgele bir rotayı seç
        int randomIndex = UnityEngine.Random.Range(0, MapManager.Instance.allRoutes.Count);
        RouteDataSO randomRoute = MapManager.Instance.allRoutes[randomIndex];

        Contract newContract = new Contract(randomRoute, contractBonusMultiplier, contractLifetimeSeconds);
        activeContracts.Add(newContract);

        Debug.Log($"[ContractManager] 🔥 YENİ İHALE! Rota: {randomRoute.GetRouteName()}, Çarpan: x{contractBonusMultiplier}, Geçerlilik: {contractLifetimeSeconds} sn");
    }

    // --- TEST KISMI ---
    [ContextMenu("Test İhale Üret")]
    public void TestSpawnContract()
    {
        TrySpawnContract();
    }
}