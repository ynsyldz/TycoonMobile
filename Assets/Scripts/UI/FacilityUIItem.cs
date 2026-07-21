using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FacilityUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private FacilityDataSO currentData;
    private FacilitiesUIManager parentManager;

    public void Setup(FacilityDataSO data, FacilitiesUIManager manager)
    {
        currentData = data;
        parentManager = manager;

        nameText.text = data.facilityName;
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        RefreshUI();
    }

    public void RefreshUI()
    {
        // Güncel seviyeyi backend'den çek
        int currentLevel = FacilityManager.Instance.GetFacilityLevel(currentData.type);

        // Seviye yazısını güncelle
        levelText.text = currentLevel == 0 ? "İnşa Edilmedi" : $"Seviye {currentLevel}";

        // Backend'deki ile aynı formülü kullanarak maliyeti hesapla
        double upgradeCost = currentData.baseCost * Mathf.Pow(currentData.costMultiplier, currentLevel);
        costText.text = $"${upgradeCost:F1}";

        // Bakiye yetmiyorsa butonu inaktif yap
        upgradeButton.interactable = TimeManager.Instance.CurrentData.money >= upgradeCost;
    }

    private void OnUpgradeClicked()
    {
        FacilityManager.Instance.UpgradeFacility(currentData.facilityID);
    }
}