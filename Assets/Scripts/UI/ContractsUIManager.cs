using UnityEngine;

public class ContractsUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject contractUIItemPrefab;
    public Transform contentContainer;

    private int lastContractCount = -1;

    private void OnEnable()
    {
        RefreshContracts();
    }

    private void Update()
    {
        if (ContractManager.Instance == null) return;

        // Performans optimizasyonu: Sadece aktif ihale sayısı değişirse UI listesini yeniden oluştur
        if (ContractManager.Instance.activeContracts.Count != lastContractCount)
        {
            RefreshContracts();
        }
    }

    public void RefreshContracts()
    {
        if (ContractManager.Instance == null || MapManager.Instance == null) return;

        lastContractCount = ContractManager.Instance.activeContracts.Count;

        // Eski listeyi temizle
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 1. Önce Fırsat İhalelerini (Dinamik) en üste diz
        foreach (var contract in ContractManager.Instance.activeContracts)
        {
            GameObject newObj = Instantiate(contractUIItemPrefab, contentContainer);
            ContractUIItem uiItem = newObj.GetComponent<ContractUIItem>();

            if (uiItem != null)
            {
                uiItem.SetupDynamic(contract, this);
            }
        }

        // 2. Ardından Sabit Rotaları (Standart) alta diz
        foreach (var route in MapManager.Instance.allRoutes)
        {
            GameObject newObj = Instantiate(contractUIItemPrefab, contentContainer);
            ContractUIItem uiItem = newObj.GetComponent<ContractUIItem>();

            if (uiItem != null)
            {
                uiItem.SetupStandard(route, this);
            }
        }
    }
}