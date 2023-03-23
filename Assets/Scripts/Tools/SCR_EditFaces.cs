using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_EditFaces : MonoBehaviour, ITool {

    public static SCR_EditFaces instance;

    private enum ToolStates { SelectingObject, SelectingFaces, MovingFaces };
    private ToolStates currentState = ToolStates.SelectingObject;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;
    [SerializeField] private Material mat;
    [SerializeField] private Color meshEditingColour = Color.red;
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private Material faceEditingMaterial;
    [SerializeField] private bool bOnlyAllowCreatedMeshes;

    //[SerializeField] private Color vertextHelperColour = Color.red;
    //[SerializeField] private Color vertextHelperSelectedColour = Color.red;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private GameObject selectedObject;
    private Vector3[] normals;
    private Vector3[] vertices;
    private Vector3[] movingVertices;
    private int[] triangles;
    private Vector2[] originalUVs;
    private Vector2[] editingUVs;

    [SerializeField] private List<int> verticesMatched = new List<int>();

    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private MeshFilter currentMeshFilter;
    private Mesh currentMesh;
    private MeshCollider currentMeshCollider;
    private Material objectMaterial;
    private Color objectStartColor;
    private Vector3 selectedNormal;
    private Vector3 startLocation;
    private Vector3 pointerLocation;
    private Vector3 offsetLocation;

    private Vector3 testOffset;
    private Vector3 selectedFaceNormal; //normal of face selected.

    //used to optimise the face highlighing to it doesn't run every frame.
    private Vector3 triangleIndexOne;
    private Vector3 triangleIndexTwo;
    private Vector3 triangleIndexThree;

    private bool bFirstTime = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

            SCR_ToolOptions.instance.DeactivateOptions();
            SCR_GridSnappingOption.instance.ActivateOption();
            SCR_SurfaceSnappingOption.instance.ActivateOption();
            SCR_LockToFaceOption.instance.ActivateOption();

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
            case ToolStates.SelectingObject:
                SelectingObject();
                break;
            case ToolStates.SelectingFaces:
                SelectingFaces();
                break;
            case ToolStates.MovingFaces:
                MovingFaces();
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
        bBusy = true;

        variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

        selectedObject = variablePointer.PointerHit.transform.gameObject;
        selectedObject.layer = 10;

        objectStartColor = variablePointer.ObjectOriginalColour;
        variablePointer.HighlightingActive = false;
        variablePointer.RemoveHighlight();

        objectMaterial = selectedObject.GetComponent<Renderer>().sharedMaterial;

        currentMeshFilter = selectedObject.GetComponent<MeshFilter>();
        currentMesh = currentMeshFilter.mesh;

        //Move all UVs onto the selected colour part of material
        originalUVs = new Vector2[currentMesh.uv.Length];
        originalUVs = currentMesh.uv;

        editingUVs = new Vector2[currentMesh.uv.Length];
        editingUVs = currentMesh.uv;

        for (int i = 0; i < editingUVs.Length; i++)
        {
            editingUVs[i] = new Vector2(0.1f, 0.1f);
        }

        currentMesh.uv = editingUVs;

        currentMeshCollider = selectedObject.GetComponent<MeshCollider>();
        currentMeshCollider.convex = false;

        selectedObject.GetComponent<Renderer>().sharedMaterial = faceEditingMaterial;

        SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
        //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);

        bActivationButtonPressed = false;

        GetMeshData();

        variablePointer.SetLayerMask(10);

        currentState = ToolStates.SelectingFaces;
    }

    void SelectingFaces()
    {

        if (variablePointer != null)
        {
            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                verticesMatched.Clear();

                for (int j = 0; j < editingUVs.Length; j++)
                {
                    editingUVs[j] = new Vector2(0.1f, 0.1f);
                }

                HighlightFace();
            }
            else
            {
                for (int j = 0; j < editingUVs.Length; j++)
                {
                    editingUVs[j] = new Vector2(0.1f, 0.1f);
                }

                triangleIndexOne = Vector3.zero;
                triangleIndexTwo = Vector3.zero;
                triangleIndexThree = Vector3.zero;

                currentMesh.uv = editingUVs;
            }

            if (bActivationButtonPressed)
            {
                

                if (variablePointer.Active && variablePointer.ValidRaycastTarget)
                {
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);

                    startLocation = selectedObject.transform.position;

                    //selectedObject.GetComponent<Renderer>().material.color = Color.white;

                    verticesMatched.Clear();
                    GetMeshData();
                    testOffset = startLocation - variablePointer.PointerPosition;
                    
                    SelectFace();

                    selectedObject.layer = 2;

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                    {
                        variablePointer.LockPointerLength(true);
                    }

                    currentState = ToolStates.MovingFaces;
                }
                else
                {
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);

                    verticesMatched.Clear();

                    variablePointer.SetLayerMask(8);
                    //currentMeshCollider.convex = true;

                    selectedObject.layer = 8;

                    currentMesh.uv = originalUVs;
                    selectedObject.GetComponent<Renderer>().sharedMaterial = objectMaterial;

                    if (selectedObject.transform.root.GetComponent<SCR_GroupParent>() != null)
                    {
                        selectedObject.transform.root.GetComponent<SCR_GroupParent>().Deselected();
                    }
                    else
                    {
                        selectedObject.GetComponent<Renderer>().sharedMaterial = objectMaterial;
                    }
                    
                    variablePointer.SetPointerColourDefault();

                    bBusy = false;
                    variablePointer.HighlightingActive = true;
                    currentState = ToolStates.SelectingObject;
                }

                bActivationButtonPressed = false;
            }
        }
    }

    void MovingFaces()
    {
        pointerLocation = variablePointer.PointerPosition;

        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            pointerLocation = Snap(pointerLocation + testOffset);
        }
        else
        {
            pointerLocation = pointerLocation + testOffset;
        }

        offsetLocation = pointerLocation;
        
        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            Snap(offsetLocation);
        }

        offsetLocation = selectedObject.transform.InverseTransformPoint(offsetLocation);

        if (SCR_LockToFaceOption.instance.LockToFaceActive == OptionActive.On)
        {
            offsetLocation = Vector3.Project(offsetLocation, selectedFaceNormal);
        }

        Matrix4x4 m = Matrix4x4.Translate(offsetLocation);
        int i = 0;
        while (i < verticesMatched.Count)
        {
            vertices[verticesMatched[i]] = m.MultiplyPoint3x4(movingVertices[verticesMatched[i]]);
            i++;
        }

        currentMesh.vertices = vertices;

        currentMesh.RecalculateBounds();
        currentMesh.RecalculateNormals();

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;
            currentMeshCollider.sharedMesh = currentMesh;

            for (int j = 0; j < editingUVs.Length; j++)
            {
                editingUVs[j] = new Vector2(0.1f, 0.1f);
            }

            currentMesh.uv = editingUVs;


            selectedObject.GetComponent<Renderer>().sharedMaterial = faceEditingMaterial;


            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            variablePointer.LockPointerLength(false);

            selectedObject.layer = 10;

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            currentState = ToolStates.SelectingFaces;
        }
    }

    //Highlights the face whilst the user points at it
    void HighlightFace()
    {

        Vector3 p1 = vertices[triangles[variablePointer.PointerHit.triangleIndex * 3 + 0]];
        Vector3 p2 = vertices[triangles[variablePointer.PointerHit.triangleIndex * 3 + 1]];
        Vector3 p3 = vertices[triangles[variablePointer.PointerHit.triangleIndex * 3 + 2]];

        if (p1 != triangleIndexOne || p2 != triangleIndexTwo || p3 != triangleIndexThree)
        {
            triangleIndexOne = p1;
            triangleIndexTwo = p2;
            triangleIndexThree = p3;

            FaceSelection(p1, p2, p3, new Vector2(0.6f, 0.3f));
        }
    }

    //selects the face they were pointing at
    void SelectFace()
    {
        //print(vertices);
        //print(variablePointer.PointerHit.triangleIndex);
        Vector3 p1 = vertices[triangles[variablePointer.PointerHit.triangleIndex * 3 + 0]];
        Vector3 p2 = vertices[triangles[variablePointer.PointerHit.triangleIndex * 3 + 1]];
        Vector3 p3 = vertices[triangles[variablePointer.PointerHit.triangleIndex * 3 + 2]];

        FaceSelection(p1, p2, p3, new Vector2(0.6f, 0.6f));
       
    }

    void FaceSelection(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 UVLocation)
    {
        Vector3 side1 = p2 - p1;
        Vector3 side2 = p3 - p1;

        Vector3 selectedNormal = Vector3.Cross(side1, side2);

        selectedNormal /= selectedNormal.magnitude;

        selectedFaceNormal = selectedNormal;

        int triangleCount = triangles.Length / 3;
        int count = 0;

        Vector3 pt1 = Vector3.zero;
        Vector3 pt2 = Vector3.zero;
        Vector3 pt3 = Vector3.zero;
        Vector3 sidet1 = Vector3.zero;
        Vector3 sidet2 = Vector3.zero;

        Vector3 tempNormal;

        //loop through all triangles to see if any triangle normals match the selected normal.
        //grab the vertices of any triangles that match.
        for (int i = 0; i < triangleCount; i++)
        {

            pt1 = vertices[triangles[count + 0]];
            pt2 = vertices[triangles[count + 1]];
            pt3 = vertices[triangles[count + 2]];

            sidet1 = pt2 - pt1;
            sidet2 = pt3 - pt1;

            tempNormal = Vector3.Cross(sidet1, sidet2);

            tempNormal /= tempNormal.magnitude;

            if (tempNormal == selectedNormal)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!verticesMatched.Contains(triangles[count + j]))
                    {
                        verticesMatched.Add(triangles[count + j]);
                    }
                }

            }

            count += 3;
        }

        for (int i = 0; i < verticesMatched.Count; i++)
        {
            editingUVs[verticesMatched[i]] = UVLocation;
        }

        currentMesh.uv = editingUVs;

        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < verticesMatched.Count; j++)
            {
                if (vertices[i] == vertices[verticesMatched[j]] && !verticesMatched.Contains(i))
                {
                    verticesMatched.Add(i);
                }
            }
        }
    }

    void GetMeshData()
    {
        verticesMatched.Clear();
        vertices = new Vector3[currentMesh.vertices.Length];
        vertices = currentMesh.vertices;
        movingVertices = new Vector3[currentMesh.vertices.Length];
        movingVertices = currentMesh.vertices;
        normals = new Vector3[currentMesh.normals.Length];
        normals = currentMesh.normals;
        triangles = new int[currentMesh.triangles.Length];
        triangles = currentMesh.triangles;
    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
