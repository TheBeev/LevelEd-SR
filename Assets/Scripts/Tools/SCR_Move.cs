using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_Move : MonoBehaviour, ITool {

    private enum ToolStates { Selecting, Moving };
    private ToolStates currentState = ToolStates.Selecting;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private ControllerInputs inputMethodLeft = ControllerInputs.LeftStick;
    [SerializeField] private ControllerInputs inputMethodRight = ControllerInputs.RightStick;
    [SerializeField] private string toolName;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    [Header("Shortcuts")]
    [SerializeField] private bool bAllowShortcuts;
    [SerializeField] private float rotationSpeedMultiplier = 5f;
    [SerializeField] private float scaleSpeedMultiplier = 0.01f;

    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private GameObject selectedObject;
    private Vector3 objectOffset;
    private Vector3 testOffset;
    private Vector3 startLocation;
    private Vector3 pointerLocation;
    private Vector3 offsetLocation;
    private Material objectStartMaterial;
    private SCR_GroupParent groupParentScript;

    private Vector2 inputMovementLeft;
    private Vector2 inputMovementRight;

    private SCR_ToolOptions toolOptions;
    private bool bSnap;
    private bool bFirstTime = true;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        if (toolOptions == null)
        {
            toolOptions = FindObjectOfType<SCR_ToolOptions>();
        }

        SCR_ToolOptions.instance.DeactivateOptions();
        SCR_GridSnappingOption.instance.ActivateOption();
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

        if (!bFirstTime)
        {
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
            SCR_InputDetection.instance.SubscribeToInput(inputMethodLeft, DoInputDetectedLeft);
            SCR_InputDetection.instance.SubscribeToInput(inputMethodRight, DoInputDetectedRight);
        }
        
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(inputMethodLeft, DoInputDetectedLeft);
        SCR_InputDetection.instance.UnsubscribeFromInput(inputMethodRight, DoInputDetectedRight);
        variablePointer.HighlightingActive = false;
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

    private void DoInputDetectedLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputMovementLeft = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            inputMovementLeft = Vector3.zero;
        }
    }

    private void DoInputDetectedRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputMovementRight = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            inputMovementRight = Vector3.zero;
        }
        
    }

    // Use this for initialization
    void Start ()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
            SCR_InputDetection.instance.SubscribeToInput(inputMethodLeft, DoInputDetectedLeft);
            SCR_InputDetection.instance.SubscribeToInput(inputMethodRight, DoInputDetectedRight);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (currentState)
        {
            case ToolStates.Selecting:
                SelectingObject();
                break;
            case ToolStates.Moving:
                MovingObject();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
    {
        if (variablePointer != null)
        {
            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                if (bActivationButtonPressed)
                {
                    bBusy = true;

                    variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);
                    variablePointer.RemoveHighlight();
                    variablePointer.HighlightingActive = false;

                    RaycastHit hit = variablePointer.PointerHit;

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.Off)
                    {
                        variablePointer.SnapToSurface();
                    }

                    //is it grouped or not
                    if (hit.transform.parent != null)
                    {
                        if (hit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                        {
                            selectedObject = hit.transform.root.gameObject;
                            objectOffset = variablePointer.PointerPosition - hit.point;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;
                            
                            groupParentScript = hit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                            groupParentScript.CheckMaterialCache();
                            groupParentScript.CurrentlySelected();
                        }
                        else if(hit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                        {
                            selectedObject = hit.transform.root.gameObject;
                            objectOffset = variablePointer.PointerPosition - hit.point;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;

                            selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        }
                    }
                    else
                    {
                        if (hit.transform.GetComponent<SCR_PrefabData>() != null)
                        {
                            selectedObject = hit.transform.gameObject;
                            objectOffset = variablePointer.PointerPosition - hit.point;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;

                            selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        }
                        else if(hit.transform.GetComponent<SCR_ObjectData>() != null)
                        {
                            selectedObject = hit.transform.gameObject;
                            objectOffset = variablePointer.PointerPosition - selectedObject.transform.position;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;
                            selectedObject.layer = 2;
                            objectStartMaterial = selectedObject.GetComponent<Renderer>().sharedMaterial;
                            selectedObject.GetComponent<Renderer>().sharedMaterial = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
                            selectedObject.GetComponent<Renderer>().sharedMaterial.mainTexture = objectStartMaterial.mainTexture;
                        }
                    }

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                    {
                        variablePointer.LockPointerLength(true);
                    }

                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                    //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                    bActivationButtonPressed = false;
                    currentState = ToolStates.Moving;
                }
            }
            else
            {
                variablePointer.HighlightingActive = true;
                bActivationButtonPressed = false;
            }
        }
    }

    void MovingObject()
    {

        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            pointerLocation = variablePointer.PointerPosition;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerLocation = pointerLocation + testOffset;
                offsetLocation = pointerLocation;
                selectedObject.transform.position = Snap(offsetLocation);
                
            }
            else
            {
                //selectedObject.transform.position = Snap(variablePointer.PointerPosition - objectOffset);
                pointerLocation = pointerLocation + testOffset;
                offsetLocation = pointerLocation;
                selectedObject.transform.position = Snap(offsetLocation);
            }
        }
        else
        {
            pointerLocation = variablePointer.PointerPosition;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerLocation = pointerLocation + testOffset;
                offsetLocation = pointerLocation;
                selectedObject.transform.position = offsetLocation;
            }
            else
            {
                //selectedObject.transform.position = variablePointer.PointerPosition - objectOffset;
                pointerLocation = pointerLocation + testOffset;
                offsetLocation = pointerLocation;
                selectedObject.transform.position = offsetLocation;
            }
            
        }

        //deals with shortcuts for rotating and scaling whilst placing the prefab
        if (bAllowShortcuts)
        {

            float rotationAmount = 0f;

            if (inputMovementRight.x > 0.8f)
            {
                rotationAmount = inputMovementRight.x;
            }
            else if (inputMovementRight.x < -0.8f)
            {
                rotationAmount = inputMovementRight.x;
            }

            if (selectedObject != null)
            {
                selectedObject.transform.Rotate(new Vector3(0f, rotationAmount * rotationSpeedMultiplier, 0f), Space.Self);
            }

            float scaleAmount = 0f;

            if (inputMovementLeft.x > 0.8f)
            {
                scaleAmount = inputMovementLeft.x;
            }
            else if (inputMovementLeft.x < -0.8f)
            {
                scaleAmount = inputMovementLeft.x;
            }

            scaleAmount *= scaleSpeedMultiplier;

            if (selectedObject != null)
            {
                selectedObject.transform.localScale = new Vector3(Mathf.Max(selectedObject.transform.localScale.x + scaleAmount, 0.05f), Mathf.Max(selectedObject.transform.localScale.y + scaleAmount, 0.05f), Mathf.Max(selectedObject.transform.localScale.z + scaleAmount, 0.05f));
            }

        }

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;
            if (selectedObject.GetComponent<SCR_GroupParent>() != null)
            {
                groupParentScript.Deselected();
            }
            else if(selectedObject.GetComponent<SCR_PrefabData>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
            }
            else
            {
                selectedObject.GetComponent<Renderer>().sharedMaterial = objectStartMaterial;
                selectedObject.layer = 8;
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            variablePointer.HighlightingActive = true;
            variablePointer.SetPointerColourDefault();

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;
            currentState = ToolStates.Selecting;
        }

    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
