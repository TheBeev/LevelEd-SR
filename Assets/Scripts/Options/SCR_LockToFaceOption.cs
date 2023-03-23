using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_LockToFaceOption : MonoBehaviour, IToolOptionMenu, IOnOffToolOption
{
    public static SCR_LockToFaceOption instance;

    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private GameObject defaultOption;
    [SerializeField] private OptionActive lockToFaceActive = OptionActive.On;

    public OptionActive LockToFaceActive
    {
        get { return lockToFaceActive; }
    }

    private OptionActive previousState;

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

        //lockToFaceActive = OptionActive.Off;
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

        lockToFaceActive = previousState;

        labelObject.color = Color.white;
    }

    public void ToggleStatus(OptionActive optionActive, GameObject referredObject)
    {
        lockToFaceActive = optionActive;

        foreach (var item in menuObjects)
        {
            item.GetComponent<IToolOptionMenuItem>().Deselected();
            item.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
        }

        referredObject.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        referredObject.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
    }

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        foreach (var item in menuObjects)
        {
            TextMeshProUGUI textItem = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textItem)
            {
                menuTextObjects.Add(textItem);
            }
        }

        previousState = lockToFaceActive;

        defaultOption.GetComponent<IToolOptionMenuItem>().SelectedToggle();
        defaultOption.GetComponent<IToolOptionMenuItem>().CheckMaterials(bOptionActive);
    }
}
