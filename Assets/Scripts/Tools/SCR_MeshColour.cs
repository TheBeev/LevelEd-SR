using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_MeshColour : MonoBehaviour, ITool {

    private enum ToolStates { Colouring,  };
    private ToolStates currentState = ToolStates.Colouring;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;

    private Color colourToUse;
    public Color ColourToUse
    {
        get { return colourToUse; }
        set { colourToUse = value; }
    }

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private GameObject objectToColour;

    private SCR_GroupParent groupParentScript;
    private SCR_ToolOptions toolOptions;

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

    void DoActivationButtonReleased(InputAction.CallbackContext context)
    {

    }

    // Use this for initialization
    void Start()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

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
            case ToolStates.Colouring:
                ColouringObject();
                break;
            default:
                break;
        }
    }

    void ColouringObject()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);


                if (variablePointer.PointerHit.transform.root.GetComponent<SCR_GroupParent>() != null)
                {
                    groupParentScript = variablePointer.PointerHit.transform.root.GetComponent<SCR_GroupParent>();
                    variablePointer.RemoveHighlight();
                    groupParentScript.CheckMaterialCache();
                    objectToColour = variablePointer.PointerHit.transform.gameObject;
                    variablePointer.ObjectOriginalColour = colourToUse;
                    objectToColour.GetComponent<MeshRenderer>().material.color = colourToUse;
                    groupParentScript.UpdateCachedMaterials();

                }
                else if(variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                {

                    objectToColour = variablePointer.PointerHit.transform.gameObject;
                    variablePointer.ObjectOriginalColour = colourToUse;
                    objectToColour.GetComponent<Renderer>().material.color = colourToUse;

                }

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bActivationButtonPressed = false;
                bBusy = false;

                variablePointer.SetPointerColourDefault();

                currentState = ToolStates.Colouring;
            }
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }
}
