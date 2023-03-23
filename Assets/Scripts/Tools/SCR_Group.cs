using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_Group : MonoBehaviour, ITool {

    private enum ToolStates { Grouping, Idling };
    private ToolStates currentState = ToolStates.Grouping;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private Dictionary<GameObject, Material> materialDictionary = new Dictionary<GameObject, Material>();

    private bool bActivationButtonPressed;
    private bool bCreatingNewObject;
    private IPointer variablePointer;
    private GameObject selectedObject;
    private GameObject newParentObject;
    private SCR_GroupParent newParentObjectScript;
    private Vector3 objectOffset;
    private Color objectStartColour;
    private Color currentGroupColour;

    private bool bFirstTime = true;

    //deals with custom highlighting
    private bool bHighlightingActive = true;
    private GameObject highlightedObject;
    private Renderer highlightedObjectRenderer;
    private Color highlightedObjectDefaultColour;
    private Material highlightedObjectDefaultMaterial;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        SCR_ToolOptions.instance.DeactivateOptions();

        SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
    }

    private void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
            {
                if (!bActivationButtonPressed)
                {
                    bActivationButtonPressed = true;
                }
            }
        }
        else if (context.canceled)
        {
            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
            {
                if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
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
            if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
            {
                bActivationButtonPressed = false;
            }
        }
    }*/

    private void Start()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        bCreatingNewObject = true;

        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
        }

    }

    private void Update()
    {
        switch (currentState)
        {
            case ToolStates.Grouping:
                GroupObjects();
                break;
            case ToolStates.Idling:
                ToolIdling();
                break;
            default:
                break;
        }
    }

    private void GroupObjects()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget && variablePointer.PointerHit.transform.parent == null)
        {
            variablePointer.HighlightingActive = false;
            variablePointer.RemoveHighlight();
            RemoveHighlight();
            HighlightObjects(variablePointer.PointerHit.collider.gameObject);

            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                variablePointer.RemoveHighlight();
                variablePointer.HighlightingActive = false;

                //create parent first time
                if (bCreatingNewObject)
                {
                    bCreatingNewObject = false;
                    SCR_SaveSystem.instance.GroupNumber++;
                    newParentObject = new GameObject("Group" + SCR_SaveSystem.instance.GroupNumber);
                    newParentObject.transform.localPosition = variablePointer.PointerHit.transform.position;
                    newParentObject.transform.localScale = Vector3.one;
                    newParentObject.AddComponent<SCR_GroupParent>();
                    //currentGroupColour = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), 1f);
                }

                RemoveHighlight();
                newParentObjectScript = newParentObject.GetComponent<SCR_GroupParent>();
                newParentObjectScript.UpdateCachedMaterials();
                selectedObject = variablePointer.PointerHit.transform.gameObject;
                materialDictionary.Add(selectedObject, highlightedObjectDefaultMaterial);
                newParentObjectScript.groupedObjectList.Add(selectedObject);
                selectedObject.GetComponent<Renderer>().sharedMaterial = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
                selectedObject.GetComponent<Renderer>().material.mainTexture = highlightedObjectDefaultMaterial.mainTexture;


                //selectedObject.GetComponent<Renderer>().material.color = Color.red;

                if (newParentObjectScript.groupedObjectList.Count >= 1)
                {
                    ReCenter(newParentObjectScript);
                }

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                //currentState = ToolStates.Moving;
            }
        }
        else if (variablePointer.Active && !variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                if (newParentObjectScript)
                {
                    foreach (var item in newParentObjectScript.groupedObjectList)
                    {
                        item.GetComponent<Renderer>().sharedMaterial = materialDictionary[item];
                    }
                }

                //complete group and save
                newParentObjectScript.bMaterialsCached = false;
                newParentObjectScript.UpdateCachedMaterials();
                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bActivationButtonPressed = false;
                currentState = ToolStates.Idling;
            }
            RemoveHighlight();
        }
        else if (bBusy && bActivationButtonPressed)
        {
            if (variablePointer.PointerHit.transform.parent != null)
            {
                selectedObject = variablePointer.PointerHit.transform.gameObject;

                if (materialDictionary.ContainsKey(selectedObject))
                {
                    //selectedObject.GetComponent<Renderer>().material.color = objectStartColour;
                    selectedObject.GetComponent<Renderer>().sharedMaterial = materialDictionary[selectedObject];
                    selectedObject.transform.parent = null;
                    newParentObjectScript.groupedObjectList.Remove(selectedObject);
                    materialDictionary.Remove(selectedObject);
                    bActivationButtonPressed = false;

                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                    //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                    return;
                }
                /*
                if (newParentObjectScript.groupedObjectList.Contains(selectedObject))
                {
                    
                }
                */
            }
        }
        else
        {
            RemoveHighlight();
            variablePointer.HighlightingActive = true;
            bActivationButtonPressed = false;
        }
    }

    private void ToolIdling()
    {
        
        bCreatingNewObject = true;

        if (newParentObjectScript)
        {
            if (newParentObjectScript.groupedObjectList.Count >= 1)
            {
                
                print("in idling");
                foreach (var item in newParentObjectScript.groupedObjectList)
                {
                    item.GetComponent<SCR_ObjectData>().parentName = newParentObject.name;
                    item.GetComponent<SCR_ObjectData>().parentID = SCR_SaveSystem.instance.GroupNumber;
                    //item.GetComponent<Renderer>().sharedMaterial = materialDictionary[item];
                    //item.GetComponent<Renderer>().material.color = currentGroupColour;
                }

                newParentObjectScript.ID = SCR_SaveSystem.instance.GroupNumber;
                SCR_SaveSystem.instance.AddParent(newParentObject);
                newParentObjectScript = null;

            }
            else
            {
                Destroy(newParentObject);
            }
        }

        materialDictionary.Clear();

        bBusy = false;

        variablePointer.SetPointerColourDefault();

        currentState = ToolStates.Grouping;
    }

    private void ReCenter(SCR_GroupParent groupParentScript)
    {
        foreach (var item in groupParentScript.groupedObjectList)
        {
            item.transform.parent = null;
        }

        newParentObject.transform.position = FindCentre(groupParentScript.groupedObjectList);

        foreach(var item in groupParentScript.groupedObjectList)
        {
            item.transform.parent = newParentObject.transform;
        }

    }

    //custom highlighting code
    void HighlightObjects(GameObject newObject)
    {
        if (bHighlightingActive)
        {
            if (highlightedObject != newObject)
            {
                if (highlightedObjectRenderer != null)
                {
                    //highlightedObjectRenderer.sharedMaterial = SCR_ToolMenuRadial.instance.highlightedObjectMaterial;
                }

                highlightedObject = newObject;
                highlightedObjectRenderer = highlightedObject.GetComponent<Renderer>();
                highlightedObjectDefaultMaterial = highlightedObjectRenderer.sharedMaterial;
                highlightedObjectRenderer.sharedMaterial = SCR_ToolMenuRadial.instance.highlightedObjectMaterial;
                highlightedObjectRenderer.material.mainTexture = highlightedObjectDefaultMaterial.mainTexture;
            }
        }
    }

    void RemoveHighlight()
    {
        if (highlightedObjectRenderer != null)
        {
            if (newParentObjectScript != null)
            {
                if (newParentObjectScript.groupedObjectList.Contains(highlightedObject))
                {
                    highlightedObjectRenderer.sharedMaterial = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
                    highlightedObjectRenderer.material.mainTexture = highlightedObjectDefaultMaterial.mainTexture;
                }
                else
                {
                    highlightedObjectRenderer.sharedMaterial = highlightedObjectDefaultMaterial;
                }
            }
            else
            {
                highlightedObjectRenderer.sharedMaterial = highlightedObjectDefaultMaterial;
            }
            
            highlightedObject = null;
            highlightedObjectRenderer = null;
        }

        
    }

    //Adapated from robertu (https://answers.unity.com/questions/511841/how-to-make-an-object-move-away-from-three-or-more.html)
    private Vector3 FindCentre(List<GameObject> targets)
    {
        if (targets.Count == 0)
        {
            return Vector3.zero;
        }
            
        if (targets.Count == 1)
        {
            return targets[0].transform.position;
        }
            
        var bounds = new Bounds(targets[0].GetComponent<Renderer>().bounds.center, Vector3.zero);

        for (var i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].GetComponent<Renderer>().bounds.center);
        }
            
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(bounds.center);
    }
}
