using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_ConvertOutline : MonoBehaviour, ITool {

	private enum ToolStates { Converting };
	private ToolStates currentState = ToolStates.Converting;

	[SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
	[SerializeField] private ControllerInputs unhideShortcutButton = ControllerInputs.LeftStick;
	[SerializeField] private string toolName;
    [SerializeField] private Material convertedObjectMaterial;
    [SerializeField] private bool bAllowUnhideShortcut;

	bool bBusy;
	public bool Busy
	{
		get { return bBusy; }
	}

	private bool bActivationButtonPressed;
	private IPointer variablePointer;
	private GameObject objectToCopy;
	private GameObject selectedObject;

	private float timer;
	private float timeToUnhide = 5f;
	private Vector2 inputMovement;

	private List<GameObject> convertedOutlineMeshes = new List<GameObject>();
	private List<GameObject> hiddenOutlineMeshes = new List<GameObject>();

	private bool bFirstTime = true;

	private void OnEnable()
	{
        if (!bFirstTime)
        {
			SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

			SCR_ToolOptions.instance.DeactivateOptions();
			SCR_GridSnappingOption.instance.ActivateOption();
			SCR_SurfaceSnappingOption.instance.ActivateOption();

			if (variablePointer != null)
			{
				variablePointer.HighlightingActive = true;
				variablePointer.ChangeLayer(13);
			}
			else
			{
				Start();
				variablePointer.HighlightingActive = true;
				variablePointer.ChangeLayer(13);
			}

			SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
			SCR_InputDetection.instance.SubscribeToInput(unhideShortcutButton, DoInputDetected);
		}		
	}

	private void OnDisable()
	{
        if (convertedOutlineMeshes.Count > 0)
        {
            foreach (var item in convertedOutlineMeshes)
            {
				item.layer = 13;
            }
        }

		convertedOutlineMeshes.Clear();

		bActivationButtonPressed = false;
		variablePointer.HighlightingActive = false;
        variablePointer.ChangeLayer(8);
		SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
		SCR_InputDetection.instance.UnsubscribeFromInput(unhideShortcutButton, DoInputDetected);

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

	private void DoInputDetected(InputAction.CallbackContext context)
	{
        if (context.performed)
        {
			inputMovement = context.ReadValue<Vector2>();
		}
        else if (context.canceled)
        {
			inputMovement = Vector3.zero;
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
			OnEnable();
        }
	}

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case ToolStates.Converting:
                ConvertingObject();
                break;
            default:
                break;
        }
    }

    void ConvertingObject()
    {
        //deals with shortcuts for rotating and scaling whilst placing the prefab
        if (bAllowUnhideShortcut)
        {
            if (inputMovement.x > 0.8f)
            {
				timer += Time.deltaTime;
            }
            else
            {
				timer = 0f;
            }

            if (timer >= timeToUnhide)
            {
                if (hiddenOutlineMeshes.Count > 0)
                {
                    foreach (var item in hiddenOutlineMeshes)
                    {
						item.SetActive(true);
                    }

					hiddenOutlineMeshes.Clear();
				}

				timer = 0f;

			}
        }

        if (variablePointer.Active && variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                bBusy = true;

                selectedObject = null;

                variablePointer.RemoveHighlight();
                variablePointer.HighlightingActive = false;

                RaycastHit hit = variablePointer.PointerHit;

                if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.Off)
                {
                    variablePointer.SnapToSurface();
                }

                objectToCopy = hit.transform.gameObject;
				Transform previousParent = objectToCopy.transform.parent;
				objectToCopy.transform.parent = null;

				convertedOutlineMeshes.Add(objectToCopy);
				hiddenOutlineMeshes.Add(objectToCopy);

				selectedObject = Instantiate(objectToCopy, objectToCopy.transform.position, objectToCopy.transform.rotation);
                selectedObject.layer = 8;

                selectedObject.GetComponent<Renderer>().sharedMaterial = convertedObjectMaterial;

                SCR_SaveSystem.instance.ObjectIDNumber++;
                selectedObject.name = "Geometry" + SCR_SaveSystem.instance.ObjectIDNumber;
                SCR_SaveSystem.instance.AddGeometry(selectedObject);
				SCR_ObjectData objectDataScript = selectedObject.AddComponent(typeof(SCR_ObjectData)) as SCR_ObjectData;
				objectDataScript.objectID = SCR_SaveSystem.instance.ObjectIDNumber;

				objectToCopy.layer = 2; //so it doesn't get in the way whilst converting the rest. Might not be necessary.
				objectToCopy.transform.parent = previousParent;
				objectToCopy.SetActive(false);

				bActivationButtonPressed = false;

                variablePointer.HighlightingActive = true;

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bBusy = false;


                currentState = ToolStates.Converting;

            }
            else
            {
                variablePointer.HighlightingActive = true;
                bActivationButtonPressed = false;
            }
        }

    }
}
