using UnityEngine;

/// <summary>
/// Harita üzerindeki normalize (0-1 aralığında) koordinatları, gerçek harita
/// RectTransform'unun boyutuna ve pivot noktasına göre doğru anchoredPosition
/// değerine çevirir. MapRenderer ve ContractPinRenderer aynı hesaplamayı
/// kullanır, böylece pin'ler ve araçlar her zaman birbiriyle tutarlı konumda olur.
/// </summary>
public static class MapCoordinateUtility
{
    public static Vector2 NormalizedToAnchoredPosition(RectTransform mapRect, Vector2 normalizedPosition)
    {
        Vector2 mapSize = mapRect.rect.size;
        Vector2 pivot = mapRect.pivot;

        return new Vector2(
            (normalizedPosition.x - pivot.x) * mapSize.x,
            (normalizedPosition.y - pivot.y) * mapSize.y
        );
    }
}