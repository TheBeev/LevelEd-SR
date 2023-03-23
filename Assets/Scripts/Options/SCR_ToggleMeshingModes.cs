using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_ToggleMeshingModes : MonoBehaviour, IToolOptionMenu, IOnOffToolOption {

    private enum MeshingType { Meshing, Outline };

    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private MeshingType meshingModeName = MeshingType.Meshing;
    [SerializeField] private GameObject defaultOption;
    [SerializeField] private OptionActive meshingModeActive = OptionActive.On;
    [SerializeField] private string tempName;

    [Header("Transparency")]
    [SerializeField] private SCR_ToggleMeshingModes otherMeshingModeScript;
    [SerializeField] private Material defaultMeshingMaterial;
    [SerializeField] private Material performanceMeshingMaterial;

    public OptionActive MeshingModeActive
    {
        get { return meshingModeActive; }
    }

    private GameObject meshingTypeParent;
    private Renderer[] meshingRenderers;
    private bool bInPerformanceMode;
    private bool bOptionActive;

    List<TextMeshProUGUI> menuTextObjects = new List<TextMeshProUGUI>();

    public void DeactivateOption()
    {
        bOptionActive = false;

        foreach (var item in menuObjects)
        {
            item.layer = 2;
        }

        foreach (var item in menuTextObjects)
        {
            Color tempItemColour = item.color;
            tempItemColour.a = 0.3f;
            item.color = tempItemColour;
        }

        foreach (var item in menuObjects)
        {
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
        }

        Color tempColour = labelObject.color;
        tempColour.a = 0.3f;
        labelObject.color = tempColour;

        //snappingText.enabled = false;
    }

    public void ActivateOption()
    {
        bOptionActive = true;

        foreach (var item in menuObjects)
        {
            item.layer = 11;
        }

        foreach (var item in menuTextObjects)
        {
            item.color = Color.white;
        }

        foreach (var item in menuObjects)
        {
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
        }

        labelObject.color = Color.white;

    }

    void ToggleMeshingMode()
    {
        switch (meshingModeActive)
        {
            case OptionActive.On:
                if (meshingTypeParent)
                {
                    meshingTypeParent.SetActive(true);
                }
                break;
            case OptionActive.Off:
                if (meshingTypeParent)
                {
                    meshingTypeParent.SetActive(false);
                }
                break;
            default:
                break;
        }

        
    }

    public void ToggleStatus(OptionActive optionActive, GameObject referredObject)
    {
        meshingModeActive = optionActive;

        if (meshingTypeParent == null)
        {
            meshingTypeParent = GameObject.Find(tempName);
        }

        foreach (var item in menuObjects)
        {
            item.GetComponent<IToolOptionMenuItem>().Deselected();
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
        }

        CheckPerformance();
        ToggleMeshingMode();

        referredObject.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        referredObject.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
    }

    public void CheckPerformance()
    {

        if (meshingTypeParent == null)
        {
            meshingTypeParent = GameObject.Find(tempName);
        }

        if (meshingTypeParent)
        {
            if (meshingModeName == MeshingType.Meshing)
            {
                if (otherMeshingModeScript.MeshingModeActive == OptionActive.On && meshingModeActive == OptionActive.On)
                {
                    if (meshingRenderers == null)
                    {
                        meshingRenderers = new Renderer[meshingTypeParent.GetComponentsInChildren<Renderer>().Length];
                        meshingRenderers = meshingTypeParent.GetComponentsInChildren<Renderer>();
                    }

                    if (meshingRenderers.Length > 0)
                    {
                        foreach (var item in meshingRenderers)
                        {
                            item.material = performanceMeshingMaterial;
                        }
                    }

                }
                else if (meshingModeActive == OptionActive.On)
                {
                    if (meshingRenderers == null)
                    {
                        meshingRenderers = new Renderer[meshingTypeParent.GetComponentsInChildren<Renderer>().Length];
                        meshingRenderers = meshingTypeParent.GetComponentsInChildren<Renderer>();
                    }

                    if (meshingRenderers.Length > 0)
                    {
                        foreach (var item in meshingRenderers)
                        {
                            item.material = defaultMeshingMaterial;
                        }
                    }

                }
            }
            else if (meshingModeName == MeshingType.Outline)
            {
                otherMeshingModeScript.CheckPerformance();
            }
        }        
    }

    // Use this for initialization
    void Start ()
    {
        foreach (var item in menuObjects)
        {
            TextMeshProUGUI textItem = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textItem)
            {
                menuTextObjects.Add(textItem);
            }
        }

        tempName = "PivotPoint" + System.Enum.GetName(typeof(MeshingType), meshingModeName);

        defaultOption.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        defaultOption.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
    }
	
}
