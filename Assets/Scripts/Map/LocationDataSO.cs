using UnityEngine;

[CreateAssetMenu(fileName = "NewLocation", menuName = "LogisticsTycoon/Map/Location Data", order = 1)]
public class LocationDataSO : ScriptableObject
{
    public string locationName;

    [Tooltip("İleride Google Maps veya 2D harita üzerinde pin koymak için kullanılacak koordinat (Enlem/Boylam veya X/Y)")]
    public Vector2 mapCoordinates;
}