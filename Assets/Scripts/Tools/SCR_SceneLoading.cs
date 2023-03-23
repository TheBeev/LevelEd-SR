using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class SCR_SceneLoading : MonoBehaviour, ITool
{
    private enum ToolStates { Idling, SelectingScene, LoadingScene };
    private ToolStates currentState = ToolStates.Idling;

    [SerializeField] private ControllerInputs launchButton = ControllerInputs.LeftButtonTwo;
    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private GameObject numberMenu;
    [SerializeField] private TextMeshProUGUI inputText;
    [SerializeField] private float menuDistanceFromPointer = 0.05f;
    [SerializeField] private SceneReplication sceneRelicationScript;
 
    private int loadButtonPresses;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private bool bLaunchButtonPressed;
    private bool bJustSelected;
    private IPointer variablePointer;
    private Transform headsetTransform;
    private GameObject selectedObject;
    private GameObject variableObject;

    private bool bParticipantNumberSelected;
    private bool bSceneLoaded;

    private GameObject currentHighlightedObject;
    private RaycastHit pointerHit;
    private PointerStates previousPointerState;
    private bool bNumberMenuOpen;
    private string currentNumberInput;
    private int currentNumber;
    private int menuItemLayer;
    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
            SCR_InputDetection.instance.SubscribeToInput(launchButton, DoLaunchButtonPressed);
        }

        if (variablePointer != null)
        {
            variablePointer.HighlightingActive = true;
        }
        else
        {
            variableObject = GameObject.FindGameObjectWithTag("LeftVariable");

            if (variableObject)
            {
                variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
            }

            menuItemLayer = 1 << 9;
            variablePointer.HighlightingActive = true;
        }

    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        variablePointer.HighlightingActive = false;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(launchButton, DoLaunchButtonPressed);
    }

    void DoLaunchButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
            {
                if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
                {
                    bLaunchButtonPressed = true;

                    if (bLaunchButtonPressed && bParticipantNumberSelected)
                    {
                        loadButtonPresses++;
                    }
                }
            }
        }
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
            {
                if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
                {
                    bActivationButtonPressed = true;
                }
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        variableObject = GameObject.FindGameObjectWithTag("LeftVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        menuItemLayer = 1 << 9;

        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
            SCR_InputDetection.instance.SubscribeToInput(launchButton, DoLaunchButtonPressed);
        }

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case ToolStates.Idling:
                WaitingForInput();
                break;
            case ToolStates.SelectingScene:
                SelectingSceneNumber();
                break;
            case ToolStates.LoadingScene:
                LoadingNewScene();
                break;
            default:
                break;
        }
    }

    void WaitingForInput()
    {
        if (!bParticipantNumberSelected && !bSceneLoaded && bLaunchButtonPressed)
        {
            bLaunchButtonPressed = false;
            OpenNumberMenu();
            currentState = ToolStates.SelectingScene;
        }
    }

    void OpenNumberMenu()
    {
        if (!bNumberMenuOpen)
        {

            bBusy = true;

            if (!headsetTransform)
            {
                headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;
            }

            previousPointerState = variablePointer.CurrentPointerState;
            variablePointer.FreezePointerState = true;
            variablePointer.SnapPointerState(PointerStates.Short);

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.LeftHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);

            numberMenu.transform.position = variablePointer.PointerEndGameObject.transform.position + (variablePointer.PointerEndGameObject.transform.forward * menuDistanceFromPointer);
            numberMenu.transform.rotation = variablePointer.PointerEndGameObject.transform.rotation;
            numberMenu.transform.LookAt(headsetTransform);
            numberMenu.SetActive(true);
            bNumberMenuOpen = true;
       
        }
    }

    void CloseNumberMenu()
    {
        variablePointer.SnapPointerState(previousPointerState);
        variablePointer.FreezePointerState = false;
        numberMenu.SetActive(false);
        bNumberMenuOpen = false;
    }

    void SelectingSceneNumber()
    {
        if (bNumberMenuOpen)
        {
            if (Physics.Raycast(variableObject.transform.position, variableObject.transform.forward, out pointerHit, 10.0f, menuItemLayer))
            {
                if (currentHighlightedObject != pointerHit.transform.gameObject)
                {
                    if (currentHighlightedObject)
                    {
                        currentHighlightedObject.GetComponent<IHighlightMenuItem>().Unhighlighted();
                    }

                    currentHighlightedObject = pointerHit.transform.gameObject;

                    currentHighlightedObject.GetComponent<IHighlightMenuItem>().Highlighted();

                    if (bJustSelected)
                    {
                        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.LeftHand);
                        bJustSelected = false;
                    }
                    else
                    {
                        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.LeftHand);
                    }

                    //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
                }
            }
            else
            {
                if (currentHighlightedObject)
                {
                    currentHighlightedObject.GetComponent<IHighlightMenuItem>().Unhighlighted();
                    currentHighlightedObject = null;
                }
            }
        }

        if (bActivationButtonPressed)
        {
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.LeftHand);

            if (currentHighlightedObject)
            {
                if (currentHighlightedObject.GetComponent<ICharacterMenuItem>() != null)
                {
                    if (currentHighlightedObject.GetComponent<ICharacterMenuItem>().CharacterOrSpecial == CharacterMenuState.Character)
                    {
                        currentNumberInput = currentNumberInput + currentHighlightedObject.GetComponent<ICharacterMenuItem>().CharacterValue;
                        inputText.text = currentNumberInput;
                        currentHighlightedObject.GetComponent<IHighlightMenuItem>().Unhighlighted();
                        bActivationButtonPressed = false;
                        currentHighlightedObject = null;
                        bJustSelected = true;
                    }
                    else if (currentHighlightedObject.GetComponent<ICharacterMenuItem>().CharacterOrSpecial == CharacterMenuState.Done)
                    {
                        int.TryParse(currentNumberInput, out currentNumber);
                        currentHighlightedObject.GetComponent<IHighlightMenuItem>().Unhighlighted();
                        CompleteNumberEntry();
                    }
                }
            }
            else
            {
                InvalidSelection();
            }
        }
        
    }

    void CompleteNumberEntry()
    {
        currentHighlightedObject = null;
        bActivationButtonPressed = false;
        bParticipantNumberSelected = true;
        currentNumberInput = null;
        inputText.text = "Enter a number...";
        CloseNumberMenu();

        variablePointer.SetPointerColourDefault();

        currentState = ToolStates.LoadingScene;
    }

    void LoadingNewScene()
    {
        //tell save script to pull data and pass along participant number
        if (!bSceneLoaded && bParticipantNumberSelected && bLaunchButtonPressed && loadButtonPresses >= 2)
        {
            bLaunchButtonPressed = false;
            sceneRelicationScript.SyncStart(currentNumber);
            bSceneLoaded = true;
            bBusy = false;
            currentState = ToolStates.Idling;
        }
    }

    //user activates button when nothing is highlighted
    void InvalidSelection()
    {
        currentHighlightedObject = null;
        bActivationButtonPressed = false;
        currentNumberInput = null;
        currentNumber = 0;
        inputText.text = "Enter a number...";
        CloseNumberMenu();

        variablePointer.SetPointerColourDefault();

        bBusy = false;

        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.LeftHand);
        
        //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
        currentState = ToolStates.Idling;
    }
}
