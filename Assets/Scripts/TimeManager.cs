using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public GameData CurrentData { get; private set; }
    public long OfflineSecondsPassed { get; private set; }

    private void Awake()
    {
        // Temel Singleton yapısı
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDataAndTime();
    }

    private void InitializeDataAndTime()
    {
        CurrentData = SaveManager.Load();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        OfflineSecondsPassed = currentTime - CurrentData.lastLoginTime;

        // Negatif zaman (cihaz saati manipülasyonu) veya aşırı büyük süreleri engelleme (Max 8 saat idle sınırı - GDD kuralı)
        if (OfflineSecondsPassed < 0) OfflineSecondsPassed = 0;
        long maxIdleSeconds = 8 * 60 * 60; // 8 saat
        if (OfflineSecondsPassed > maxIdleSeconds) OfflineSecondsPassed = maxIdleSeconds;

        Debug.Log($"Oyuna dönüldü. Geçen offline süre: {OfflineSecondsPassed} saniye.");
    }

    private void OnApplicationQuit()
    {
        SaveManager.Save(CurrentData);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveManager.Save(CurrentData);
        }
    }
}