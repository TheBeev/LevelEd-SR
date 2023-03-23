using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_GridSnappingOption : MonoBehaviour, IToolOptionMenu, IOnOffToolOption {

    public static SCR_GridSnappingOption instance;

    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private TextMeshProUGUI snappingText;
    [SerializeField] private float snapSize = 0.1f;
    [SerializeField] private GameObject defaultOption;
    [SerializeField] private OptionActive snappingActive = OptionActive.On;

    public OptionActive SnappingActive
    {
        get { return snappingActive; }
    }

    List<TextMeshProUGUI> menuTextObjects = new List<TextMeshProUGUI>();

    private bool bOptionActive;

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
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
        }

        Color tempColour = labelObject.color;
        tempColour.a = 0.3f;
        labelObject.color = tempColour;

        snappingText.enabled = false;
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
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
        }

        labelObject.color = Color.white;

        SetSnappingText();

        snappingText.enabled = true;
    }

    void SetSnappingText()
    {
        switch (snappingActive)
        {
            case OptionActive.On:
                snappingText.text = "Snapping";
                break;
            case OptionActive.Off:
                snappingText.text = "Not Snapping";
                break;
            default:
                break;
        }
    }

    public void ToggleStatus(OptionActive optionActive, GameObject referredObject)
    {
        snappingActive = optionActive;

        foreach (var item in menuObjects)
        {
            item.GetComponent<IToolOptionMenuItem>().Deselected();
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
        }

        SetSnappingText();

        referredObject.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        referredObject.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
    }

    //adapted from Weimann (2017) https://unity3d.college/2017/10/08/simple-unity3d-snap-grid-system/
    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        position -= Vector3.zero;

        int xCount = Mathf.RoundToInt(position.x / snapSize);
        int yCount = Mathf.RoundToInt(position.y / snapSize);
        int zCount = Mathf.RoundToInt(position.z / snapSize);

        Vector3 result = new Vector3((float)xCount * snapSize, (float)yCount * snapSize, (float)zCount * snapSize);

        result += Vector3.zero;

        return result;
    }

    void Awake()
    {
        instance = this;
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

        //defaultOption.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
        defaultOption.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        defaultOption.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
    }
	
}
