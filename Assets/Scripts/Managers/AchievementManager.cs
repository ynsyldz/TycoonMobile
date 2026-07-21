using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public List<AchievementDataSO> allAchievements;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // UI'da ilerleme çubuğunu doldurmak için mevcut değeri döndürür
    public float GetCurrentStatValue(StatType statType)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return 0f;

        StatisticsData stats = TimeManager.Instance.CurrentData.stats;
        switch (statType)
        {
            case StatType.TotalMoneyEarned: return (float)stats.totalMoneyEarned;
            case StatType.TotalTasksCompleted: return stats.totalTasksCompleted;
            case StatType.TotalDistanceTraveled: return stats.totalDistanceTraveled;
            case StatType.TotalVehiclesBought: return stats.totalVehiclesBought;
            default: return 0f;
        }
    }

    // Başarımın tamamlanıp tamamlanmadığını kontrol eder
    public bool IsAchievementReadyToClaim(AchievementDataSO achievement)
    {
        if (TimeManager.Instance.CurrentData.claimedAchievements.Contains(achievement.achievementID))
            return false; // Zaten alınmış

        float currentValue = GetCurrentStatValue(achievement.targetStat);
        return currentValue >= achievement.targetValue;
    }

    // UI'daki "Ödülü Al" butonuna bağlanacak metot
    public bool ClaimAchievement(string achievementID)
    {
        if (TimeManager.Instance.CurrentData.claimedAchievements.Contains(achievementID))
        {
            Debug.LogWarning("[Achievement] Bu ödül zaten alındı!");
            return false;
        }

        AchievementDataSO achievement = allAchievements.Find(a => a.achievementID == achievementID);
        if (achievement == null) return false;

        if (GetCurrentStatValue(achievement.targetStat) >= achievement.targetValue)
        {
            // Ödülleri ver
            TimeManager.Instance.CurrentData.money += achievement.rewardMoney;
            ProgressionManager.Instance.AddXP(achievement.rewardXP);

            // Başarımı kaydedilenler listesine ekle
            TimeManager.Instance.CurrentData.claimedAchievements.Add(achievementID);
            SaveManager.Save(TimeManager.Instance.CurrentData);

            Debug.Log($"[Achievement] Başarım Tamamlandı: {achievement.title}! Kazanılan: {achievement.rewardMoney} Para, {achievement.rewardXP} XP");
            return true;
        }

        Debug.LogWarning("[Achievement] Şartlar henüz sağlanmadı!");
        return false;
    }

    // --- TEST KISMI ---
    [Header("Test Data")]
    public string testAchievementID = "ach_first_steps";

    [ContextMenu("Test Başarım Ödülü Al")]
    public void TestClaimAchievement()
    {
        ClaimAchievement(testAchievementID);
    }
}