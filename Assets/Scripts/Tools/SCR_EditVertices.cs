using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VertexGroups
{
    public List<Vector3> vertexPosition = new List<Vector3>();
    public List<int> vertexIndex = new List<int>();
}

public class SCR_EditVertices : MonoBehaviour, ITool
{

    public static SCR_EditVertices instance;

    private enum ToolStates { SelectingObject, ChoosingVertex, MovingVertex };
    private ToolStates currentState = ToolStates.SelectingObject;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;
    [SerializeField] private GameObject vertexHelperPrefab;
    [SerializeField] private float vertexHelperStartingScale;
    [SerializeField] private float vertexHelperMaxScale;
    [SerializeField] private float vertexHelperScaleMultiplier;
    [SerializeField] private bool bScaleBasedOnIndividualVertexHelper;
    [SerializeField] private bool bOnlyAllowCreatedMeshes;
    [SerializeField] private Material vertexHelperMaterialDefault;
    [SerializeField] private Material vertexHelperMaterialSelected;
    [SerializeField] private Material vertexHelperMaterialHighlighted;
    //[SerializeField] private Color vertexHelperColour = Color.red;
    //[SerializeField] private Color vertexHelperSelectedColour = Color.blue;
    //[SerializeField] private Color vertexHelperHighlightedColour = Color.blue;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;

    private GameObject selectedObject;
    private GameObject selectedHelperObject;
    private GameObject highlightedHelperObject;
    private SCR_VertexHelper selectedHelperScript;
    private Color objectStartColor;
    private MeshFilter currentMeshFilter;
    private Mesh currentMesh;
    private MeshCollider currentMeshCollider;
    [SerializeField] private float vertexHelperCurrentScale;
    private Transform headsetTransform;
    private Material objectMaterial;

    //list of vertices from the mesh
    private List<Vector3> vertexList = new List<Vector3>();

    //list of indexes of vertices that have already been grouped
    private List<int> vertexIndexMatched = new List<int>();

    //list of vertex groups that include their positions and their index.
    //this is used to move all the vertices at once.
    private List<VertexGroups> vertexGroupList = new List<VertexGroups>();

    [SerializeField] private List<GameObject> vertexHelperList = new List<GameObject>();

    private Vector3[] verticesOfMesh;

    private Vector3 startLocation;
    private Vector3 offsetLocation;

    private bool bFirstTime = true;

