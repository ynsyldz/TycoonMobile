using System;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    // UI tarafı seviye atlandığında ekranı güncellemek için bu eventi dinleyecek
    public static event Action<int> OnLevelUp;

    [Header("XP Settings")]
    [Tooltip("Seviye 2'ye geçmek için gereken taban XP")]
    public int baseXPRequirement = 500;

    [Tooltip("Her seviyede gereken XP miktarının artış çarpanı")]
    public float xpMultiplierPerLevel = 1.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        TimeManager.Instance.CurrentData.playerXP += amount;
        Debug.Log($"[Progression] +{amount} XP kazanıldı. Toplam XP: {TimeManager.Instance.CurrentData.playerXP}");

        CheckLevelUp();
        SaveManager.Save(TimeManager.Instance.CurrentData);
    }

    private void CheckLevelUp()
    {
        int currentLevel = TimeManager.Instance.CurrentData.playerLevel;
        int xpRequiredForNext = GetXPForNextLevel(currentLevel);

        // Kazanılan XP birden fazla seviye atlatmaya yetiyorsa (Offline kazanımlar için while döngüsü)
        while (TimeManager.Instance.CurrentData.playerXP >= xpRequiredForNext)
        {
            TimeManager.Instance.CurrentData.playerXP -= xpRequiredForNext;
            TimeManager.Instance.CurrentData.playerLevel++;
            currentLevel = TimeManager.Instance.CurrentData.playerLevel;

            Debug.Log($"[Progression] 🌟 SEVİYE ATLADIN! Yeni Seviye: {currentLevel}");
            OnLevelUp?.Invoke(currentLevel);

            xpRequiredForNext = GetXPForNextLevel(currentLevel);
        }
    }

    // İleride UI'da Progress Bar (XP Bar) doldurmak için gerekli olacak saf matematik fonksiyonu
    public int GetXPForNextLevel(int level)
    {
        // Formül: BaseXP * (Multiplier ^ (Seviye - 1))
        return Mathf.RoundToInt(baseXPRequirement * Mathf.Pow(xpMultiplierPerLevel, level - 1));
    }

    // --- TEST KISMI ---
    [ContextMenu("Test 1000 XP Ekle")]
    public void TestAddXP()
    {
        AddXP(1000);
    }
}