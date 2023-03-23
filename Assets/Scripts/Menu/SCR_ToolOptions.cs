using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public enum OptionActive { On, Off };
public enum GroupModeSelected { Grouping, Ungrouping };


public class SCR_ToolOptions : MonoBehaviour
{

    private GroupModeSelected currentGroupMode = GroupModeSelected.Grouping;
    public GroupModeSelected CurrentGroupMode
    {
        get { return currentGroupMode; }
    }

    public static SCR_ToolOptions instance;
    public Color defaultObjectColour;
    public Color defaultOutlineColour;
    public Color highlightedObjectColour;
    public Color highlightedOutlineColour;
    public Color selectedObjectColour;
    public Color selectedOutlineColour;

    [SerializeField] private ControllerInputs optionsMenuButton = ControllerInputs.LeftButtonOne;
    [SerializeField] private ControllerInputs selectionButton = ControllerInputs.LeftTrigger;
    [SerializeField] private string controllerTag = "LeftVariable";
    [SerializeField] private GameObject optionsMenuUI;
    [SerializeField] private GameObject[] menuOptionsGO;
    [SerializeField] private Transform headsetCentre;
    [SerializeField] private LayerMask menuItemLayer;
    [SerializeField] private Text groupModeText;
    
    private bool bSnappingActive;
    private bool bAxisOptionsActive;
    private bool bGroupModeActive;

    private bool bFirstTime = true;

    private IPointer variablePointer;
    private GameObject variableObject;
    private PointerStates previousPointerState;
    private SCR_Group groupScript;
    private RaycastHit pointerHit;
    private GameObject currentHighlightedObject;

    private Color previousObjectColour;
    private Color previousObjectOutlineColour;

    private bool bOptionsMenuOpen;
    public bool OptionsMenuOpen
    {
        get { return bOptionsMenuOpen; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        variableObject = GameObject.FindGameObjectWithTag(controllerTag);

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            OnEnable();
        }
        
    }

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            headsetCentre = SCR_HeadsetReferences.instance.centerEye.transform;
            SCR_InputDetection.instance.SubscribeToInput(optionsMenuButton, DoActivationButtonPressed);
            SCR_InputDetection.instance.SubscribeToInput(selectionButton, DoActivationSelectionButtonPressed);
        } 
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.UnsubscribeFromInput(optionsMenuButton, DoActivationButtonPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(selectionButton, DoActivationSelectionButtonPressed);

        if (bOptionsMenuOpen)
        {
            MenuClose();
        }
    }

    void DoActivationSelectionButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (bOptionsMenuOpen)
            {
                MenuClose();
            }
        }
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (bOptionsMenuOpen)
            {
                MenuClose();
            }
            else
            {
                MenuOpen();
            }
        }
    }

    void MenuOpen()
    {
        previousPointerState = variablePointer.CurrentPointerState;
        variablePointer.FreezePointerState = true;
        variablePointer.SnapPointerState(PointerStates.Short);
        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.LeftHand);
        //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
        optionsMenuUI.transform.position = variablePointer.PointerEndGameObject.transform.position + (variablePointer.PointerEndGameObject.transform.forward * 0.04f);
        optionsMenuUI.transform.rotation = variablePointer.PointerEndGameObject.transform.rotation;
        optionsMenuUI.transform.LookAt(headsetCentre);
        optionsMenuUI.SetActive(true);
        bOptionsMenuOpen = true;
    }

    public void TempOptionsMenuClose()
    {
        if (bOptionsMenuOpen)
        {
            if (optionsMenuUI)
            {
                optionsMenuUI.SetActive(false);
            }

            if (variableObject)
            {
                variablePointer.SnapPointerState(previousPointerState);
                variablePointer.FreezePointerState = false;
            }

            bOptionsMenuOpen = false;
        }
        
    }

    void MenuClose()
    {
        bOptionsMenuOpen = false;

        if (optionsMenuUI)
        {
            optionsMenuUI.SetActive(false);
        }

        if (variableObject)
        {
            variablePointer.SnapPointerState(previousPointerState);
            variablePointer.FreezePointerState = false;
        }      

        if (currentHighlightedObject)
        {
            currentHighlightedObject.GetComponent<IToolOptionMenuItem>().Selected();
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.LeftHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 2.0f);
        }

        currentHighlightedObject = null;
        previousObjectColour = defaultObjectColour;
        previousObjectOutlineColour = defaultOutlineColour;
    }

    public void DeactivateOptions()
    {
        foreach (var item in menuOptionsGO)
        {
            IToolOptionMenu optionMenuItem = item.GetComponent<IToolOptionMenu>();
            if (optionMenuItem != null)
            {
                optionMenuItem.DeactivateOption();
            }
        }
    }

    void Update()
    {
        if (bOptionsMenuOpen)
        {
            if (Physics.Raycast(variablePointer.PointerLineRendererStartTransform.position, variablePointer.PointerLineRendererStartTransform.forward, out pointerHit, 10.0f, menuItemLayer))
            {
                if (currentHighlightedObject != pointerHit.collider.gameObject)
                {
                    if (currentHighlightedObject)
                    {
                        currentHighlightedObject.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
                        currentHighlightedObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    }

                    currentHighlightedObject = pointerHit.collider.gameObject;
                    currentHighlightedObject.GetComponent<Renderer>().material = SCR_ToolMenuRadial.instance.highlightedMenuMaterial;
                    
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.LeftHand);
                    //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
                    currentHighlightedObject.transform.localScale = new Vector3(1.6f, 1.6f, 1.1f);

                }
            }
            else
            {
                if (currentHighlightedObject)
                {
                    currentHighlightedObject.GetComponent<IToolOptionMenuItem>().CheckMaterials(true);
                    currentHighlightedObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                    currentHighlightedObject = null;
                }

            }
        }
    }

    private void DoToggleGroupModeButtonPressed()
    {
        if (bGroupModeActive && !groupScript.Busy)
        {
            switch (currentGroupMode)
            {
                case GroupModeSelected.Grouping:
                    groupModeText.text = "Mode: Ungrouping";
                    currentGroupMode = GroupModeSelected.Ungrouping;
                    break;
                case GroupModeSelected.Ungrouping:
                    groupModeText.text = "Mode: Grouping";
                    currentGroupMode = GroupModeSelected.Grouping;
                    break;
                default:
                    break;
            }
        }
    }

    public void GroupModeActive(bool bDisplay, SCR_Group newGroupScript)
    {
        groupScript = newGroupScript;

        if (bDisplay)
        {
            groupModeText.enabled = true;
            bGroupModeActive = true;
        }
        else
        {
            groupModeText.enabled = false;
            bGroupModeActive = false;
        }
    }

}