    private void OnEnable()
    {

        if (!bFirstTime)
        {
            SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

            SCR_ToolOptions.instance.DeactivateOptions();
            SCR_GridSnappingOption.instance.ActivateOption();
            SCR_SurfaceSnappingOption.instance.ActivateOption();

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

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

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
            case ToolStates.SelectingObject:
                SelectingObject();
                break;
            case ToolStates.ChoosingVertex:
                ChoosingVertex();
                break;
            case ToolStates.MovingVertex:
                MovingVertex();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
    {
        if (variablePointer != null)
        {
            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                if (bActivationButtonPressed)
                {
                    if (bOnlyAllowCreatedMeshes)
                    {
                        if (variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                        {
                            SelectingObjectSetup();
                        }
                    }
                    else
                    {
                        SelectingObjectSetup();
                    }
                }
            }
            else
            {
                bActivationButtonPressed = false;
            }
        }
    }

    void SelectingObjectSetup()
    {
        if (variablePointer.PointerHit.transform.gameObject.GetComponent<MeshCollider>())
        {
            bBusy = true;

            variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

            variablePointer.HighlightingActive = false;
            variablePointer.RemoveHighlight();

            selectedObject = variablePointer.PointerHit.transform.gameObject;
            //selectedObject.layer = 2;
            objectMaterial = selectedObject.GetComponent<Renderer>().sharedMaterial;
            currentMeshFilter = selectedObject.GetComponent<MeshFilter>();
            currentMesh = currentMeshFilter.mesh;
            selectedObject.GetComponent<Renderer>().sharedMaterial = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
            selectedObject.GetComponent<Renderer>().material.mainTexture = objectMaterial.mainTexture;
            currentMeshCollider = selectedObject.GetComponent<MeshCollider>();

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);

            bActivationButtonPressed = false;

            GenerateVertexHelpers();

            variablePointer.SetLayerMask(10);

            currentState = ToolStates.ChoosingVertex;
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }

    void ChoosingVertex()
    {
        UpdateHelperScales();

        if (variablePointer != null)
        {
            if (bActivationButtonPressed)
            {
                if (variablePointer.Active && variablePointer.ValidRaycastTarget)
                {
                    selectedHelperObject = variablePointer.PointerHit.collider.gameObject;
                    selectedHelperObject.GetComponent<Renderer>().sharedMaterial = vertexHelperMaterialSelected;
                    selectedHelperScript = selectedHelperObject.GetComponent<SCR_VertexHelper>();
                    selectedHelperObject.layer = 2;
                    startLocation = selectedObject.transform.position;

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                    {
                        variablePointer.LockPointerLength(true);
                    }

                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                    currentState = ToolStates.MovingVertex;
                }
                else
                {

                    for (int i = 0; i < vertexHelperList.Count; i++)
                    {
                        Destroy(vertexHelperList[i]);
                    }

                    vertexHelperList.Clear();
                    vertexList.Clear();
                    vertexIndexMatched.Clear();
                    vertexGroupList.Clear();


                    variablePointer.SetLayerMask(8);

                    if (selectedObject.transform.root.GetComponent<SCR_GroupParent>() != null)
                    {
                        selectedObject.transform.root.GetComponent<SCR_GroupParent>().Deselected();
                    }
                    else
                    {
                        selectedObject.GetComponent<Renderer>().sharedMaterial = objectMaterial;
                    }

                    variablePointer.HighlightingActive = false;
                    bBusy = false;

                    variablePointer.SetPointerColourDefault();

                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                    currentState = ToolStates.SelectingObject;
                }

                

                bActivationButtonPressed = false;
            }

            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                if (variablePointer.PointerHit.collider.gameObject != highlightedHelperObject)
                {
                    highlightedHelperObject = variablePointer.PointerHit.collider.gameObject;
                    highlightedHelperObject.GetComponent<Renderer>().sharedMaterial = vertexHelperMaterialHighlighted;
                    highlightedHelperObject.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else
            {
                if (highlightedHelperObject != null)
                {
                    highlightedHelperObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
                    highlightedHelperObject.GetComponent<Renderer>().sharedMaterial = vertexHelperMaterialDefault;
                    highlightedHelperObject = null;
                }
            }

        }


    }

    void MovingVertex()
    {
        UpdateHelperScales();

        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            selectedHelperObject.transform.position = Snap(variablePointer.PointerPosition);
        }
        else
        {
            selectedHelperObject.transform.position = variablePointer.PointerPosition;

        }

        offsetLocation = selectedHelperObject.transform.position;

        offsetLocation = selectedObject.transform.InverseTransformPoint(offsetLocation);

        UpdateVertexPositions(selectedHelperScript.vertexIndexList, offsetLocation);

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;
            currentMeshCollider.sharedMesh = currentMesh;
            highlightedHelperObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
            highlightedHelperObject.GetComponent<Renderer>().sharedMaterial = vertexHelperMaterialDefault;
            highlightedHelperObject = null;
            selectedHelperObject.layer = 10;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            currentState = ToolStates.ChoosingVertex;
        }

    }

    void GenerateVertexHelpers()
    {
        currentMesh.GetVertices(vertexList);

        for (int i = 0; i < vertexList.Count; i++)
        {
            Vector3 currentVertexPosition = vertexList[i];
            VertexGroups vg = new VertexGroups();
            int k = 0;
            for (int j = 0; j < vertexList.Count; j++)
            {
                if (vertexList[j] == currentVertexPosition && (!vertexIndexMatched.Contains(j)))
                {
                    vg.vertexPosition.Add(selectedObject.transform.TransformPoint(vertexList[j]));
                    vg.vertexIndex.Add(j);
                    vertexIndexMatched.Add(j);
                    k++;
                }
            }

            if (k >= 1)
            {
                vertexGroupList.Add(vg);
            }
        }

        foreach (var item in vertexGroupList)
        {
            GameObject goHelper = (GameObject)Instantiate(vertexHelperPrefab, item.vertexPosition[0], Quaternion.identity) as GameObject;
            vertexHelperList.Add(goHelper);
            SCR_VertexHelper vertexHelperScript = goHelper.GetComponent<SCR_VertexHelper>();

            for (int i = 0; i < item.vertexIndex.Count; i++)
            {
                vertexHelperScript.vertexIndexList.Add(item.vertexIndex[i]);
            }

        }

        verticesOfMesh = new Vector3[currentMesh.vertices.Length];
    }

    public void UpdateVertexPositions(List<int> verticesToUpdate, Vector3 newPosition)
    {

        verticesOfMesh = currentMesh.vertices;

        for (int i = 0; i < verticesToUpdate.Count; i++)
        {
            verticesOfMesh[verticesToUpdate[i]] = newPosition;
        }

        currentMesh.vertices = verticesOfMesh;

        currentMesh.RecalculateBounds();
        currentMesh.RecalculateNormals();

        currentMeshFilter.mesh = currentMesh;
    }

    private void UpdateHelperScales()
    {
        if (bScaleBasedOnIndividualVertexHelper)
        {
            foreach (var item in vertexHelperList)
            {
                Vector3 difference = headsetTransform.position - item.transform.position;
                float distance = difference.magnitude;

                vertexHelperCurrentScale = Mathf.Min(vertexHelperStartingScale * distance * vertexHelperScaleMultiplier, vertexHelperMaxScale);
                vertexHelperCurrentScale = Mathf.Max(vertexHelperCurrentScale, vertexHelperStartingScale);
                item.transform.localScale = new Vector3(vertexHelperCurrentScale, vertexHelperCurrentScale, vertexHelperCurrentScale);
            }
        }
        else
        {
            Vector3 difference = headsetTransform.position - selectedObject.transform.position;
            float distance = difference.magnitude;

            vertexHelperCurrentScale = Mathf.Min(vertexHelperStartingScale * distance * vertexHelperScaleMultiplier, vertexHelperMaxScale);
            vertexHelperCurrentScale = Mathf.Max(vertexHelperCurrentScale, vertexHelperStartingScale);

            foreach (var item in vertexHelperList)
            {
                item.transform.localScale = new Vector3(vertexHelperCurrentScale, vertexHelperCurrentScale, vertexHelperCurrentScale);
            }
        }

    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
