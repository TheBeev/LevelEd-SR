using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_AntennaPointer : MonoBehaviour {

    [SerializeField] private LineRenderer antennaLineRender;
    [SerializeField] private GameObject pointerEnd;
    [SerializeField] private ControllerInputs activationButton = ControllerInputs.LeftStickTouched;


    private bool bToggleDistance = false;
    public bool ToggleDistance
    {
        get { return bToggleDistance; }
        set { bToggleDistance = value; }
    }

    public Vector3 PointerPosition
    {
        get
        {
            return pointerEnd.transform.position;
        }
    }

    public bool ValidTargetPosition
    {
        get
        {
            return true;
        }
    }

    private bool bActive;
    public bool Active
    {
        get { return bActive; }
        set { bActive = value; }
    }

    private void OnEnable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started += DoActivationButtonPressed;
        bActive = true;
        //controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started -= DoActivationButtonPressed;
        //controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        antennaLineRender.enabled = !antennaLineRender.enabled;
        pointerEnd.SetActive(!pointerEnd.activeInHierarchy);
        bActive = !bActive;
    }

    void DoActivationButtonReleased(InputAction.CallbackContext context)
    {
        antennaLineRender.enabled = true;
        pointerEnd.SetActive(true);
        bActive = true;
    }

}
