using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class SCR_EditScript : MonoBehaviour, ITool
{
    private enum ToolStates { Selecting, EditingNumber, EditingNodeOutput };
    private ToolStates currentState = ToolStates.Selecting;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;
    [SerializeField] private GameObject numberMenu;
    [SerializeField] private TextMeshProUGUI inputText;
    [SerializeField] private float menuDistanceFromPointer = 0.05f;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private bool bJustSelected;
    private IPointer variablePointer;
    private IOutputNode currentOutputNodeScript;
    private GameObject currentOutputNodeObject;
    private IInputNode currentInputNodeScript;
    private NodeType nodeTypeRequired;
    private Transform headsetTransform;
    private GameObject selectedObject;


    private IEditableNumber currentEditableNumberScript;
    private IEditableBool currentEditableBoolScript;
    private GameObject variableObject;
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
            SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

            SCR_ToolOptions.instance.DeactivateOptions();
            SCR_SurfaceSnappingOption.instance.ActivateOption();

            if (variablePointer != null)
            {
                variablePointer.HighlightingActive = true;
            }
            else
            {
                Start();
                variablePointer.HighlightingActive = true;
            }

            headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;

            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
        }
        
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        variablePointer.HighlightingActive = false;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
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
        variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        menuItemLayer = 1 << 9;

        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            OnEnable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case ToolStates.Selecting:
                SelectingObject();
                break;
            case ToolStates.EditingNumber:
                EditingNumber();
                break;
            case ToolStates.EditingNodeOutput:
                EditingNodeOutput();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
    {
        if (variablePointer != null)
        {
            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                if (variablePointer.Active && variablePointer.ValidRaycastTarget)
                {
                    //is it grouped or not
                    if (variablePointer.PointerHit.transform.parent)
                    {
                        if (variablePointer.PointerHit.transform.parent.gameObject.GetComponent<IOutputNode>() != null)
                        {
                            print("Output Node");
                            currentOutputNodeObject = variablePointer.PointerHit.transform.parent.gameObject;
                            currentOutputNodeScript = currentOutputNodeObject.GetComponent<IOutputNode>();
                            currentOutputNodeScript.Selected(true);
                            nodeTypeRequired = currentOutputNodeScript.NodeTypeRequired;
                            bActivationButtonPressed = false;

                            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                            currentState = ToolStates.EditingNodeOutput;
                            return;
                        }
                        else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<IEditableNumber>() != null)
                        {
                            print("Editable Number");
                            currentEditableNumberScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<IEditableNumber>();
                            bActivationButtonPressed = false;
                            SCR_ToolOptions.instance.DeactivateOptions();
                            OpenNumberMenu();
                            currentState = ToolStates.EditingNumber;
                            return;
                        }
                        else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<IEditableBool>() != null)
                        {
                            print("Editable Bool");
                            currentEditableBoolScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<IEditableBool>();
                            currentEditableBoolScript.SetNewBoolValue();
                        }

                    }

                }

                //SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);

                variablePointer.SetPointerColourDefault();

                bBusy = false;
                bActivationButtonPressed = false;
                print("Not compatible");
            }
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

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
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

    void EditingNumber()
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
                        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                        bJustSelected = false;
                    }
                    else
                    {
                        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.RightHand);
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
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);

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
                        currentEditableNumberScript.SetNewNumberValue(currentNumber);
                        currentHighlightedObject.GetComponent<IHighlightMenuItem>().Unhighlighted();
                        CompleteNumberEntry();
                    }
                }
            }
            else
            {
                CompleteNumberEntry();
            }
        }
        
    }

    void CompleteNumberEntry()
    {
        currentHighlightedObject = null;
        bActivationButtonPressed = false;
        currentEditableNumberScript = null;
        currentNumberInput = null;
        currentNumber = 0;
        inputText.text = "Enter a number...";
        CloseNumberMenu();
        SCR_SurfaceSnappingOption.instance.ActivateOption();

        variablePointer.SetPointerColourDefault();

        bBusy = false;
        currentState = ToolStates.Selecting;
    }

    void EditingNodeOutput()
    {
        if (variablePointer != null)
        {
            if (bActivationButtonPressed)
            {
                if (variablePointer.Active && variablePointer.ValidRaycastTarget)
                {
                    if (variablePointer.PointerHit.transform.parent)
                    {
                        if (variablePointer.PointerHit.transform.parent.gameObject.GetComponent<IInputNode>() != null)
                        {
                            currentInputNodeScript = variablePointer.PointerHit.transform.parent.gameObject.GetComponent<IInputNode>();
                            
                            if(nodeTypeRequired == currentInputNodeScript.ThisNodeType)
                            {
                                currentOutputNodeScript.RemoveTarget();
                                print("Input Node");

                                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                                currentOutputNodeScript.UpdateInputNodeToConnectTo(currentInputNodeScript.InputNodeID, variablePointer.PointerHit.transform.parent.gameObject, true);
                                currentInputNodeScript.OutputNodeConnectedObject = currentOutputNodeObject;
                                currentOutputNodeScript.Selected(false);
                                bActivationButtonPressed = false;
                                currentInputNodeScript = null;
                                bBusy = false;
                                variablePointer.SetPointerColourDefault();
                                currentState = ToolStates.Selecting;
                                return;
                            }
                        }
                        else if(variablePointer.PointerHit.transform.parent.gameObject.GetComponent<IOutputNode>() == currentOutputNodeScript)
                        {
                            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                            currentOutputNodeScript.RemoveTarget();
                            currentOutputNodeScript.Selected(false);
                            bActivationButtonPressed = false;
                            currentInputNodeScript = null;
                            bBusy = false;
                            variablePointer.SetPointerColourDefault();
                            currentState = ToolStates.Selecting;
                            return;
                        }
                    }
                }

                //user hasn't selected a valid input tag so cancel the task
                InvalidSelection();
            }
        }
    }

    void InvalidSelection()
    {
        bBusy = false;
        bActivationButtonPressed = false;
        currentOutputNodeScript.Selected(false);
        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
        //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
        currentInputNodeScript = null;
        currentOutputNodeScript = null;
        currentState = ToolStates.Selecting;
    }
}
