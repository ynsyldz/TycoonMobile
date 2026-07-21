using UnityEngine;

public static class EconomyCalculator
{
    // Süre = Mesafe / Hız
    public static float CalculateTaskDuration(float distance, float speed)
    {
        if (speed <= 0) return 0f;
        return distance / speed;
    }

    // GDD Formülü: Kazanç = (Kapasite × Mesafe × Bölge Katsayısı) / Süre
    public static double CalculateTaskReward(float capacity, float distance, float zoneMultiplier, float duration)
    {
        if (duration <= 0) return 0;
        return (capacity * distance * zoneMultiplier) / duration;
    }

    // Yakıt maliyeti (gelirden düşülecek)
    public static double CalculateFuelCost(float distance, float fuelEfficiency)
    {
        return distance * fuelEfficiency;
    }

    // Efektif Kapasite = Taban Kapasite * (1 + (Seviye-1) * Büyüme Oranı)
    public static float GetEffectiveCapacity(float baseCapacity, float growthRate, int currentLevel)
    {
        return baseCapacity * (1f + ((currentLevel - 1) * growthRate));
    }

    // Efektif Hız = Taban Hız * (1 + (Seviye-1) * Büyüme Oranı)
    public static float GetEffectiveSpeed(float baseSpeed, float growthRate, int currentLevel)
    {
        return baseSpeed * (1f + ((currentLevel - 1) * growthRate));
    }

    // Geliştirme Maliyeti = Taban Maliyet * (Çarpan ^ (Seviye-1))
    public static double GetUpgradeCost(double baseCost, float multiplier, int currentLevel)
    {
        return baseCost * Mathf.Pow(multiplier, currentLevel - 1);
    }
}