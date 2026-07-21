using System;

[Serializable]
public class OwnedVehicleData
{
    public string instanceID;
    public string vehicleID;
    public int upgradeLevel;
    public bool isBusy;
    public bool hasDriver;
    public float currentCondition; // YENİ: Aracın mevcut sağlığı (0-100)

    public OwnedVehicleData(string id)
    {
        instanceID = Guid.NewGuid().ToString();
        vehicleID = id;
        upgradeLevel = 1;
        isBusy = false;
        hasDriver = false;
        currentCondition = 100f; // Fabrikadan %100 sağlam çıkar
    }
}