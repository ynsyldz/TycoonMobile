using UnityEngine;

[CreateAssetMenu(fileName = "NewLocation", menuName = "LogisticsTycoon/Map/Location Data", order = 1)]
public class LocationDataSO : ScriptableObject
{
    public string locationName;

    [Tooltip("GÜNCELLENDİ: Artık piksel/dünya koordinatı DEĞİL. Gerçek harita " +
             "görselinin (MapContent) sol-alt köşesi (0,0), sağ-üst köşesi (1,1) " +
             "kabul edilerek bu şehrin haritadaki normalize (0-1 arası) konumu.")]
    public Vector2 mapCoordinates;
}