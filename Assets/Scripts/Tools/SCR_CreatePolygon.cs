using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_CreatePolygon : MonoBehaviour, ITool
{

    private enum ToolStates { Shape, Height, Idling };
    private ToolStates currentState = ToolStates.Idling;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private string toolName;
    [SerializeField] private GameObject vertexHelperPrefab;
    [SerializeField] private GameObject lineRendererPrefab;
    [SerializeField] private float distanceToStartVertex = 0.1f;
    [SerializeField] private float distanceToStartVertexMax = 1f;
    [SerializeField] private float distanceToStartVertexMultiplier = 0.1f;
    [SerializeField] private float vertexHelperStartingScale;
    [SerializeField] private float vertexHelperMaxScale;
    [SerializeField] private float vertexHelperScaleMultiplier;
    [SerializeField] private bool bScaleBasedOnIndividualVertexHelper;
    [SerializeField] private Material startingVertexHelperMaterial;
    [SerializeField] private Color lineRendererColour = Color.white;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private bool bActivationButtonPressed;
    private IPointer variablePointer;

    private List<Vector3> vertexList = new List<Vector3>();
    private List<int> triangleList = new List<int>();
    private List<Vector2> uvList = new List<Vector2>();
    private List<Vector3> topVertices = new List<Vector3>();
    private List<GameObject> vertexHelperList = new List<GameObject>();

    private GameObject newGameObject;
    private Vector3 startLocation;
    private MeshFilter meshFilter;
    private Renderer meshRenderer;
    private SCR_ObjectData objectDataScript;
    private Mesh mesh;
    private float startYLocation;
    private float vertexHelperCurrentScale;
    [SerializeField] private float distanceToStartVertexSnap;
    private Transform headsetTransform;
    private Vector3 pointerLocation;
    private LineRenderer lineRendererHelper;
    private GameObject lineRendererObject;
    private GameObject currentVertexHelper;

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
            case ToolStates.Shape:
                Shape();
                break;
            case ToolStates.Height:
                Height();
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
            if (variablePointer.Active && variablePointer.ValidTargetPosition)
            {
                bActivationButtonPressed = false;

                if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
                {
                    startLocation = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(variablePointer.PointerPosition);
                }
                else
                {
                    startLocation = variablePointer.PointerPosition;
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

                vertexHelperList.Add(Instantiate(vertexHelperPrefab, startLocation, Quaternion.identity));
                vertexList.Add(vertexHelperList[0].transform.position);
                lineRendererObject = Instantiate(lineRendererPrefab, vertexHelperList[0].transform.position, Quaternion.identity);
                lineRendererHelper = lineRendererObject.GetComponent<LineRenderer>();
                lineRendererHelper.SetPosition(lineRendererHelper.positionCount - 1, vertexHelperList[0].transform.position);
                startYLocation = vertexList[0].y;

                lineRendererHelper.positionCount += 1;
                lineRendererHelper.startColor = lineRendererColour;
                lineRendererHelper.endColor = lineRendererColour;
                vertexHelperList.Add(Instantiate(vertexHelperPrefab, startLocation, Quaternion.identity));
                vertexHelperList[0].GetComponent<MeshRenderer>().sharedMaterial = startingVertexHelperMaterial;

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);

                mesh.Clear();

                currentState = ToolStates.Shape;
            }
        }
    }

    void Shape()
    {

        UpdateHelperScales();

        
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

            Vector3 difference = headsetTransform.position - vertexList[0];
            distanceToStartVertexSnap = difference.magnitude;

            distanceToStartVertexSnap = Mathf.Min(distanceToStartVertex * distanceToStartVertexSnap * distanceToStartVertexMultiplier, distanceToStartVertexMax);
            distanceToStartVertexSnap = Mathf.Max(distanceToStartVertexSnap, distanceToStartVertex);

            if (Vector3.Distance(startLocation, vertexList[0]) <= distanceToStartVertexSnap)
            {
                vertexHelperList[vertexHelperList.Count - 1].GetComponent<MeshRenderer>().enabled = false;
                vertexHelperList[vertexHelperList.Count - 1].transform.position = new Vector3(startLocation.x, startYLocation, startLocation.z);
                lineRendererHelper.SetPosition(lineRendererHelper.positionCount - 1, vertexHelperList[0].transform.position);
            }
            else
            {
                vertexHelperList[vertexHelperList.Count - 1].GetComponent<MeshRenderer>().enabled = true;
                vertexHelperList[vertexHelperList.Count - 1].transform.position = new Vector3(startLocation.x, startYLocation, startLocation.z);
                lineRendererHelper.SetPosition(lineRendererHelper.positionCount - 1, new Vector3(startLocation.x, startYLocation, startLocation.z));
            }

            if (bActivationButtonPressed)
            {
                bActivationButtonPressed = false;
                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.1f, ControllerHand.RightHand);

                if (Vector3.Distance(vertexHelperList[vertexHelperList.Count - 1].transform.position, vertexList[0]) <= distanceToStartVertexSnap)
                {

                    List<Vector3> vertexListReverse = new List<Vector3>();
                    List<Vector3> sideVertexList = new List<Vector3>();
                    List<Vector3> fullTopVertices = new List<Vector3>();
                    List<Vector3> tempListForReversing = new List<Vector3>();
                    topVertices.Clear();

                    for (int i = 0; i < vertexList.Count; i++)
                    {
                        topVertices.Add(vertexList[i]);
                        vertexListReverse.Add(vertexList[i]);
                        tempListForReversing.Add(vertexList[i]);
                    }

                    //used to check if the polygons were created clockwise or not
                    //it will reverese the order of the side vertices if it is anti-clockwise
                    Vector2[] newVertices2d = new Vector2[vertexListReverse.Count + 1];

                    for (int i = 0; i < vertexListReverse.Count; i++)
                    {
                        newVertices2d[i] = new Vector2(vertexListReverse[i].x, vertexListReverse[i].z);
                    }

                    //close the loop
                    newVertices2d[newVertices2d.Length - 1] = newVertices2d[0];

                    bool bPolygonsAreClockwise = PolygonIsClockwise(newVertices2d);

                    print(bPolygonsAreClockwise);

                    if (!bPolygonsAreClockwise)
                    {
                        tempListForReversing.Reverse();
                    }

                    for (int i = 0; i < tempListForReversing.Count; i++)
                    {
                        sideVertexList.Add(tempListForReversing[i]);
                    }
                    sideVertexList.Add(tempListForReversing[0]);

                    tempListForReversing.Clear();

                    //ensures the bottom polygon faces outward by reversing the triangles but not the vertices.
                    //vertexListReverse.Reverse();

                    TriangulateVertices(vertexListReverse, false);

                    int count = topVertices.Count;
                    for (int i = 0; i < count; i++)
                    {
                        vertexList.Add(topVertices[i]);
                        tempListForReversing.Add(topVertices[i]);
                    }

                    if (!bPolygonsAreClockwise)
                    {
                        tempListForReversing.Reverse();
                    }

                    for (int i = 0; i < tempListForReversing.Count; i++)
                    {
                        sideVertexList.Add(tempListForReversing[i]);
                    }
                    sideVertexList.Add(tempListForReversing[0]);



                    for (int i = 0; i < sideVertexList.Count; i++)
                    {
                        vertexList.Add(sideVertexList[i]);
                    }

                    TriangulateVertices(topVertices, true);

                    CreateSides(sideVertexList);

                    foreach (var item in vertexHelperList)
                    {
                        Destroy(item);
                    }
                    vertexHelperList.Clear();

                    Destroy(lineRendererHelper);

                    variablePointer.LockPointerLength(true);
                    currentState = ToolStates.Height;
                }
                else
                {
                    vertexHelperList[vertexHelperList.Count - 1].transform.position = new Vector3(startLocation.x, startYLocation, startLocation.z);
                    vertexHelperList.Add(Instantiate(vertexHelperPrefab, new Vector3(startLocation.x, startYLocation, startLocation.z), Quaternion.identity));
                    vertexList.Add(vertexHelperList[vertexHelperList.Count - 1].transform.position);
                    lineRendererHelper.positionCount += 1;

                }
            }
        }
    }

    void Height()
    {

        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            pointerLocation = Snap(variablePointer.PointerPosition);
        }
        else
        {
            pointerLocation = variablePointer.PointerPosition;
        }

        if (pointerLocation.y > startLocation.y)
        {



            int i = topVertices.Count;
            while (i < topVertices.Count * 2)
            {
                vertexList[i] = new Vector3(vertexList[i].x, pointerLocation.y, vertexList[i].z);
                i++;
            }

            int j = vertexList.Count - (topVertices.Count + 1);
            while (j < vertexList.Count)
            {
                vertexList[j] = new Vector3(vertexList[j].x, pointerLocation.y, vertexList[j].z);
                j++;
            }

            Vector3[] vertices = new Vector3[vertexList.Count];
            for (int k = 0; k < vertices.Length; k++)
            {
                vertices[k] = newGameObject.transform.InverseTransformPoint(vertexList[k]);
            }

            mesh.vertices = vertices;
            mesh.uv = CalculateSideUV(vertexList);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            if (bActivationButtonPressed)
            {
                bActivationButtonPressed = false;

                AddCollisionMesh();

                SCR_SaveSystem.instance.AddGeometry(newGameObject);

                meshRenderer.sharedMaterial = SCR_ToolMenuRadial.instance.defaultCreationMaterial;

                vertexList.Clear();
                triangleList.Clear();
                uvList.Clear();
                topVertices.Clear();
                vertexHelperList.Clear();

                bBusy = false;

                variablePointer.SetPointerColourDefault();

                SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.15f, ControllerHand.RightHand);
                //VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                variablePointer.LockPointerLength(false);
                currentState = ToolStates.Idling;
            }

        }
    }

    void TriangulateVertices(List<Vector3> vertices3, bool topVertices)
    {
        mesh.Clear();

        Vector2[] newVertices2d = new Vector2[vertices3.Count];

        for (int i = 0; i < vertices3.Count; i++)
        {
            newVertices2d[i] = new Vector2(vertices3[i].x, vertices3[i].z);
        }

        SCR_Triangulator tr = new SCR_Triangulator(newVertices2d);
        int[] indices = tr.Triangulate(topVertices);

        if (topVertices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                triangleList.Add(indices[i] + vertices3.Count);
            }
        }
        else
        {
            for (int i = 0; i < indices.Length; i++)
            {
                triangleList.Add(indices[i]);
            }
        }


        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertexList.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = newGameObject.transform.InverseTransformPoint(vertexList[i]);
        }

        Vector2[] uvs = new Vector2[vertexList.Count];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertexList[i].x, vertexList[i].z);
        }

        // Create the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangleList.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void CreateSides(List<Vector3> verticesForSides)
    {
        //bottom triangles
        int countBottom = (int)(verticesForSides.Count - 2) / 2;

        int[] trianglesBottom = new int[(countBottom) * 3];
        int triangleCountBottom = 0;
        for (int i = 0; i < countBottom; i++)
        {
            trianglesBottom[i + 0 + triangleCountBottom] = i + (verticesForSides.Count - 2);
            trianglesBottom[i + 1 + triangleCountBottom] = i + (verticesForSides.Count - 2) + 1;
            trianglesBottom[i + 2 + triangleCountBottom] = i + (verticesForSides.Count - 2) + 2 + countBottom;
            triangleCountBottom += 2;
        }

        for (int i = 0; i < trianglesBottom.Length; i++)
        {
            triangleList.Add(trianglesBottom[i]);
        }

        //top triangles
        int countTop = (int)(verticesForSides.Count - 2) / 2;
        int[] trianglesTop = new int[(countTop) * 3];
        int triangleCountTop = 0;
        for (int i = 0; i < countTop; i++)
        {
            trianglesTop[i + 1 + triangleCountTop] = i + (verticesForSides.Count - 1) + countTop;
            trianglesTop[i + 0 + triangleCountTop] = i + (verticesForSides.Count - 1) + countTop + 1;
            trianglesTop[i + 2 + triangleCountTop] = i + (verticesForSides.Count - 2);
            triangleCountTop += 2;
        }

        for (int i = 0; i < trianglesTop.Length; i++)
        {
            triangleList.Add(trianglesTop[i]);
        }

        // Create the mesh
        mesh.triangles = triangleList.ToArray();
        mesh.normals = vertexList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    Vector2[] CalculateSideUV(List<Vector3> newVertexList)
    {
        int startPoint = newVertexList.Count - ((newVertexList.Count + 2) / 2);

        Vector2[] UV = new Vector2[(newVertexList.Count + 2) / 2];
        float previousLength = 0f;
        for (int i = 0; i < UV.Length / 2; i++)
        {
            if (i == 0)
            {
                UV[i] = new Vector2(0, 0);
            }
            else
            {
                UV[i] = new Vector2(previousLength + Vector3.Distance(newVertexList[startPoint + i - 1], newVertexList[i + startPoint]), 0);
                previousLength += Vector3.Distance(newVertexList[startPoint + i - 1], newVertexList[i + startPoint]);
            }

        }

        int count = UV.Length / 2;
        float heightUV = Vector3.Distance(newVertexList[startPoint + count], newVertexList[startPoint]);
        float previousLengthTop = 0f;
        for (int i = count; i < UV.Length; i++)
        {
            if (i == count)
            {
                UV[i] = new Vector2(0, heightUV);
            }
            else
            {
                UV[i] = new Vector2(previousLengthTop + Vector3.Distance(newVertexList[startPoint + i - 1], newVertexList[i + startPoint]), heightUV);
                previousLengthTop += Vector3.Distance(newVertexList[startPoint + i - 1], newVertexList[i + startPoint]);
            }

        }

        Vector2[] allUVs = new Vector2[vertexList.Count];

        for (int i = 0; i < vertexList.Count / 2; i++)
        {
            allUVs[i] = mesh.uv[i];
        }

        for (int i = startPoint; i < vertexList.Count; i++)
        {
            allUVs[i] = UV[i - startPoint];
        }

        return allUVs;
    }

    //taken from Cyclopse (2010) https://answers.unity.com/questions/15978/determine-whether-or-not-model-is-inside-out.html
    bool PolygonIsClockwise(params Vector2[] points)
    {
        int l = points.Length;

        float sum = 0f;

        for (int i = 0; i < l; i++)
        {
            int n = i + 1 >= l - 1 ? 0 : i + 1;

            float x = points[n].x - points[i].x;
            float y = points[n].y + points[i].y;
            sum += (x * y);
        }

        return (sum < 0) ? false : true;
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
            Vector3 difference = headsetTransform.position - newGameObject.transform.position;
            float distance = difference.magnitude;

            vertexHelperCurrentScale = Mathf.Min(vertexHelperStartingScale * distance * vertexHelperScaleMultiplier, vertexHelperMaxScale);
            vertexHelperCurrentScale = Mathf.Max(vertexHelperCurrentScale, vertexHelperStartingScale);

            foreach (var item in vertexHelperList)
            {
                item.transform.localScale = new Vector3(vertexHelperCurrentScale, vertexHelperCurrentScale, vertexHelperCurrentScale);
            }
        }

    }

    //Add collision mesh once polygon object is completed.
    void AddCollisionMesh()
    {
        (newGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
        MeshCollider meshCollider = newGameObject.GetComponent<MeshCollider>();
        //meshCollider.convex = true;
    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
