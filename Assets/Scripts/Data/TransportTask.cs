using System;
using UnityEngine; // EKSİK OLAN SATIR BURASI

[Serializable]
public class TransportTask
{
    public string taskId;
    public string vehicleInstanceID;
    public string assignedVehicleName;
    public string routeName;

    // Haritada çizim yapabilmek için koordinatları saklıyoruz
    public Vector2 startCoord;
    public Vector2 endCoord;

    public float distance;
    public bool isAutomated;
    public int vehicleLevel;

    public float durationSeconds;
    public double expectedReward;
    public long startTimeUnix;
    public bool isCompleted;

    public TransportTask(OwnedVehicleData ownedVehicle, VehicleDataSO vehicleData, RouteDataSO route, bool automated = false, float contractMultiplier = 1.0f)
    {
        taskId = Guid.NewGuid().ToString();
        vehicleInstanceID = ownedVehicle.instanceID;
        assignedVehicleName = vehicleData.vehicleName;
        routeName = route.GetRouteName();

        // Koordinatları kopyala
        startCoord = route.startLocation.mapCoordinates;
        endCoord = route.endLocation.mapCoordinates;

        distance = route.routeDistance;
        isAutomated = automated;
        vehicleLevel = ownedVehicle.upgradeLevel;

        float hqBonus = FacilityManager.Instance.GetGlobalIncomeMultiplier();
        float garageDiscount = FacilityManager.Instance.GetGlobalFuelMultiplier();

        float effectiveCapacity = EconomyCalculator.GetEffectiveCapacity(vehicleData.capacity, vehicleData.capacityGrowthRate, vehicleLevel);
        float effectiveSpeed = EconomyCalculator.GetEffectiveSpeed(vehicleData.speed, vehicleData.speedGrowthRate, vehicleLevel);

        durationSeconds = EconomyCalculator.CalculateTaskDuration(distance, effectiveSpeed);
        double grossReward = EconomyCalculator.CalculateTaskReward(effectiveCapacity, distance, route.zoneMultiplier, durationSeconds);

        grossReward = grossReward * contractMultiplier * hqBonus;

        double fuelCost = EconomyCalculator.CalculateFuelCost(distance, vehicleData.fuelEfficiency);
        fuelCost = fuelCost * garageDiscount;

        expectedReward = grossReward - fuelCost;
        startTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        isCompleted = false;
    }

    public float GetRemainingTime(long currentUnixTime)
    {
        if (isCompleted) return 0f;
        long passedTime = currentUnixTime - startTimeUnix;
        float remaining = durationSeconds - passedTime;
        return remaining > 0 ? remaining : 0f;
    }

    public float GetProgress(long currentUnixTime)
    {
        if (isCompleted) return 1f;

        float remainingSeconds = GetRemainingTime(currentUnixTime);
        float passedSeconds = durationSeconds - remainingSeconds;

        if (durationSeconds <= 0) return 1f;

        return passedSeconds / durationSeconds;
    }

    public string GetFormattedRemainingTime(long currentUnixTime)
    {
        if (isCompleted) return "00:00";

        float remaining = GetRemainingTime(currentUnixTime);
        TimeSpan time = TimeSpan.FromSeconds(remaining);

        if (time.TotalHours >= 1)
            return time.ToString(@"hh\:mm\:ss");
        else
            return time.ToString(@"mm\:ss");
    }
}