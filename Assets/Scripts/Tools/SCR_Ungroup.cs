using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_Ungroup : MonoBehaviour, ITool {

    private enum ToolStates { Ungrouping, Idling };
    private ToolStates currentState = ToolStates.Ungrouping;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;
    [SerializeField] private Color defaultColour;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;

    private bool bFirstTime = true;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);
        SCR_ToolOptions.instance.DeactivateOptions();

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
        }
        
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        variablePointer.HighlightingActive = false;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
    }

    private void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
            {
                bActivationButtonPressed = true;
            }
        }
        else if (context.canceled)
        {
            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
            {
                if (bActivationButtonPressed)
                {
                    bActivationButtonPressed = false;
                }
            }
        }
    }

    /*
    private void DoActivationButtonDepressed(InputAction.CallbackContext context)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            if (bActivationButtonPressed)
            {
                bActivationButtonPressed = false;
            }
        }
    }
    */

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
        }

    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (currentState)
        {
            case ToolStates.Ungrouping:
                UngroupObjects();
                break;
            case ToolStates.Idling:
                //ToolIdling();
                break;
            default:
                break;
        }
    }

    private void UngroupObjects()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget /*&& variablePointer.PointerHit.transform.parent != null*/)
        {
            if (variablePointer.PointerHit.transform.parent)
            {
                if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                {
                    if (bActivationButtonPressed && !bBusy)
                    {
                        bBusy = true;

                        variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                        GameObject oldGroupParentObject = variablePointer.PointerHit.transform.parent.gameObject;
                        SCR_GroupParent oldGroupParentScript = oldGroupParentObject.GetComponent<SCR_GroupParent>();

                        if (oldGroupParentScript)
                        {
                            oldGroupParentScript.CheckMaterialCache();
                            oldGroupParentScript.CurrentlySelected();

                            StartCoroutine(DelayedUngroup(oldGroupParentObject));
                        }
                    }
                }
            }
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }

    IEnumerator DelayedUngroup(GameObject oldParentObjectToDelete)
    {
        variablePointer.HighlightingActive = false;
        variablePointer.RemoveHighlight();

        SCR_GroupParent groupParentScript = oldParentObjectToDelete.GetComponent<SCR_GroupParent>();

        yield return new WaitForSeconds(0.1f);

        groupParentScript.Deselected();

        groupParentScript.groupedObjectList.Clear();

        oldParentObjectToDelete.transform.DetachChildren();

        SCR_SaveSystem.instance.RemoveParent(oldParentObjectToDelete);
        Destroy(oldParentObjectToDelete);

        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
        //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
        bBusy = false;

        variablePointer.HighlightingActive = true;
        variablePointer.SetPointerColourDefault();

    }

}
