using System;

[Serializable]
public class OwnedFacilityData
{
    public string facilityID;
    public int level;

    public OwnedFacilityData(string id)
    {
        facilityID = id;
        level = 1; // İnşa edildiğinde direkt 1. seviye olur
    }
}