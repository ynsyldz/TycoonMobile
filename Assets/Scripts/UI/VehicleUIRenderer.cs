using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro için

public class VehicleUIRenderer : MonoBehaviour
{
    [Header("UI Components")]
    public Image progressBarFill;
    public TextMeshProUGUI timerText;

    private TransportTask currentTask;

    // MapRenderer bu aracı oluşturduğunda veriyi buraya enjekte edecek
    public void Setup(TransportTask task)
    {
        currentTask = task;
    }

    private void Update()
    {
        if (currentTask == null) return;

        long currentUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // İlerleme çubuğunu güncelle (0.0 ile 1.0 arası)
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = currentTask.GetProgress(currentUnixTime);
        }

        // Kalan süreyi metin olarak yaz (Örn: 01:15)
        if (timerText != null)
        {
            timerText.text = currentTask.GetFormattedRemainingTime(currentUnixTime);
        }
    }
}