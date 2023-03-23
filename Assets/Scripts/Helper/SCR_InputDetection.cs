using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;


public enum ControllerInputs
{
    RightStick,
    RightStickClick,
    RightStickTouched,
    RightButtonOne,
    RightButtonOneTouched,
    RightButtonTwo,
    RightButtonTwoTouched,
    RightTrigger,
    RightGrip,
    LeftStick,
    LeftStickClick,
    LeftStickTouched,
    LeftButtonOne,
    LeftButtonOneTouched,
    LeftButtonTwo,
    LeftButtonTwoTouched,
    LeftTrigger,
    LeftGrip,
}

public class SCR_InputDetection : MonoBehaviour
{
    public static SCR_InputDetection instance;
    public PlayerInput playerInput;

    public EditorActions inputs;

    private void Awake()
    {
        instance = this;
        playerInput = GetComponent<PlayerInput>();
        inputs = new EditorActions();
        inputs.Enable();
    }

    public void SubscribeToInput(ControllerInputs newInput, Action<InputAction.CallbackContext> callback)
    {
        switch (newInput)
        {
            case ControllerInputs.RightStick:
                inputs.EditorInput.RightStick.performed += callback;
                inputs.EditorInput.RightStick.canceled += callback;
                break;
            case ControllerInputs.RightStickClick:
                inputs.EditorInput.RightStickClick.started += callback;
                inputs.EditorInput.RightStickClick.canceled += callback;
                break;
            case ControllerInputs.RightStickTouched:
                inputs.EditorInput.RightStickTouched.started += callback;
                inputs.EditorInput.RightStickTouched.canceled += callback;
                break;
            case ControllerInputs.RightButtonOne:
                inputs.EditorInput.RightButtonOne.started += callback;
                inputs.EditorInput.RightButtonOne.canceled += callback;
                break;
            case ControllerInputs.RightButtonOneTouched:
                inputs.EditorInput.RightButtonOneTouched.started += callback;
                inputs.EditorInput.RightButtonOneTouched.canceled += callback;
                break;
            case ControllerInputs.RightButtonTwo:
                inputs.EditorInput.RightButtonTwo.started += callback;
                inputs.EditorInput.RightButtonTwo.canceled += callback;
                break;
            case ControllerInputs.RightButtonTwoTouched:
                inputs.EditorInput.RightButtonTwoTouched.started += callback;
                inputs.EditorInput.RightButtonTwoTouched.canceled += callback;
                break;
            case ControllerInputs.RightTrigger:
                inputs.EditorInput.RightTrigger.started += callback;
                inputs.EditorInput.RightTrigger.canceled += callback;
                break;
            case ControllerInputs.RightGrip:
                inputs.EditorInput.RightGrip.started += callback;
                inputs.EditorInput.RightGrip.canceled += callback;
                break;
            case ControllerInputs.LeftStick:
                inputs.EditorInput.LeftStick.performed += callback;
                inputs.EditorInput.LeftStick.canceled += callback;
                break;
            case ControllerInputs.LeftStickClick:
                inputs.EditorInput.LeftStickClick.started += callback;
                inputs.EditorInput.LeftStickClick.canceled += callback;
                break;
            case ControllerInputs.LeftStickTouched:
                inputs.EditorInput.LeftStickTouched.started += callback;
                inputs.EditorInput.LeftStickTouched.canceled += callback;
                break;
            case ControllerInputs.LeftButtonOne:
                inputs.EditorInput.LeftButtonOne.started += callback;
                inputs.EditorInput.LeftButtonOne.canceled += callback;
                break;
            case ControllerInputs.LeftButtonOneTouched:
                inputs.EditorInput.LeftButtonOneTouched.started += callback;
                inputs.EditorInput.LeftButtonOneTouched.canceled += callback;
                break;
            case ControllerInputs.LeftButtonTwo:
                inputs.EditorInput.LeftButtonTwo.started += callback;
                inputs.EditorInput.LeftButtonTwo.canceled += callback;
                break;
            case ControllerInputs.LeftButtonTwoTouched:
                inputs.EditorInput.LeftButtonTwoTouched.started += callback;
                inputs.EditorInput.LeftButtonTwoTouched.canceled += callback;
                break;
            case ControllerInputs.LeftTrigger:
                inputs.EditorInput.LeftTrigger.started += callback;
                inputs.EditorInput.LeftTrigger.canceled += callback;
                break;
            case ControllerInputs.LeftGrip:
                inputs.EditorInput.LeftGrip.started += callback;
                inputs.EditorInput.LeftGrip.canceled += callback;
                break;
            default:
                break;
        }
    }

