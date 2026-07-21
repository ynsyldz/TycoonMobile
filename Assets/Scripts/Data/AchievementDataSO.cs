using UnityEngine;

public enum StatType
{
    TotalMoneyEarned,
    TotalTasksCompleted,
    TotalDistanceTraveled,
    TotalVehiclesBought
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "LogisticsTycoon/Achievement Data", order = 4)]
public class AchievementDataSO : ScriptableObject
{
    public string achievementID;
    public string title;
    public string description;

    [Header("Requirements")]
    public StatType targetStat;
    public float targetValue;

    [Header("Rewards")]
    public double rewardMoney;
    public int rewardXP;
}