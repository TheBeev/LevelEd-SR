using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_SceneMover : MonoBehaviour, ITool {

	private enum ToolStates { Selecting, Moving, Done };
	private ToolStates currentState = ToolStates.Selecting;

	[SerializeField] private ControllerInputs activationButtonRight = ControllerInputs.RightTrigger;
	[SerializeField] private ControllerInputs activationButtonLeft = ControllerInputs.LeftTrigger;
	[SerializeField] private ControllerInputs inputMethod = ControllerInputs.RightStick;
	[SerializeField] private string toolName;
	[SerializeField] private bool bAllowShortcuts;
	[SerializeField] private float rotationSpeedMultiplier = 5f;

	bool bBusy;
	public bool Busy
	{
		get { return bBusy; }
	}

	private SCR_SaveSystem saveScript;
	private GameObject sceneParentObject;
	private bool bActivationButtonPressed;
	private bool bActivationButtonPressedLeft;
	private IPointer variablePointer;
	private GameObject selectedObject;
	private Vector3 pointerLocation;

	private Vector2 inputMovement;
	private bool bFirstTime = true;

	private SCR_ToolOptions toolOptions;

	private void OnEnable()
	{
        if (!bFirstTime)
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

			SCR_InputDetection.instance.SubscribeToInput(activationButtonRight, DoActivationButtonPressed);
			SCR_InputDetection.instance.SubscribeToInput(activationButtonLeft, DoActivationButtonPressedLeft);
			SCR_InputDetection.instance.SubscribeToInput(inputMethod, DoInputDetected);
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

	void DoActivationButtonPressedLeft(InputAction.CallbackContext context)
	{
        if (context.started)
        {
			if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
			{
				if (!bActivationButtonPressedLeft && !SCR_ToolMenuRadial.instance.Busy)
				{
					bActivationButtonPressedLeft = true;
				}
			}
		}
		
	}

	private void DoInputDetected(InputAction.CallbackContext context)
	{
        if (context.performed)
        {
			inputMovement = context.ReadValue<Vector2>();
		}
		else if(context.canceled)
        {
			inputMovement = Vector3.zero;

		}
	}

	private void OnDisable()
	{
		bActivationButtonPressed = false;
		SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonRight, DoActivationButtonPressed);
		SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonLeft, DoActivationButtonPressedLeft);
		SCR_InputDetection.instance.UnsubscribeFromInput(inputMethod, DoInputDetected);
		variablePointer.HighlightingActive = false;
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
			OnEnable();
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
        if (sceneParentObject && bActivationButtonPressed)
        {
			bActivationButtonPressed = false;
			bBusy = true;
			currentState = ToolStates.Moving;
        }

        if (bActivationButtonPressedLeft)
        {
			saveScript.FinishSettingScene();
			bActivationButtonPressedLeft = false;
		}
	}

	void MovingObject()
    {
		if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
		{
			pointerLocation = variablePointer.PointerPosition;

			if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
			{
				sceneParentObject.transform.position = Snap(pointerLocation);

			}
			else
			{
				sceneParentObject.transform.position = Snap(pointerLocation);
			}
		}
		else
		{
			pointerLocation = variablePointer.PointerPosition;

			if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
			{
				sceneParentObject.transform.position = pointerLocation;
			}
			else
			{
				sceneParentObject.transform.position = pointerLocation;
			}

		}

		if (bAllowShortcuts)
		{
			float rotationAmount = 0f;

			if (inputMovement.x > 0.8f)
			{
				rotationAmount = inputMovement.x;
			}
			else if (inputMovement.x < -0.8f)
			{
				rotationAmount = inputMovement.x;
			}

			if (sceneParentObject != null)
			{
				sceneParentObject.transform.Rotate(new Vector3(0f, rotationAmount * rotationSpeedMultiplier, 0f), Space.Self);
			}
		}

		if (bActivationButtonPressed)
		{
			bActivationButtonPressed = false;
			SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
			//VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
			bBusy = false;
			
			currentState = ToolStates.Selecting;
		}
	}

	public void InitialiseMovement(SCR_SaveSystem newSaveScript, GameObject newParentObjectToMove)
    {
		sceneParentObject = newParentObjectToMove;
		saveScript = newSaveScript;
	}

	Vector3 Snap(Vector3 snapNearPoint)
	{
		return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
	}
}
