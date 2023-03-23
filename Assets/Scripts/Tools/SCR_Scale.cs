using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_Scale : MonoBehaviour, ITool {

    private enum ToolStates { Selecting, Scaling };
    private ToolStates currentState = ToolStates.Selecting;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private ControllerInputs inputMethod = ControllerInputs.RightStick;
    [SerializeField] private string toolName;
    [SerializeField] private float scaleMultiplier;
    [SerializeField] private bool scaleFromCentre;

    [Header("Widget Settings")]
    [SerializeField] private GameObject scaleWidgetPrefab;
    [SerializeField] private float widgetMinimumScale = 0.1f;
    [SerializeField] private float widgetMaximumScale = 2.0f;
    [SerializeField] private float widgetScaleMultiplier = 1.1f;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private GameObject variableObject;
    private GameObject selectedObject;
    private GameObject tempObjectForScaling;
    private Material objectStartMaterial;

    private float scaleAmount;
    private float startingBoundsX;
    private Vector3 startScale;
    private Vector3 controllerStartPosition;

    private SCR_GroupParent groupParentScript;
    private SCR_ToolOptions toolOptions;

    private AxisSelected previousAxis;
    private AxisSelected previousWidgetAxis;
    private GameObject widgetParent;
    private GameObject xAxisWidget;
    private GameObject yAxisWidget;
    private GameObject zAxisWidget;
    private SCR_WidgetData widgetData;
    private Transform headsetTransform;
    private float widgetScale;
    private float widgetStartingScale;
    private GameObject tempObjectForHighlighting;
    private bool bAllowShortcut;
    private bool bShortcutInputTriggered;
    private Vector2 inputMovement;

    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

            SCR_ToolOptions.instance.DeactivateOptions();
            SCR_GridSnappingOption.instance.ActivateOption();
            SCR_AxisOption.instance.ActivateOption();
            SCR_AxisOption.instance.AllowAllAxis(true);

            if (widgetParent == null)
            {
                widgetParent = Instantiate(scaleWidgetPrefab, transform.position, transform.rotation);
                widgetParent.SetActive(false);
                widgetData = widgetParent.GetComponent<SCR_WidgetData>();
                xAxisWidget = widgetData.widgetChildren[0];
                yAxisWidget = widgetData.widgetChildren[1];
                zAxisWidget = widgetData.widgetChildren[2];
                widgetStartingScale = widgetParent.transform.localScale.x;
            }

            headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;

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
            SCR_InputDetection.instance.SubscribeToInput(inputMethod, DoInputDetected);
        }
        
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        variablePointer.HighlightingActive = false;

        if (widgetParent != null)
        {
            widgetParent.SetActive(false);
        }

        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(inputMethod, DoInputDetected);
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
            case ToolStates.Scaling:
                ScalingObject();
                break;
            default:
                break;
        }

        ShortcutHandling();
    }

    //Supports switching axis from the analogue stick
    void ShortcutHandling()
    {
        if (bAllowShortcut)
        {
            if (inputMovement.x > 0.5f)
            {
                if (!bShortcutInputTriggered)
                {
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.RightHand);
                    SCR_AxisOption.instance.ShortcutSwapAxisOption(false);
                }
                bShortcutInputTriggered = true;
            }
            else if (inputMovement.x < -0.5f)
            {
                if (!bShortcutInputTriggered)
                {
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.RightHand);
                    SCR_AxisOption.instance.ShortcutSwapAxisOption(true);
                }
                bShortcutInputTriggered = true;
            }
            else
            {
                bShortcutInputTriggered = false;
            }
        }
    }

    void SelectingObject()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget && !variablePointer.PointerHit.transform.CompareTag("Floor"))
        {

            bAllowShortcut = true;
            if (variablePointer.PointerHit.transform.parent)
            {
                tempObjectForScaling = variablePointer.PointerHit.transform.root.gameObject;
            }
            else
            {
                if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                {
                    tempObjectForScaling = variablePointer.PointerHit.transform.gameObject;
                    tempObjectForScaling.transform.rotation = variablePointer.PointerHit.transform.gameObject.transform.rotation;
                }
                else
                {
                    if (tempObjectForHighlighting == null)
                    {
                        tempObjectForHighlighting = new GameObject("TempScaling");
                    }
                    tempObjectForScaling = tempObjectForHighlighting;
                    tempObjectForScaling.transform.rotation = variablePointer.PointerHit.transform.gameObject.transform.localRotation;
                    //tempObjectForScaling.transform.forward = variablePointer.PointerHit.transform.gameObject.transform.forward;

                    if (scaleFromCentre)
                    {
                        tempObjectForScaling.transform.position = variablePointer.PointerHit.transform.gameObject.GetComponent<Renderer>().bounds.center;
                    }
                    else
                    {
                        tempObjectForScaling.transform.position = variablePointer.PointerHit.transform.position;
                    }
                }

            }

            //Deal with the rotation widgets while highlighting so you can see which axis is currently selected
            widgetParent.SetActive(true);
            widgetParent.transform.position = tempObjectForScaling.transform.position;
            widgetParent.transform.rotation = tempObjectForScaling.transform.rotation;
            //widgetParent.transform.parent = tempObjectForScaling.transform;

            //Deal with scale of roation widget based on distance
            WidgetScaling();
            WidgetAxisHighlighting();

            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                variablePointer.HighlightingActive = false;
                variablePointer.RemoveHighlight();

                if (variablePointer.PointerHit.transform.parent)
                {
                    if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.root.gameObject;

                        groupParentScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                        groupParentScript.CheckMaterialCache();
                        groupParentScript.CurrentlySelected();

                        startScale = selectedObject.transform.localScale;
                    }
                    else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.root.gameObject;
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        startScale = selectedObject.transform.localScale;
                    }
                    
                }
                else
                {
                    if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.gameObject;
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        startScale = selectedObject.transform.localScale;
                    }
                    else if (variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.gameObject;
                        selectedObject.layer = 2;
                        objectStartMaterial = selectedObject.GetComponent<Renderer>().sharedMaterial;
                        selectedObject.GetComponent<Renderer>().sharedMaterial = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
                        selectedObject.GetComponent<Renderer>().sharedMaterial.mainTexture = objectStartMaterial.mainTexture;

                        if (scaleFromCentre)
                        {
                            tempObjectForScaling = new GameObject("TempScaling");
                            tempObjectForScaling.transform.position = selectedObject.GetComponent<Renderer>().bounds.center;
                            tempObjectForScaling.transform.rotation = selectedObject.transform.rotation;
                            selectedObject.transform.parent = tempObjectForScaling.transform;
                            tempObjectForScaling.transform.localScale = Vector3.one;
                            startScale = tempObjectForScaling.transform.localScale;
                        }
                        else
                        {
                            startScale = selectedObject.transform.localScale;
                        }
                    }
                }
                
                controllerStartPosition = variableObject.transform.position;

                widgetParent.SetActive(true);
                widgetParent.transform.position = tempObjectForScaling.transform.position;
                //widgetParent.transform.localScale *= Mathf.Min(selectedObject.GetComponent<Renderer>().bounds.extents.magnitude * 2f, 1.5f);

                widgetParent.transform.rotation = tempObjectForScaling.transform.rotation;
                //widgetParent.transform.parent = tempObjectForScaling.transform;

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                currentState = ToolStates.Scaling;
            }
        }
        else
        {
            variablePointer.HighlightingActive = true;
            widgetParent.transform.localScale = Vector3.one;
            widgetParent.transform.parent = null;
            widgetParent.SetActive(false);
            Destroy(tempObjectForHighlighting);
            bActivationButtonPressed = false;
        }
    }

    void ScalingObject()
    {

        scaleAmount = (controllerStartPosition.y - variableObject.transform.position.y) * scaleMultiplier;

        //this creates a dead zone to make it safer to swap axis. Also creates a snap to the starting scale.
        if (scaleAmount <= 0.075f && scaleAmount >= -0.075f && SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            scaleAmount = 0f;
        }

        AxisSelected currentAxis = SCR_AxisOption.instance.CurrentAxis;

        WidgetAxisHighlighting();
        WidgetScaling();

        //this ensures the user can swap axis mid scaling and it maintain their last changes
        if (previousAxis != currentAxis)
        {
            controllerStartPosition = variableObject.transform.position;
            previousAxis = currentAxis;
            scaleAmount = 0f;

            if (scaleFromCentre)
            {
                startScale = tempObjectForScaling.transform.localScale;
            }
            else
            {
                startScale = selectedObject.transform.localScale;
            }
        }

        if (scaleFromCentre)
        {
            if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.None)
            {
                tempObjectForScaling.transform.localScale = new Vector3(Mathf.Clamp(startScale.x + scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.y + scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.z + scaleAmount, 0, Mathf.Infinity));
            }
            else if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.XAxis)
            {
                tempObjectForScaling.transform.localScale = new Vector3(Mathf.Clamp(startScale.x + scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.y, 0, Mathf.Infinity), Mathf.Clamp(startScale.z, 0, Mathf.Infinity));
            }
            else if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.YAxis)
            {
                tempObjectForScaling.transform.localScale = new Vector3(Mathf.Clamp(startScale.x, 0, Mathf.Infinity), Mathf.Clamp(startScale.y - scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.z, 0, Mathf.Infinity));
            }
            else if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.ZAxis)
            {
                tempObjectForScaling.transform.localScale = new Vector3(Mathf.Clamp(startScale.x, 0, Mathf.Infinity), Mathf.Clamp(startScale.y, 0, Mathf.Infinity), Mathf.Clamp(startScale.z + scaleAmount, 0, Mathf.Infinity));
            }
        }
        else
        {
            if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.None)
            {
                selectedObject.transform.localScale = new Vector3(Mathf.Clamp(startScale.x + scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.y + scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.z + scaleAmount, 0, Mathf.Infinity));
            }
            else if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.XAxis)
            {
                selectedObject.transform.localScale = new Vector3(Mathf.Clamp(startScale.x + scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.y, 0, Mathf.Infinity), Mathf.Clamp(startScale.z, 0, Mathf.Infinity));
            }
            else if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.YAxis)
            {
                selectedObject.transform.localScale = new Vector3(Mathf.Clamp(startScale.x, 0, Mathf.Infinity), Mathf.Clamp(startScale.y - scaleAmount, 0, Mathf.Infinity), Mathf.Clamp(startScale.z, 0, Mathf.Infinity));
            }
            else if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.ZAxis)
            {
                selectedObject.transform.localScale = new Vector3(Mathf.Clamp(startScale.x, 0, Mathf.Infinity), Mathf.Clamp(startScale.y, 0, Mathf.Infinity), Mathf.Clamp(startScale.z + scaleAmount, 0, Mathf.Infinity));
            }
            
        }

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;

            widgetParent.transform.parent = null;
            widgetParent.transform.localScale = Vector3.one;
            widgetParent.SetActive(false);

            if (selectedObject.GetComponent<SCR_GroupParent>() != null)
            {
                groupParentScript.Deselected();

                for (int i = 0; i < groupParentScript.groupedObjectList.Count; i++)
                {
                    if (groupParentScript.groupedObjectList[i].GetComponent<SphereCollider>())
                    {
                        Destroy(groupParentScript.groupedObjectList[i].GetComponent<SphereCollider>());
                        MeshCollider meshCol = groupParentScript.groupedObjectList[i].AddComponent<MeshCollider>();
                        meshCol.sharedMesh = groupParentScript.groupedObjectList[i].GetComponent<MeshFilter>().sharedMesh;
                    }
                }
            }
            else if(selectedObject.GetComponent<SCR_PrefabData>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
            }
            else
            {
                if (selectedObject.GetComponent<SphereCollider>())
                {
                    Destroy(selectedObject.GetComponent<SphereCollider>());
                    MeshCollider meshCol = selectedObject.AddComponent<MeshCollider>();
                    meshCol.sharedMesh = selectedObject.GetComponent<MeshFilter>().sharedMesh;
                }

                selectedObject.layer = 8;
                selectedObject.GetComponent<Renderer>().sharedMaterial = objectStartMaterial;
                selectedObject.transform.parent = null;
                Destroy(tempObjectForScaling);
            }

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;

            variablePointer.SetPointerColourDefault();

            variablePointer.HighlightingActive = true;

            currentState = ToolStates.Selecting;
        }

    }

    //Deals with scale of scaling widget based on distance
    void WidgetScaling()
    {
        Vector3 difference = headsetTransform.position - widgetParent.transform.position;
        float distance = difference.magnitude;
        widgetScale = Mathf.Min(widgetStartingScale * distance * widgetScaleMultiplier, widgetMaximumScale);
        widgetScale = Mathf.Max(widgetScale, widgetMinimumScale);
        widgetParent.transform.localScale = new Vector3(widgetScale, widgetScale, widgetScale);
    }

    //Changes which widget axis torus should be coloured (highlighted)
    void WidgetAxisHighlighting()
    {
        if (previousWidgetAxis != SCR_AxisOption.instance.CurrentAxis)
        {

            widgetData.widgetChildrenRenderers[0].material = widgetData.defaultMaterial;
            widgetData.widgetChildrenRenderers[1].material = widgetData.defaultMaterial;
            widgetData.widgetChildrenRenderers[2].material = widgetData.defaultMaterial;

            switch (SCR_AxisOption.instance.CurrentAxis)
            {
                case AxisSelected.XAxis:
                    widgetData.widgetChildrenRenderers[0].material = widgetData.widgetChildrenMaterials[0];
                    previousWidgetAxis = AxisSelected.XAxis;
                    break;
                case AxisSelected.YAxis:
                    widgetData.widgetChildrenRenderers[1].material = widgetData.widgetChildrenMaterials[1];
                    previousWidgetAxis = AxisSelected.YAxis;
                    break;
                case AxisSelected.ZAxis:
                    widgetData.widgetChildrenRenderers[2].material = widgetData.widgetChildrenMaterials[2];
                    previousWidgetAxis = AxisSelected.ZAxis;
                    break;
                case AxisSelected.None:
                    widgetData.widgetChildrenRenderers[0].material = widgetData.widgetChildrenMaterials[0];
                    widgetData.widgetChildrenRenderers[1].material = widgetData.widgetChildrenMaterials[1];
                    widgetData.widgetChildrenRenderers[2].material = widgetData.widgetChildrenMaterials[2];
                    previousWidgetAxis = AxisSelected.None;
                    break;
            }
        }


    }

}
