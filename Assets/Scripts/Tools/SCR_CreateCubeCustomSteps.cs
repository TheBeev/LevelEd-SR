using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_CreateCubeCustomSteps : MonoBehaviour, ITool
{ 
    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;

    [SerializeField] private string toolName;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bFirstTime = true;

    private GameObject newGameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshCollider meshCollider;
    private SCR_ObjectData objectDataScript;

    private Vector3 startLocation;
    private Vector3 nonAdjustedBaseEndLocation;
    private Vector3 baseEndLocation;
    private Vector3 nonAdjustedHeightEndLocation;
    private Vector3 heightEndLocation;

    private bool bActivationButtonPressed = false;
    //private bool bActivationButtonReleased = true;

    const float c_DrawDistance = 0.075f;

    private IPointer antenna;
    private IPointer distance;
    private IPointer variablePointer;

    private SCR_ToolOptions toolOptions;

    enum CubeCreationStates
    {
        Started,
        Base,
        Height,
        Finished
    }

    CubeCreationStates state = CubeCreationStates.Started;

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
        
        //controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);

    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);
        //controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
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

    /*
    void DoActivationButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        bActivationButtonReleased = true;
    }*/

    // Use this for initialization
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
        switch (state)
        {
            case CubeCreationStates.Started:
                StartCube();
                break;
            case CubeCreationStates.Base:
                EditingBaseCube();
                break;
            case CubeCreationStates.Height:
                EditingHeightCube();
                break;
            case CubeCreationStates.Finished:
                FinishCube();
                break;
            default:
                break;
        }
    }

    void StartCube()
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

            SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
            //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
            bActivationButtonPressed = false;
            state = CubeCreationStates.Base;

        }

    }

    void EditingBaseCube()
    {

        if (variablePointer.Active && variablePointer.ValidTargetPosition)
        {
            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                nonAdjustedBaseEndLocation = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(variablePointer.PointerPosition);
            }
            else
            {
                nonAdjustedBaseEndLocation = variablePointer.PointerPosition;
            }
        }
        else
        {
            return;
        }

        baseEndLocation = nonAdjustedBaseEndLocation;

        //repositions the mesh at local o,o,o rather than off in the distance
        baseEndLocation.x += (0 - startLocation.x);
        baseEndLocation.y += (0 - startLocation.y);
        baseEndLocation.z += (0 - startLocation.z);

        UpdateCube(Vector3.zero, baseEndLocation.x, 0.0001f, baseEndLocation.z);

        if (bActivationButtonPressed)
        {
            if (nonAdjustedBaseEndLocation.x != startLocation.x && nonAdjustedBaseEndLocation.z != startLocation.z)
            {
                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                variablePointer.LockPointerLength(true);
                bActivationButtonPressed = false;
                state = CubeCreationStates.Height;
            }
            else
            {
                bActivationButtonPressed = false;
            }
            
        }

    }

    void EditingHeightCube()
    {

        if (variablePointer.Active && variablePointer.ValidTargetPosition)
        {
            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                nonAdjustedHeightEndLocation = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(variablePointer.PointerPosition);
            }
            else
            {
                nonAdjustedHeightEndLocation = variablePointer.PointerPosition;
            }
            
        }
        else
        {
            return;
        }

        heightEndLocation = nonAdjustedHeightEndLocation;

        //repositions the mesh at local o,o,o rather than off in the distance
        heightEndLocation.x += (0 - startLocation.x);
        heightEndLocation.y += (0 - startLocation.y);
        heightEndLocation.z += (0 - startLocation.z);

        UpdateCube(Vector3.zero, baseEndLocation.x, heightEndLocation.y, baseEndLocation.z);

        if (bActivationButtonPressed)
        {
            if (nonAdjustedHeightEndLocation.y != startLocation.y)
            {
                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bActivationButtonPressed = false;
                bBusy = false;
                state = CubeCreationStates.Finished;
            }
            else
            {
                bActivationButtonPressed = false;
            }
            
        }

    }

    void FinishCube()
    {
        SetMaterialOpaque();
        AddCollisionMesh();

        variablePointer.SetPointerColourDefault();
        variablePointer.LockPointerLength(false);

        state = CubeCreationStates.Started;
        SCR_SaveSystem.instance.AddGeometry(newGameObject);
    }

    public void UpdateCube(Vector3 startLocation, float width, float height, float depth)
    {

        mesh.vertices = new Vector3[]
        {
            //front
            new Vector3(startLocation.x, startLocation.y, startLocation.z), //left top front 0
            new Vector3(startLocation.x, height, startLocation.z), //left bottom front 2
            new Vector3(width, startLocation.y, startLocation.z), //right top front 1
            new Vector3(width, height, startLocation.z), //right bottom front 3

            //right
            new Vector3(startLocation.x, startLocation.y, startLocation.z),
            new Vector3(startLocation.x, height, startLocation.z),
            new Vector3(startLocation.x, startLocation.y, depth),
            new Vector3(startLocation.x, height, depth), 

            //bottom
            new Vector3(startLocation.x, startLocation.y, startLocation.z),
            new Vector3(width, startLocation.y, startLocation.z),
            new Vector3(startLocation.x, startLocation.y, depth),
            new Vector3(width, startLocation.y, depth), 

            //top
            new Vector3(startLocation.x, height, startLocation.z),
            new Vector3(width, height, startLocation.z),
            new Vector3(startLocation.x, height, depth),
            new Vector3(width, height, depth),

            //left
            new Vector3(width, startLocation.y, startLocation.z),
            new Vector3(width, height, startLocation.z),
            new Vector3(width, startLocation.y, depth),
            new Vector3(width, height, depth),

            //back
            new Vector3(startLocation.x, startLocation.y, depth),
            new Vector3(startLocation.x, height, depth),
            new Vector3(width, startLocation.y, depth),
            new Vector3(width, height, depth)
        };

        mesh.uv = new Vector2[]
        {
            //front
            new Vector2(0,0),
            new Vector2(0,Vector3.Distance(mesh.vertices[1], mesh.vertices[0])),
            new Vector2(Vector3.Distance(mesh.vertices[2], mesh.vertices[0]),0),
            new Vector2(Vector3.Distance(mesh.vertices[3], mesh.vertices[1]), Vector3.Distance(mesh.vertices[3], mesh.vertices[2])),
                       
            
            //right
            new Vector2(0,0),
            new Vector2(0,Vector3.Distance(mesh.vertices[5], mesh.vertices[4])),
            new Vector2(Vector3.Distance(mesh.vertices[6], mesh.vertices[4]),0),
            new Vector2(Vector3.Distance(mesh.vertices[7], mesh.vertices[5]), Vector3.Distance(mesh.vertices[7], mesh.vertices[6])),

            //bottom
            new Vector2(0,0),
            new Vector2(0,Vector3.Distance(mesh.vertices[9], mesh.vertices[8])),
            new Vector2(Vector3.Distance(mesh.vertices[10], mesh.vertices[8]),0),
            new Vector2(Vector3.Distance(mesh.vertices[11], mesh.vertices[9]), Vector3.Distance(mesh.vertices[11], mesh.vertices[10])),

            //top
            new Vector2(0,0),
            new Vector2(0,Vector3.Distance(mesh.vertices[13], mesh.vertices[12])),
            new Vector2(Vector3.Distance(mesh.vertices[14], mesh.vertices[12]),0),
            new Vector2(Vector3.Distance(mesh.vertices[15], mesh.vertices[13]), Vector3.Distance(mesh.vertices[15], mesh.vertices[14])),

            //left
            new Vector2(0,0),
            new Vector2(0,Vector3.Distance(mesh.vertices[17], mesh.vertices[16])),
            new Vector2(Vector3.Distance(mesh.vertices[18], mesh.vertices[16]),0),
            new Vector2(Vector3.Distance(mesh.vertices[19], mesh.vertices[17]), Vector3.Distance(mesh.vertices[19], mesh.vertices[18])),

            //back
            new Vector2(0,0),
            new Vector2(0,Vector3.Distance(mesh.vertices[21], mesh.vertices[20])),
            new Vector2(Vector3.Distance(mesh.vertices[22], mesh.vertices[20]),0),
            new Vector2(Vector3.Distance(mesh.vertices[23], mesh.vertices[21]), Vector3.Distance(mesh.vertices[23], mesh.vertices[22])),
        };

        mesh.triangles = new int[]
        {
            //front
            0,1,2,
            2,1,3,
            
            //right
            6,5,4,
            6,7,5,

            //bottom
            8,9,11,
            10,8,11,

            //top
            15,13,12,
            12,14,15,

            //left
            16,17,19,
            19,18,16,

            //back
            22,21,20,
            21,22,23
        };

        //checks to see if the mesh has become inverted
        float negativeXCheck = startLocation.x - width;
        float negativeYCheck = startLocation.y - height;
        float negativeZCheck = startLocation.z - depth;

        if (Mathf.Sign(negativeXCheck) == -1)
        {
            mesh = ReverseNormals(mesh);
        }

        if (Mathf.Sign(negativeYCheck) == -1)
        {
            mesh = ReverseNormals(mesh);
        }

        if (Mathf.Sign(negativeZCheck) == 1)
        {
            mesh = ReverseNormals(mesh);
        }

        meshFilter.mesh = mesh;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

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

    //Adapted from Ante (2012) takes the normals of the mesh and reverses them. Stops mesh going inverted.
    //http://wiki.unity3d.com/index.php/ReverseNormals
    private Mesh ReverseNormals(Mesh mesh)
    {
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];

        mesh.normals = normals;

        for (int m = 0; m < mesh.subMeshCount; m++)
        {
            int[] triangles = mesh.GetTriangles(m);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i + 0];
                triangles[i + 0] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.SetTriangles(triangles, m);
        }

        return mesh;
    }
}
