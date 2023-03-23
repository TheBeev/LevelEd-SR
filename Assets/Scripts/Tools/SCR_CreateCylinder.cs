using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_CreateCylinder : MonoBehaviour, ITool {

    private enum ToolStates { Started, Radius, Height, Sides, Idling };
    private ToolStates currentState = ToolStates.Idling;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;

    [Header("Testing Values")]
    [SerializeField] private int sides = 18;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;

    private GameObject newGameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshCollider meshCollider;
    private SCR_ObjectData objectDataScript;

    private Vector3 startLocation;
    private Vector3 dimensionsEndLocation;
    private float cylinderRadius;
    private float cylinderHeight;

    private bool bFirstTime = true;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        SCR_ToolOptions.instance.DeactivateOptions();
        SCR_GridSnappingOption.instance.ActivateOption();
        SCR_SurfaceSnappingOption.instance.ActivateOption();

        if (!bFirstTime)
        {
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
        }
    }

    private void OnDisable()
    {
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
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (currentState)
        {
            case ToolStates.Started:
                StartCylinder();
                break;
            case ToolStates.Radius:
                EditRadius();
                break;
            case ToolStates.Height:
                EditHeight();
                break;
            case ToolStates.Sides:
                AdjustSides();
                break;
            case ToolStates.Idling:
                Idling();
                break;
            default:
                break;
        }
    }

    void Idling()
    {
        if (bActivationButtonPressed)
        {
            currentState = ToolStates.Started;
        }
    }

    void StartCylinder()
    {
        if (bActivationButtonPressed)
        {
            if (variablePointer.Active && variablePointer.ValidTargetPosition)
            {
                if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
                {
                    startLocation = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(variablePointer.PointerPosition);
                }
                else
                {
                    startLocation = variablePointer.PointerPosition;
                }

            }
            else
            {
                return;
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(true);
            }

            bBusy = true;

            variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

            SCR_SaveSystem.instance.ObjectIDNumber++;

            newGameObject = new GameObject("Geometry" + SCR_SaveSystem.instance.ObjectIDNumber);
            newGameObject.layer = 8;
            newGameObject.transform.localPosition = startLocation;

            newGameObject.transform.localScale = Vector3.one;
            meshFilter = newGameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            meshRenderer = newGameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            objectDataScript = newGameObject.AddComponent(typeof(SCR_ObjectData)) as SCR_ObjectData;
            objectDataScript.objectID = SCR_SaveSystem.instance.ObjectIDNumber;

            mesh = new Mesh();

            meshRenderer.material = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
            meshRenderer.material.mainTexture = SCR_ToolMenuRadial.instance.defaultCreationMaterial.mainTexture;

            meshFilter.mesh = mesh;

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);

            mesh.Clear();

            bActivationButtonPressed = false;
            currentState = ToolStates.Radius;

        }
    }

    void EditRadius()
    {
        if (variablePointer.Active && variablePointer.ValidTargetPosition)
        {
            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                dimensionsEndLocation = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(variablePointer.PointerPosition);
            }
            else
            {
                dimensionsEndLocation = variablePointer.PointerPosition;
            }

        }
        else
        {
            return;
        }

        cylinderRadius = Vector3.Distance(startLocation, new Vector3(dimensionsEndLocation.x, startLocation.y, dimensionsEndLocation.z));

        UpdateCylinder(startLocation, cylinderRadius, 0.0001f, sides);

        if (bActivationButtonPressed)
        {
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            if (cylinderRadius > 0)
            {
                variablePointer.LockPointerLength(true);
                currentState = ToolStates.Height;
                bActivationButtonPressed = false;
            }
            else
            {
                bActivationButtonPressed = false;
            }
            
        }

    }

    void EditHeight()
    {
        if (variablePointer.Active && variablePointer.ValidTargetPosition)
        {
            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                dimensionsEndLocation = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(variablePointer.PointerPosition);
            }
            else
            {
                dimensionsEndLocation = variablePointer.PointerPosition;
            }

        }
        else
        {
            return;
        }

        cylinderHeight = Vector3.Distance(startLocation, new Vector3(startLocation.x, dimensionsEndLocation.y, startLocation.z));

        UpdateCylinder(startLocation, cylinderRadius, cylinderHeight, sides);

        if (bActivationButtonPressed)
        {
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);

            if (cylinderHeight > 0)
            {
                currentState = ToolStates.Sides;
                bActivationButtonPressed = false;
            }
            else
            {
                bActivationButtonPressed = false;
            }
            
        }

    }

    void AdjustSides()
    {
        FinishedCylinder();
        currentState = ToolStates.Idling;
        bBusy = false;
    }

    void FinishedCylinder()
    {
        SetMaterialOpaque();
        AddCollisionMesh();

        if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
        {
            variablePointer.LockPointerLength(false);
        }

        variablePointer.SetPointerColourDefault();
        variablePointer.LockPointerLength(false);

        SCR_SaveSystem.instance.AddGeometry(newGameObject);

        bActivationButtonPressed = false;   
    }

    //Adapted Bérenger 2014 from http://wiki.unity3d.com/index.php/ProceduralPrimitives
    void UpdateCylinder(Vector3 startLocation, float newRadius, float newHeight, int newSides)
    {
        float height = newHeight;
        int nbSides = newSides;
        int nbHeightSeg = 1; // Not implemented yet
        float bottomRadius = newRadius;
        float topRadius = newRadius;
        int nbVerticesCap = nbSides + 1;

        #region Vertices

        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * nbHeightSeg * 2 + 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;

        // Bottom cap
        vertices[vert++] = new Vector3(0f, 0f, 0f);
        while (vert <= nbSides)
        {
            float rad = (float)vert / nbSides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
            vert++;
        }

        // Top cap
        vertices[vert++] = new Vector3(0f, height, 0f);
        while (vert <= nbSides * 2 + 1)
        {
            float rad = (float)(vert - nbSides - 1) / nbSides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
            vert++;
        }

        // Sides
        int v = 0;
        while (vert <= vertices.Length - 4)
        {
            float rad = (float)v / nbSides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
            vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
            vert += 2;
            v++;
        }
        vertices[vert] = vertices[nbSides * 2 + 2];
        vertices[vert + 1] = vertices[nbSides * 2 + 3];
        #endregion

        #region Normales

        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;

        // Bottom cap
        while (vert <= nbSides)
        {
            normales[vert++] = Vector3.down;
        }

        // Top cap
        while (vert <= nbSides * 2 + 1)
        {
            normales[vert++] = Vector3.up;
        }

        // Sides
        v = 0;
        while (vert <= vertices.Length - 4)
        {
            float rad = (float)v / nbSides * _2pi;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            normales[vert] = new Vector3(cos, 0f, sin);
            normales[vert + 1] = normales[vert];

            vert += 2;
            v++;
        }
        normales[vert] = normales[nbSides * 2 + 2];
        normales[vert + 1] = normales[nbSides * 2 + 3];
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        // Bottom cap
        int u = 0;
        uvs[u++] = new Vector2(0.5f, 0.5f);
        while (u <= nbSides)
        {
            float rad = (float)u / nbSides * _2pi;
            uvs[u] = new Vector2(Mathf.Cos(rad) * bottomRadius + .5f, Mathf.Sin(rad) * bottomRadius + .5f);
            u++;
        }

        // Top cap
        uvs[u++] = new Vector2(0.5f, 0.5f);
        while (u <= nbSides * 2 + 1)
        {
            float rad = (float)u / nbSides * _2pi;
            uvs[u] = new Vector2(Mathf.Cos(rad) * topRadius + .5f, Mathf.Sin(rad) * topRadius + .5f);
            u++;
        }

        // Sides
        int u_sides = 0;
        float cylinderLength = (2 * Mathf.PI * topRadius);
        float sideLength = cylinderLength / nbSides;
        float t;
        while (u <= uvs.Length - 4)
        {
            //float t = (float)u_sides / nbSides;
            t = 0 + (sideLength * (u_sides));
            uvs[u] = new Vector3(t, height);
            uvs[u + 1] = new Vector3(t, 0f);
            u += 2;
            u_sides++;
        }
        t = 0 + (sideLength * (u_sides));
        uvs[u] = new Vector2(t, height);
        uvs[u + 1] = new Vector2(t, 0f);
        #endregion

        #region Triangles
        int nbTriangles = nbSides + nbSides + nbSides * 2;
        int[] triangles = new int[nbTriangles * 3 + 3];

        // Bottom cap
        int tri = 0;
        int i = 0;
        while (tri < nbSides - 1)
        {
            triangles[i] = 0;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = tri + 2;
            tri++;
            i += 3;
        }
        triangles[i] = 0;
        triangles[i + 1] = tri + 1;
        triangles[i + 2] = 1;
        tri++;
        i += 3;

        // Top cap
        //tri++;
        while (tri < nbSides * 2)
        {
            triangles[i] = tri + 2;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
        }

        triangles[i] = nbVerticesCap + 1;
        triangles[i + 1] = tri + 1;
        triangles[i + 2] = nbVerticesCap;
        tri++;
        i += 3;
        tri++;

        // Sides
        while (tri <= nbTriangles)
        {
            triangles[i] = tri + 2;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = tri + 0;
            tri++;
            i += 3;

            triangles[i] = tri + 1;
            triangles[i + 1] = tri + 2;
            triangles[i + 2] = tri + 0;
            tri++;
            i += 3;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }

    void SetMaterialOpaque()
    {
        meshRenderer.sharedMaterial = SCR_ToolMenuRadial.instance.defaultCreationMaterial;
    }

    //Add collision mesh once cube is completed.
    void AddCollisionMesh()
    {
        (newGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
        meshCollider = newGameObject.GetComponent<MeshCollider>();
        meshCollider.convex = true;
    }
}
