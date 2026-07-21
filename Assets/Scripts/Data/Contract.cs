using System;

[Serializable]
public class Contract
{
    public string contractID;
    public RouteDataSO route;
    public float bonusMultiplier;
    public long expirationUnixTime;

    public Contract(RouteDataSO targetRoute, float multiplier, int lifetimeSeconds)
    {
        contractID = Guid.NewGuid().ToString();
        route = targetRoute;
        bonusMultiplier = multiplier;
        // Şu anki zamana yaşam süresini ekleyerek son kullanma tarihini belirliyoruz
        expirationUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + lifetimeSeconds;
    }

    public bool IsExpired(long currentUnixTime)
    {
        return currentUnixTime >= expirationUnixTime;
    }

    // YENİ: UI'da sayacı geriye saydırmak için kalan süreyi saniye cinsinden döndürür
    public long GetRemainingTime(long currentUnixTime)
    {
        long remaining = expirationUnixTime - currentUnixTime;
        return remaining > 0 ? remaining : 0;
    }
}