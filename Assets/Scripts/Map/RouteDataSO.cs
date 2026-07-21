using UnityEngine;

[CreateAssetMenu(fileName = "NewRoute", menuName = "LogisticsTycoon/Map/Route Data", order = 2)]
public class RouteDataSO : ScriptableObject
{
    public LocationDataSO startLocation;
    public LocationDataSO endLocation;

    [Tooltip("Görev süresi ve kazancı belirleyecek asıl mesafe değeri")]
    public float routeDistance;

    [Tooltip("GDD: 1-5 İlçe, 6-15 Şehir. Bu rotanın açılması için gereken minimum oyuncu seviyesi.")]
    public int requiredLevel = 1;

    [Tooltip("GDD'deki bölge katsayısı. Uzun/zor rotalarda kazancı katlamak için (Varsayılan: 1)")]
    public float zoneMultiplier = 1.0f;

    // UI'da rotayı yazdırırken kolaylık sağlaması için
    public string GetRouteName()
    {
        if (startLocation != null && endLocation != null)
        {
            // Eğer inspector'da locationName alanı boş bırakıldıysa, dosyanın kendi adını (.name) kullan
            string startName = string.IsNullOrEmpty(startLocation.locationName) ? startLocation.name : startLocation.locationName;
            string endName = string.IsNullOrEmpty(endLocation.locationName) ? endLocation.name : endLocation.locationName;

            return $"{startName} -> {endName}";
        }
        return "Geçersiz Rota";
    }
}