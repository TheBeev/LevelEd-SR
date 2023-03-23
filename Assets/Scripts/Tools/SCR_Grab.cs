using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_Grab : MonoBehaviour, ITool
{
    private enum ToolStates { Selecting, Grabbing };
    private ToolStates currentState = ToolStates.Selecting;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private GameObject variableObject;
    private GameObject selectedObject;
    private Material objectStartMaterial;
    private SCR_GroupParent groupParentScript;

    private SCR_ToolOptions toolOptions;
    private bool bSnap;

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
            case ToolStates.Grabbing:
                MovingObject();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
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

                if (hit.transform.parent)
                {
                    if (hit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                    {
                        selectedObject = hit.transform.root.gameObject;;

                        groupParentScript = hit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                        groupParentScript.CheckMaterialCache();
                        groupParentScript.CurrentlySelected();
                    }
                    else if(hit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                    {
                        selectedObject = hit.transform.root.gameObject;
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                    }
                }
                else
                {
                    if (hit.transform.GetComponent<SCR_PrefabData>() != null)
                    {
                        selectedObject = hit.transform.gameObject;
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                    }
                    else if(hit.transform.GetComponent<SCR_ObjectData>() != null)
                    {
                        selectedObject = hit.transform.gameObject;
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

                selectedObject.transform.parent = variablePointer.PointerEndGameObject.transform;

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                currentState = ToolStates.Grabbing;
            }
        }
        else
        {
            variablePointer.HighlightingActive = true;
            bActivationButtonPressed = false;
        }
    }

    void MovingObject()
    {

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;

            if (selectedObject.GetComponent<SCR_GroupParent>() != null)
            {
                selectedObject.GetComponent<SCR_GroupParent>().Deselected();
            }
            else if(selectedObject.GetComponent<SCR_PrefabData>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
            }
            else
            {
                selectedObject.layer = 8;
                selectedObject.GetComponent<Renderer>().sharedMaterial = objectStartMaterial;
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            selectedObject.transform.parent = null;

            variablePointer.HighlightingActive = true;

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;

            variablePointer.SetPointerColourDefault();

            currentState = ToolStates.Selecting;
        }

    }
}
