using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(GameData data)
    {
        // Çıkış yaparken o anki zamanı kaydet
        data.lastLoginTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Oyun Kaydedildi: " + SavePath);
    }

    public static GameData Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameData>(json);
        }

        // Kayıt yoksa yeni oyun verisi oluştur
        Debug.Log("Kayıt bulunamadı, yeni oyun başlatılıyor.");
        return new GameData();
    }
}