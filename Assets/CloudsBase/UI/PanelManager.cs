using System.Collections.Generic;
using Clouds.Ultilities;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{
    [SerializeField] protected List<GameObject> ListPanels;
    public string Curpanel;

    protected virtual void LoadComponents()
    {
        LoadListpanel();
    }

    protected void Start()
    {
        Application.targetFrameRate = 60;
    }

    public List<GameObject> GetListPanels() => ListPanels;

    protected void LoadListpanel()
    {
        if (ListPanels.Count > 0) return;
        foreach (Transform panel in transform)
            ListPanels.Add(panel.gameObject);
    }

    protected void OnEnable()
    {
        if (ListPanels.Count > 0) Curpanel = ListPanels[0].name;
    }

    public void ReturntoMainMenu()
    {
        DeActiveAll();
        if (ListPanels.Count > 0) ListPanels[0].SetActive(true);
    }

    public void DeActiveAll()
    {
        foreach (var panel in ListPanels)
            panel.SetActive(false);
    }

    public Transform GetPanelbyName(string panelname)
    {
        Curpanel = panelname;
        foreach (var panel in ListPanels)
        {
            if (panel.name == panelname) return panel.transform;
        }
        Debug.Log(transform.name + " Cant found " + panelname);
        return null;
    }

    public Transform DeActivePanel(string panelname)
    {
        foreach (var panel in ListPanels)
        {
            if (panel.name == panelname)
            {
                panel.SetActive(false);
                return panel.transform;
            }
        }
        Debug.Log(transform.name + " Cant found " + panelname);
        return null;
    }
}
