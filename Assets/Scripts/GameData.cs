using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public double money; // İleride sayılar çok büyüyeceği için float yerine double
    public long lastLoginTime;

    public List<TransportTask> activeTasks = new List<TransportTask>();
    public List<OwnedVehicleData> ownedVehicles = new List<OwnedVehicleData>();
    public List<OwnedFacilityData> ownedFacilities = new List<OwnedFacilityData>();
    public List<TaskHistoryData> taskHistory = new List<TaskHistoryData>();
    // YENİ: Toplam oyun istatistikleri
    public StatisticsData stats = new StatisticsData();
    public List<string> claimedAchievements = new List<string>();
    public int playerLevel;
    public int playerXP;
    public GameData()
    {
        money = 15000;
        playerLevel = 1; // Yeni oyuncu 1. seviyeden başlar
        playerXP = 0;
        lastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ownedVehicles.Add(new OwnedVehicleData("minivan_01"));
    }
}