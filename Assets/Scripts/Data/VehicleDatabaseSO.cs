using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VehicleDatabase", menuName = "LogisticsTycoon/Vehicle Database", order = 0)]
public class VehicleDatabaseSO : ScriptableObject
{
    public List<VehicleDataSO> allVehicles;

    // ID'ye göre araç verisini bulup döndürür
    public VehicleDataSO GetVehicleByID(string id)
    {
        return allVehicles.Find(v => v.vehicleID == id);
    }
}