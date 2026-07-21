using UnityEngine;

public enum FacilityType
{
    HQ,
    Hub,
    Garage
}

[CreateAssetMenu(fileName = "NewFacility", menuName = "LogisticsTycoon/Facility Data", order = 3)]
public class FacilityDataSO : ScriptableObject
{
    public string facilityID;
    public string facilityName;
    public FacilityType type;

    [Header("Economy")]
    [Tooltip("İlk inşa etme veya birinci seviyeye getirme maliyeti")]
    public double baseCost;

    [Tooltip("Her seviyede yükseltme maliyetinin katlanma oranı")]
    public float costMultiplier = 1.5f;
}