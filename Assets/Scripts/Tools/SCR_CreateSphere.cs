using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_CreateSphere : MonoBehaviour, ITool {

    private enum ToolStates { Started, Radius, Idling };
    private ToolStates currentState = ToolStates.Idling;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;

    //[Header("Testing Values")]
    //[SerializeField] private float testRadius = 1f;

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
    //private MeshCollider meshCollider; //Might need to convert to mesh collider when editing polys.
    private SphereCollider sphereCollider;
    private SCR_ObjectData objectDataScript;
    private float radius;

    private Vector3 startLocation;
    private Vector3 dimensionsEndLocation;
    private float cylinderRadius;

    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

            SCR_ToolOptions.instance.DeactivateOptions();
            SCR_GridSnappingOption.instance.ActivateOption();
            SCR_SurfaceSnappingOption.instance.ActivateOption();

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
            OnEnable();
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

            bBusy = true;

            variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

            SCR_SaveSystem.instance.ObjectIDNumber++;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(true);
            }

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

        UpdateSphere(cylinderRadius);

        if (bActivationButtonPressed)
        {
            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);

            if (cylinderRadius > 0)
            {
                FinishedSphere();
                currentState = ToolStates.Idling;

                variablePointer.SetPointerColourDefault();

                bBusy = false;
                bActivationButtonPressed = false;
            }
            else
            {
                bActivationButtonPressed = false;
            }
            
        }

    }

    void FinishedSphere()
    {
        SetMaterialOpaque();
        AddCollisionMesh();

        if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
        {
            variablePointer.LockPointerLength(false);
        }

        SCR_SaveSystem.instance.AddGeometry(newGameObject);

        bActivationButtonPressed = false;   
    }

    //Adapted from Bérenger (2014) http://wiki.unity3d.com/index.php/ProceduralPrimitives
    void UpdateSphere(float newRadius)
    {

        radius = newRadius;
        // Longitude |||
        int nbLong = 16;
        // Latitude ---
        int nbLat = 10;

        #region Vertices
        Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        for (int lat = 0; lat < nbLat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius;
        #endregion

        #region Normals		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < nbLat - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) - 1;
            triangles[i++] = vertices.Length - (lon + 1) - 1;
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

    //This will need editing if we ever get to adding poly editing (extrusion, moving, etc.)
    //Add collision mesh once cube is completed.
    void AddCollisionMesh()
    {
        //(newGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
        //meshCollider = newGameObject.GetComponent<MeshCollider>();
        sphereCollider = newGameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = radius;
        //meshCollider.convex = true;
    }
}
