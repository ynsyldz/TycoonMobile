using System.Collections.Generic;
using UnityEngine;

public enum PanelType
{
    Map,
    Garage,
    Contracts,
    Facilities
}

[System.Serializable]
public class UIPanelConfig
{
    public PanelType panelType;
    public GameObject panelObject;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panel Configuration")]
    public List<UIPanelConfig> panels;

    [Header("Settings")]
    public PanelType startingPanel = PanelType.Map;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Oyun başladığında varsayılan paneli aç
        OpenPanel(startingPanel);
    }

    // Merkezi panel açma metodu
    public void OpenPanel(PanelType type)
    {
        foreach (var p in panels)
        {
            if (p.panelObject != null)
            {
                // İstenen panel ise aktif et, değilse kapat
                p.panelObject.SetActive(p.panelType == type);
            }
        }

        Debug.Log($"[UIManager] Aktif Panel: {type}");
    }

    // Unity Button OnClick eventleri için yardımcı (Wrapper) metotlar
    public void OpenMap() => OpenPanel(PanelType.Map);
    public void OpenGarage() => OpenPanel(PanelType.Garage);
    public void OpenContracts() => OpenPanel(PanelType.Contracts);
    public void OpenFacilities() => OpenPanel(PanelType.Facilities);
}