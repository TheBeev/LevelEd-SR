using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum AxisSelected { XAxis, YAxis, ZAxis, None };

public class SCR_AxisOption : MonoBehaviour, IToolOptionMenu, IAxisToolOption {

    public static SCR_AxisOption instance;

    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private TextMeshProUGUI axisText;
    [SerializeField] private GameObject defaultOption;

    [SerializeField] private AxisSelected currentAxis = AxisSelected.None;
    public AxisSelected CurrentAxis
    {
        get { return currentAxis; }
    }

    List<TextMeshProUGUI> menuTextObjects = new List<TextMeshProUGUI>();

    private bool bAllowAllAxisShortcut;
    private bool bOptionActive;

    public void SetCurrentAxis(AxisSelected newAxisSelected, GameObject referredObject)
    {
        currentAxis = newAxisSelected;

        SetAxisText();

        foreach (var item in menuObjects)
        {
            item.GetComponent<IToolOptionMenuItem>().Deselected();
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
        }

        referredObject.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        referredObject.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
    }

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

        axisText.enabled = false;
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

        SetAxisText();

        axisText.enabled = true;
    }

    public void AllowAllAxis(bool bAllowAllAxis)
    {
        bAllowAllAxisShortcut = bAllowAllAxis;
    }

    void SetAxisText()
    {
        switch (currentAxis)
        {
            case AxisSelected.XAxis:
                axisText.text = "Axis: X";
                break;
            case AxisSelected.YAxis:
                axisText.text = "Axis: Y";
                break;
            case AxisSelected.ZAxis:
                axisText.text = "Axis: Z";
                break;
            case AxisSelected.None:
                axisText.text = "Axis: None";
                break;
            default:
                break;
        }
    }

    public void ShortcutSwapAxisOption(bool bLeft)
    {
        switch (currentAxis)
        {
            case AxisSelected.XAxis:
                if (bLeft)
                {
                    if (bAllowAllAxisShortcut)
                    {
                        SetCurrentAxis(AxisSelected.None, menuObjects[0]);
                    }
                    else
                    {
                        SetCurrentAxis(AxisSelected.ZAxis, menuObjects[3]);
                    }
                }
                else
                {
                    SetCurrentAxis(AxisSelected.YAxis, menuObjects[2]);
                }
                break;
            case AxisSelected.YAxis:
                if (bLeft)
                {
                    SetCurrentAxis(AxisSelected.XAxis, menuObjects[1]);
                }
                else
                {
                    SetCurrentAxis(AxisSelected.ZAxis, menuObjects[3]);
                }
                break;
            case AxisSelected.ZAxis:
                if (bLeft)
                {
                    SetCurrentAxis(AxisSelected.YAxis, menuObjects[2]);
                }
                else
                {
                    if (bAllowAllAxisShortcut)
                    {
                        SetCurrentAxis(AxisSelected.None, menuObjects[0]);
                    }
                    else
                    {
                        SetCurrentAxis(AxisSelected.XAxis, menuObjects[1]);
                    }
                }
                break;
            case AxisSelected.None:
                if (bLeft)
                {
                    SetCurrentAxis(AxisSelected.ZAxis, menuObjects[3]);
                }
                else
                {
                    SetCurrentAxis(AxisSelected.XAxis, menuObjects[1]);
                }
                break;
        }
    }

    // Use this for initialization
    void Start ()
    {
        

        foreach (var item in menuObjects)
        {
            TextMeshProUGUI textItem = item.GetComponentInChildren<TextMeshProUGUI>();
            if(textItem)
            {
                menuTextObjects.Add(textItem);
            }
        }

        defaultOption.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        defaultOption.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);

    }	

    void Awake()
    {
        instance = this;
    }
}
