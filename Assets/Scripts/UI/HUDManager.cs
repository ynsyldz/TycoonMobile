using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Money UI")]
    public TextMeshProUGUI moneyText;

    [Header("Level & XP UI")]
    public TextMeshProUGUI levelText;
    public Image xpFillBar;
    public TextMeshProUGUI xpText;

    [Header("Optimization")]
    [Tooltip("UI'ın saniyede kaç saniyede bir güncelleneceği (Performans için)")]
    public float refreshRate = 0.1f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= refreshRate)
        {
            timer = 0f;
            UpdateHUD();
        }
    }

    private void UpdateHUD()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.CurrentData == null) return;
        if (ProgressionManager.Instance == null) return;

        GameData data = TimeManager.Instance.CurrentData;

        // Para Formatı (Örn: $15,400)
        if (moneyText != null)
        {
            moneyText.text = $"${data.money:N0}";
        }

        // Seviye
        if (levelText != null)
        {
            levelText.text = $"Lvl {data.playerLevel}";
        }

        // XP Bar ve Metni
        if (xpFillBar != null && xpText != null)
        {
            int currentXP = data.playerXP;
            int requiredXP = ProgressionManager.Instance.GetXPForNextLevel(data.playerLevel);

            // Eğer seviye hesaplamasında bir hata olmazsa progress'i hesapla
            if (requiredXP > 0)
            {
                xpFillBar.fillAmount = (float)currentXP / requiredXP;
                xpText.text = $"{currentXP} / {requiredXP} XP";
            }
        }
    }
}