    public void UnsubscribeFromInput(ControllerInputs newInput, Action<InputAction.CallbackContext> callback)
    {
        switch (newInput)
        {
            case ControllerInputs.RightStick:
                inputs.EditorInput.RightStick.performed -= callback;
                inputs.EditorInput.RightStick.canceled -= callback;
                break;
            case ControllerInputs.RightStickClick:
                inputs.EditorInput.RightStickClick.started -= callback;
                inputs.EditorInput.RightStickClick.canceled -= callback;
                break;
            case ControllerInputs.RightStickTouched:
                inputs.EditorInput.RightStickTouched.started -= callback;
                inputs.EditorInput.RightStickTouched.canceled -= callback;
                break;
            case ControllerInputs.RightButtonOne:
                inputs.EditorInput.RightButtonOne.started -= callback;
                inputs.EditorInput.RightButtonOne.canceled -= callback;
                break;
            case ControllerInputs.RightButtonOneTouched:
                inputs.EditorInput.RightButtonOneTouched.started -= callback;
                inputs.EditorInput.RightButtonOneTouched.canceled -= callback;
                break;
            case ControllerInputs.RightButtonTwo:
                inputs.EditorInput.RightButtonTwo.started -= callback;
                inputs.EditorInput.RightButtonTwo.canceled -= callback;
                break;
            case ControllerInputs.RightButtonTwoTouched:
                inputs.EditorInput.RightButtonTwoTouched.started -= callback;
                inputs.EditorInput.RightButtonTwoTouched.canceled -= callback;
                break;
            case ControllerInputs.RightTrigger:
                inputs.EditorInput.RightTrigger.started -= callback;
                inputs.EditorInput.RightTrigger.canceled -= callback;
                break;
            case ControllerInputs.RightGrip:
                inputs.EditorInput.RightGrip.started -= callback;
                inputs.EditorInput.RightGrip.canceled -= callback;
                break;
            case ControllerInputs.LeftStick:
                inputs.EditorInput.LeftStick.performed -= callback;
                break;
            case ControllerInputs.LeftStickClick:
                inputs.EditorInput.LeftStickClick.started -= callback;
                inputs.EditorInput.LeftStickClick.canceled -= callback;
                break;
            case ControllerInputs.LeftStickTouched:
                inputs.EditorInput.LeftStickTouched.started -= callback;
                inputs.EditorInput.LeftStickTouched.canceled -= callback;
                break;
            case ControllerInputs.LeftButtonOne:
                inputs.EditorInput.LeftButtonOne.started -= callback;
                inputs.EditorInput.LeftButtonOne.canceled -= callback;
                break;
            case ControllerInputs.LeftButtonOneTouched:
                inputs.EditorInput.LeftButtonOneTouched.started -= callback;
                inputs.EditorInput.LeftButtonOneTouched.canceled -= callback;
                break;
            case ControllerInputs.LeftButtonTwo:
                inputs.EditorInput.LeftButtonTwo.started -= callback;
                inputs.EditorInput.LeftButtonTwo.canceled -= callback;
                break;
            case ControllerInputs.LeftButtonTwoTouched:
                inputs.EditorInput.LeftButtonTwoTouched.started -= callback;
                inputs.EditorInput.LeftButtonTwoTouched.canceled -= callback;
                break;
            case ControllerInputs.LeftTrigger:
                inputs.EditorInput.LeftTrigger.started -= callback;
                inputs.EditorInput.LeftTrigger.canceled -= callback;
                break;
            case ControllerInputs.LeftGrip:
                inputs.EditorInput.LeftGrip.started -= callback;
                inputs.EditorInput.LeftGrip.canceled -= callback;
                break;
            default:
                break;
        }
    }
}
