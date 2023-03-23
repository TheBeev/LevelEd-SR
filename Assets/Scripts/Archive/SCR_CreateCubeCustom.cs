using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_CreateCubeCustom : MonoBehaviour, ITool {

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private Material mat;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private GameObject newGameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshCollider meshCollider;

    private Vector3 startLocation;
    private Vector3 endLocation;

    const float c_DrawDistance = 0.075f;

    private IPointer antenna;
    private IPointer distance;

    enum CubeCreationStates
    {
        Started,
        Base,
        Height,
    }

    CubeCreationStates state = CubeCreationStates.Started;

    private void OnEnable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started += DoActivationButtonPressed;
        //controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started -= DoActivationButtonPressed;
        //controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        switch (state)
        {
            case CubeCreationStates.Started:
                StartCube();
                break;
            case CubeCreationStates.Base:
                state = CubeCreationStates.Height;
                break;
            case CubeCreationStates.Height:
                FinishCube();
                break;
            default:
                break;
        }
    }

    void DoActivationButtonReleased(InputAction.CallbackContext context)
    {

    }

    // Use this for initialization
    void Start()
    {
        GameObject antennaObject = GameObject.FindGameObjectWithTag("RightAntenna");

        if (antennaObject)
        {
            antenna = (IPointer)antennaObject.GetComponent(typeof(IPointer));
        }


        GameObject distanceObject = GameObject.FindGameObjectWithTag("RightDistance");
        if (distanceObject)
        {
            distance = (IPointer)distanceObject.GetComponent(typeof(IPointer));
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case CubeCreationStates.Started:
                break;
            case CubeCreationStates.Base:
                EditingCube();
                break;
            case CubeCreationStates.Height:
                break;
            default:
                break;
        }
    }

    void StartCube()
    {
        newGameObject = new GameObject("Cube");

        if (antenna.Active && antenna.ValidTargetPosition)
        {
            startLocation = antenna.PointerPosition;
        }
        else if (distance.Active && distance.ValidTargetPosition)
        {
            startLocation = distance.PointerPosition;
        }
        else
        {
            return;
        }

        bBusy = true;

        newGameObject.transform.localPosition = startLocation;

        //m_NewGameObject.transform.position = new Vector3(0, 0, 0);
        newGameObject.transform.localScale = Vector3.one;
        meshFilter = newGameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        meshRenderer = newGameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        mesh = new Mesh();

        meshRenderer.sharedMaterial = mat;
        Color c = meshRenderer.sharedMaterial.color;
        c.a = 0.5f;
        meshRenderer.sharedMaterial.color = c;

        state = CubeCreationStates.Base;

    }

    void EditingCube()
    {
        if (antenna.Active && antenna.ValidTargetPosition)
        {
            endLocation = antenna.PointerPosition;
        }
        else if (distance.Active && distance.ValidTargetPosition)
        {
            endLocation = distance.PointerPosition;
        }
        else
        {
            return;
        }

        print("End Location: " + endLocation);

        //repositions the mesh at local o,o,o rather than off in the distance
        endLocation.x += (0 - startLocation.x);
        endLocation.y += (0 - startLocation.y);
        endLocation.z += (0 - startLocation.z);

        UpdateCube(Vector3.zero, endLocation.x, endLocation.y, endLocation.z);

    }

    void FinishCube()
    {
        SetMaterialOpaque();
        AddCollisionMesh();
        bBusy = false;
        state = CubeCreationStates.Started;
    }

    public void UpdateCube(Vector3 startLocation, float width, float height, float depth)
    {

        mesh.vertices = new Vector3[]
        {
            //front
            new Vector3(startLocation.x, startLocation.y, startLocation.z), //left top front 0
            new Vector3(width, startLocation.y, startLocation.z), //right top front 1
            new Vector3(startLocation.x, height, startLocation.z), //left bottom front 2
            new Vector3(width, height, startLocation.z), //right bottom front 3

            //right
            new Vector3(startLocation.x, startLocation.y, startLocation.z),
            new Vector3(startLocation.x, startLocation.y, depth),
            new Vector3(startLocation.x, height, startLocation.z),
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
            new Vector3(width, startLocation.y, depth),
            new Vector3(startLocation.x, height, depth),
            new Vector3(width, height, depth)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),

            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),

            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),

            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),

            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),

            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
        };

        mesh.triangles = new int[]
        {
            //front
            2,1,0,
            1,2,3,
            
            //right
            4,5,6,
            5,7,6,

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
            20,21,23,
            23,22,20
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
        Color c = meshRenderer.sharedMaterial.color;
        c.a = 1f;
        meshRenderer.sharedMaterial.color = c;
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
