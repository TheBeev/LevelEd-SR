using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SCR_ToolMenuRadial : MonoBehaviour
{
    public static SCR_ToolMenuRadial instance;
    public Color defaultObjectColour;
    public Color defaultOutlineColour;
    public Color highlightedObjectColour;
    public Color highlightedOutlineColour;
    public Color selectedObjectColour;
    public Color selectedOutlineColour;
    public Color toolBusyPointerColour;
    public Color highlightedGameObjectColour;
    public Color infinitePointerColour;
    public Material selectedObjectMaterial;
    public Material highlightedObjectMaterial;
    public Material selectedMenuMaterial;
    public Material highlightedMenuMaterial;
    public Material defaultMenuMaterial;
    public Material selectedMenuMaterialFaded;
    public Material defaultMenuMaterialFaded;
    public Material defaultCreationMaterial;

    [SerializeField] private ControllerInputs activationButtonTool = ControllerInputs.RightButtonTwo;
    [SerializeField] private ControllerInputs activationButtonEdit = ControllerInputs.RightButtonOne;
    [SerializeField] private ControllerInputs activationButtonSRSetup = ControllerInputs.LeftButtonOne;
    [SerializeField] private ControllerInputs selectionButton = ControllerInputs.RightTrigger;
    [SerializeField] private GameObject toolMenuUI;
    [SerializeField] private GameObject editMenuUI;
    [SerializeField] private float menuDistanceFromPointer = 0.05f;
    [SerializeField] private Transform headsetCentre;
    [SerializeField] private TextMeshProUGUI toolText;
    [SerializeField] private GameObject defaultTool;
    [SerializeField] private GameObject defaultMenuItem;

    private bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private GameObject currentTool;

    private RectTransform toolMenuRectTransform;
    private Vector3 overlapBoxExtents;
    private Vector3 centre;
    //private int geometryLayer;

    private PointerStates previousPointerState;
    private int menuItemLayer;
    private bool bToolMenuOpen;
    private bool bEditMenuOpen;
    private bool bSelectionTriggerPressed;
    private RaycastHit pointerHit;
    private IToolMenuItem currentMenuItem;
    private GameObject currentMenuItemGO;
    private GameObject currentToolMenuItem;
    private GameObject currentHighlightedObject;
    private GameObject currentActivePopoutObject;
    private bool bFirstTime = true;
    private bool bSRSetupMenuActivated = false;

    private Color previousObjectColour;
    private Color previousObjectOutlineColour;
    private GameObject previousSelectedObject;

    private IPointer variablePointer;
    private GameObject variableObject;

    private void Awake()
    {
        if (instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
        //previousSelectedObject.GetComponent<SCR_Outline>().OutlineColor = selectedOutlineColour;
    }

    // Use this for initialization
    void Start()
    {
        //geometryLayer = 1 << 8;
        menuItemLayer = 1 << 9;

        variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        //fixes an issue where the edit menu wouldn't be usable the first time it was opened.
        toolMenuUI.SetActive(true);
        editMenuUI.SetActive(true);

        defaultMenuItem.GetComponent<IToolMenuItem>().OnSelected();

        previousSelectedObject = defaultMenuItem;
        previousSelectedObject.GetComponent<Renderer>().material.color = selectedObjectColour;

        toolMenuUI.SetActive(false);
        editMenuUI.SetActive(false);

        headsetCentre = SCR_HeadsetReferences.instance.centerEye.transform;

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
            SCR_InputDetection.instance.SubscribeToInput(activationButtonTool, DoActivationButtonToolPressed);
            SCR_InputDetection.instance.SubscribeToInput(activationButtonEdit, DoActivationButtonEditPressed);
            SCR_InputDetection.instance.SubscribeToInput(selectionButton, DoActivationButtonSelectionPressed);
            SCR_InputDetection.instance.SubscribeToInput(activationButtonSRSetup, DoSRSetupMenuButtonPressed);
        }  
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonTool, DoActivationButtonToolPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonEdit, DoActivationButtonEditPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(selectionButton, DoActivationButtonSelectionPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonSRSetup, DoSRSetupMenuButtonPressed);


        if (bEditMenuOpen)
        {
            MenuClose(false);
        }

        if (bToolMenuOpen)
        {
            MenuClose(true);
        }
    }

    void DoSRSetupMenuButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            bSRSetupMenuActivated = true;
        }
    }

    void DoActivationButtonSelectionPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            MenuButtonPressed();
        }
        else if (context.canceled)
        {
            bBusy = false;
        }
    }

    void MenuButtonPressed()
    {
        bSelectionTriggerPressed = true;

        if (currentHighlightedObject)
        {

            if (previousSelectedObject)
            {
                if (previousSelectedObject != currentHighlightedObject)
                {
                    if (currentHighlightedObject.GetComponent<IMenuModeItem>() == null)
                    {

                        previousSelectedObject.SetActive(true);

                        previousSelectedObject.GetComponent<IToolMenuItem>().Deselected();

                        //previousSelectedObject.SetActive(false);
                        previousSelectedObject = currentHighlightedObject;
                    }
                }
            }

            currentHighlightedObject.GetComponent<IToolMenuItem>().OnSelected();
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 2.0f);

        }

        previousObjectColour = defaultObjectColour;
        previousObjectOutlineColour = defaultOutlineColour;


        if (bToolMenuOpen)
        {
            if (currentHighlightedObject)
            {
                if (currentHighlightedObject.GetComponent<IToolMenuItem>().CloseMenuOnSelection)
                {
                    MenuClose(true);
                }
            }
            else
            {

            }
        }
        else if (bEditMenuOpen)
        {
            MenuClose(false);
        }

        currentHighlightedObject = null;
        bSRSetupMenuActivated = false;

    }

    void DoActivationButtonToolPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (bToolMenuOpen)
            {
                MenuClose(true);
                bBusy = false;
            }
            else if (bEditMenuOpen)
            {
                MenuClose(false);
                MenuOpen(true);
            }
            else
            {
                previousPointerState = variablePointer.CurrentPointerState;
                MenuOpen(true);
            }
        }  
    }

    void DoActivationButtonEditPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (bEditMenuOpen)
            {
                MenuClose(false);
                bBusy = false;
            }
            else if (bToolMenuOpen)
            {
                MenuClose(true);
                MenuOpen(false);
            }
            else
            {
                previousPointerState = variablePointer.CurrentPointerState;
                MenuOpen(false);
            }
        }  
    }
    
    /*
    void DoActivationButtonSelectionDepressed(InputAction.CallbackContext context)
    {
        bBusy = false;
    }
    */

    void MenuOpen(bool bToolMenu)
    {
        if (!bToolMenuOpen && !bEditMenuOpen)
        {
            bBusy = true;
            

            if (!currentTool.GetComponent<ITool>().Busy)
            {
                
                variablePointer.FreezePointerState = true;
                //variablePointer.LockPointerLength(true);
                variablePointer.SnapPointerState(PointerStates.Short);

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);

                currentTool.SetActive(false);

                if (bToolMenu)
                {
                    toolMenuUI.transform.position = variablePointer.PointerEndGameObject.transform.position + (variablePointer.PointerEndGameObject.transform.forward * menuDistanceFromPointer);
                    toolMenuUI.transform.rotation = variablePointer.PointerEndGameObject.transform.rotation;
                    toolMenuUI.transform.LookAt(headsetCentre);
                    toolMenuUI.SetActive(true);
                    bToolMenuOpen = true;
                }
                else
                {
                    editMenuUI.transform.position = variablePointer.PointerEndGameObject.transform.position + (variablePointer.PointerEndGameObject.transform.forward * menuDistanceFromPointer);
                    editMenuUI.transform.rotation = variablePointer.PointerEndGameObject.transform.rotation;
                    editMenuUI.transform.LookAt(headsetCentre);
                    editMenuUI.SetActive(true);
                    bEditMenuOpen = true;
                }
            }
        }
    }

    public void TempCloseMenu()
    {
        if (bToolMenuOpen || bEditMenuOpen)
        {
            if (toolMenuUI)
            {
                toolMenuUI.SetActive(false);
            }

            bToolMenuOpen = false;

            if (editMenuUI)
            {
                editMenuUI.SetActive(false);
            }

            bEditMenuOpen = false;

            bBusy = false;

            if (variableObject)
            {
                variablePointer.LockPointerLength(false);
                variablePointer.FreezePointerState = false;
                variablePointer.SnapPointerState(previousPointerState);
            }

            currentTool.SetActive(true);
        }
        
    }

    void MenuClose(bool bToolMenu)
    {
        if (bToolMenu)
        {
            if (toolMenuUI)
            {
                toolMenuUI.SetActive(false);
            }
            
            bToolMenuOpen = false;
        }
        else
        {
            if (editMenuUI)
            {
                editMenuUI.SetActive(false);
            }
            
            bEditMenuOpen = false;
        }

        bBusy = false;

        if (variableObject)
        {
            variablePointer.LockPointerLength(false);
            variablePointer.FreezePointerState = false;
            variablePointer.SnapPointerState(previousPointerState);
        }

        currentTool.SetActive(true);

    }

    public void ToolChanged(GameObject newTool, string toolName)
    {
        if (currentTool && currentTool != newTool)
        {
            currentTool.SetActive(false);
        }

        currentTool = newTool;
        toolText.text = toolName;
    }
    public bool ToolBusy()
    {
        return currentTool.GetComponent<ITool>().Busy;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            MenuButtonPressed();
        }
        */

        if (bSRSetupMenuActivated)
        {
            MenuButtonPressed();
        }

        if (bToolMenuOpen || bEditMenuOpen)
        {
            if (Physics.Raycast(variablePointer.PointerLineRendererStartTransform.position, variablePointer.PointerLineRendererStartTransform.forward, out pointerHit, 10.0f, menuItemLayer))
            {
                if (currentHighlightedObject != pointerHit.transform.gameObject)
                {
                    if (currentHighlightedObject)
                    {
                        currentHighlightedObject.GetComponent<IToolMenuItem>().Unhighlighted();
                    }

                    currentHighlightedObject = pointerHit.transform.gameObject;

                    //deals with hiding and revealing popout options
                    if (currentHighlightedObject.GetComponent<IMenuPopout>() != null)
                    {
                        currentHighlightedObject.GetComponent<IMenuPopout>().ActivatePopout();
                        currentActivePopoutObject = currentHighlightedObject;
                    }
                    else
                    {
                        if (currentActivePopoutObject != null)
                        {
                            if (currentHighlightedObject.GetComponent<IMenuPopoutItem>() == null)
                            {
                                currentActivePopoutObject.GetComponent<IMenuPopout>().DeactivatePopout();
                            }  
                        }
                    }

                    currentHighlightedObject.GetComponent<IToolMenuItem>().Highlighted();
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.RightHand);
                    //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
                }
            }
            else
            {
                if (currentHighlightedObject)
                {
                    currentHighlightedObject.GetComponent<IToolMenuItem>().Unhighlighted();
                    currentHighlightedObject = null;
                }
            }
        }
    }
}
