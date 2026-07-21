using System;

[Serializable]
public class TaskHistoryData
{
    public string routeName;
    public string vehicleName;
    public double earnedMoney;
    public long completionTimeUnix;

    public TaskHistoryData(string route, string vehicle, double money)
    {
        routeName = route;
        vehicleName = vehicle;
        earnedMoney = money;
        completionTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}