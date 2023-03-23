using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_AddPrefab : MonoBehaviour, ITool {

    private enum ToolStates { Placing, Adding };
    private ToolStates currentState = ToolStates.Placing;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private ControllerInputs inputMethodLeft = ControllerInputs.LeftStick;
    [SerializeField] private ControllerInputs inputMethodRight = ControllerInputs.RightStick;
    [SerializeField] private string toolName;
    [SerializeField] private Material guidePrefabMaterial;

    [Header("Shortcuts")]
    [SerializeField] private bool bAllowShortcuts;
    [SerializeField] private float rotationSpeedMultiplier;
    [SerializeField] private float scaleSpeedMultiplier;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private GameObject prefabToSpawn;
    public GameObject PrefabToSpawn
    {
        get { return prefabToSpawn; }
        set { prefabToSpawn = value; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private Vector3 startLocation;
    private Vector3 pointerLocation;
    private Vector3 offsetLocation;
    private SCR_PrefabData prefabDataScript;
    private GameObject newlyCreatedPrefab;
    private GameObject guidePrefab;
    private string previousName;
    private GameObject variableObject;
    private Texture currentObjectTexture;
    private Vector2 inputMovementLeft;
    private Vector2 inputMovementRight;

    private bool bSnap;
    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            CheckGuideStatus();

            SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

            SCR_ToolOptions.instance.DeactivateOptions();
            SCR_GridSnappingOption.instance.ActivateOption();
            SCR_SurfaceSnappingOption.instance.ActivateOption();
            SCR_PrefabRotateTowardsOption.instance.ActivateOption();

            SpawnGuidePrefab();

            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
            SCR_InputDetection.instance.SubscribeToInput(inputMethodLeft, DoInputDetectedLeft);
            SCR_InputDetection.instance.SubscribeToInput(inputMethodRight, DoInputDetectedRight);
        }
        
    }

    private void OnDisable()
    {
        CheckGuideStatus();

        bActivationButtonPressed = false;
        Destroy(guidePrefab);
        guidePrefab = null;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(inputMethodLeft, DoInputDetectedLeft);
        SCR_InputDetection.instance.UnsubscribeFromInput(inputMethodRight, DoInputDetectedRight);
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

    void CheckGuideStatus()
    {
        if (guidePrefab)
        {

            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.Game && guidePrefab.activeInHierarchy)
            {
                guidePrefab.SetActive(false);
            }
            else if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor && !guidePrefab.activeInHierarchy)
            {
                guidePrefab.SetActive(true);
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
    void Update ()
    {
        switch (currentState)
        {
            case ToolStates.Placing:
                PlacingObject();
                break;
            case ToolStates.Adding:
                AddingObject();
                break;
            default:
                break;
        }

        if (guidePrefab != null)
        {

            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                guidePrefab.transform.position = Snap(variablePointer.PointerPosition);
            }
            else
            {
                guidePrefab.transform.position = variablePointer.PointerPosition;
            }

            if (SCR_PrefabRotateTowardsOption.instance.PrefabRotateTowardsActive == OptionActive.On)
            {
                guidePrefab.transform.LookAt(new Vector3(SCR_HeadsetReferences.instance.centerEye.transform.position.x, guidePrefab.transform.position.y, SCR_HeadsetReferences.instance.centerEye.transform.position.z));
            }

            if (SCR_ToolMenuRadial.instance.Busy && guidePrefab.activeInHierarchy)
            {
                guidePrefab.SetActive(false);
            }
            else if (!SCR_ToolMenuRadial.instance.Busy && !guidePrefab.activeInHierarchy)
            {
                CheckGuideStatus();
            }
        }
    }

    //proxy prefab is used as a guide so users can see what they're spawning and where it's going. 
    void SpawnGuidePrefab()
    {
        if (variablePointer == null)
        {
            Start();
        }

        if (prefabToSpawn.tag == "CantCopy")
        {
            GameObject objectToCheck = GameObject.Find(prefabToSpawn.name + "(Clone)");
            if (objectToCheck)
            {
                return;
            }
        }

        guidePrefab = Instantiate(prefabToSpawn, variablePointer.PointerPosition, prefabToSpawn.transform.rotation);

        SCR_PrefabData prefabDataScript = guidePrefab.GetComponent<SCR_PrefabData>();

        //this deals with objects that must be unique, like game managers.
        if (guidePrefab.tag == "CantCopy")
        {
            previousName = guidePrefab.name;
            guidePrefab.tag = "Untagged";
            guidePrefab.name = "GuidePrefab";

            if (prefabDataScript)
            {
                prefabDataScript.CurrentlyGuiding(guidePrefabMaterial, false);
            }
        }
        else
        {
            if (prefabDataScript)
            {
                prefabDataScript.CurrentlyGuiding(guidePrefabMaterial, true);
            }
        }
    }

    void PlacingObject()
    {
        CheckGuideStatus();

        if (variablePointer != null)
        {
            if (variablePointer.Active)
            {
                //deals with shortcuts for rotating and scaling whilst placing the prefab
                if (bAllowShortcuts)
                {
                    if (SCR_PrefabRotateTowardsOption.instance.PrefabRotateTowardsActive == OptionActive.Off)
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

                        if (guidePrefab != null)
                        {
                            guidePrefab.transform.Rotate(new Vector3(0f, rotationAmount * rotationSpeedMultiplier, 0f), Space.Self);
                        }
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

                    if (guidePrefab != null)
                    {
                        guidePrefab.transform.localScale = new Vector3(Mathf.Max(guidePrefab.transform.localScale.x + scaleAmount, 0.05f), Mathf.Max(guidePrefab.transform.localScale.y + scaleAmount, 0.05f), Mathf.Max(guidePrefab.transform.localScale.z + scaleAmount, 0.05f));
                    }
                    
                }
                

                if (bActivationButtonPressed)
                {
                    if (prefabToSpawn.tag == "CantCopy")
                    {
                        GameObject objectToCheck = GameObject.Find(prefabToSpawn.name + "(Clone)");
                        if (objectToCheck)
                        {
                            bActivationButtonPressed = false;
                            return;
                        }
                    }

                    bBusy = true;
                    bActivationButtonPressed = false;
                    Vector3 locationToPlace;

                    if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
                    {
                        locationToPlace = Snap(variablePointer.PointerPosition);
                    }
                    else
                    {
                        locationToPlace = variablePointer.PointerPosition;
                    }

                    if (prefabToSpawn.tag == "CantCopy")
                    {
                        guidePrefab.GetComponent<SCR_PrefabData>().StopGuiding();
                        newlyCreatedPrefab = guidePrefab;
                        newlyCreatedPrefab.name = previousName;
                        guidePrefab = null;
                    }
                    else
                    {
                        newlyCreatedPrefab = Instantiate(prefabToSpawn, locationToPlace, guidePrefab.transform.rotation);
                        newlyCreatedPrefab.transform.localScale = guidePrefab.transform.localScale;
                    }

                    if (SCR_PrefabRotateTowardsOption.instance.PrefabRotateTowardsActive == OptionActive.On)
                    {
                        newlyCreatedPrefab.transform.LookAt(new Vector3(SCR_HeadsetReferences.instance.centerEye.transform.position.x, newlyCreatedPrefab.transform.position.y, SCR_HeadsetReferences.instance.centerEye.transform.position.z));
                    }

                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                    //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                    currentState = ToolStates.Adding;
                }
            }
        }
    }

    void AddingObject()
    {
        if (newlyCreatedPrefab.transform.root.GetComponent<IScriptable>() != null)
        {
            newlyCreatedPrefab.transform.root.GetComponent<IScriptable>().ConfigureNewScriptable();
            SCR_SaveSystem.instance.AddScript(newlyCreatedPrefab);
        }
        else
        {
            SCR_SaveSystem.instance.AddPrefab(newlyCreatedPrefab);
        }
        
        bBusy = false;
        currentState = ToolStates.Placing;
    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
