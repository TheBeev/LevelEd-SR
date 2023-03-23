using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SVector3
{
    public float x = 0;
    public float y = 0;
    public float z = 0;

    public SVector3(float posX, float posY, float posZ)
    {
        x = posX;
        y = posY;
        z = posZ;
    }
}

[System.Serializable]
public class SColor
{
    public float r = 0;
    public float g = 0;
    public float b = 0;
    public float a = 0;

    public SColor(float red, float green, float blue, float alpha)
    {
        r = red;
        g = green;
        b = blue;
        a = alpha;
    }
}

[System.Serializable]
public class ParentScriptData
{

}

[System.Serializable]
public class GeometryGameObjectSave
{
    public SVector3 transform;
    public SVector3 rotation;
    public SVector3 scale;
    public SColor color;
    public string materialName;
    public string name;
    public int objectID;
    public int parentID;
    public string parentName;
    public byte[] meshData;
}

[System.Serializable]
public class ParentGameObjectSave
{
    public SVector3 transform;
    public SVector3 rotation;
    public SVector3 scale;
    public string name;
    public int ID;
}

[System.Serializable]
public class PrefabGameObjectSave
{
    public SVector3 transform;
    public SVector3 rotation;
    public SVector3 scale;
    public string name;
    public int prefabID;
}

[System.Serializable]
public class ScriptGameObjectSave
{
    public SVector3 transform;
    public SVector3 rotation;
    public SVector3 scale;
    public string name;
    public int prefabID;
    public ParentScriptData scriptData;
}

[System.Serializable]
public class SaveData
{
    public int currentGroupNumber;

    public int currentObjectNumber;

    public int currentScriptNumber;

    public SVector3 controllerMidPointLocation;
    public SVector3 controllerMidPointRotation;

    //physical mesh objects
    public List<GeometryGameObjectSave> geometryGameObjectsToSave = new List<GeometryGameObjectSave>();

    //empty game objects used as parents
    public List<ParentGameObjectSave> parentGameObjectsToSave = new List<ParentGameObjectSave>();

    //prefabs added to scene
    public List<PrefabGameObjectSave> prefabGameObjectsToSave = new List<PrefabGameObjectSave>();

    //data related to script objects
    public List<ScriptGameObjectSave> scriptGameObjectsToSave = new List<ScriptGameObjectSave>();
}

public class SCR_SaveSystem : MonoBehaviour
{

    public static SCR_SaveSystem instance;

    [SerializeField] private string fileName = "Mesh.data";
    [SerializeField] private Material gridMaterial;
    [SerializeField] private Material gridTwoSidedMaterial;
    [SerializeField] private bool bLoad;
    [SerializeField] private bool bSubstitionalReality;
    [SerializeField] private SceneReplication sceneReplicationScript;
    [SerializeField] private GameObject prefabCube;
    [SerializeField] private string materialFolderLocation = "Materials/";
    [SerializeField] private bool bMovingSceneVersion;
    [SerializeField] private GameObject sceneMoverToolObject;

    [SerializeField] List<GameObject> geometryList = new List<GameObject>();
    [SerializeField] List<GameObject> parentList = new List<GameObject>();
    [SerializeField] List<GameObject> prefabList = new List<GameObject>();
    [SerializeField] List<GameObject> scriptList = new List<GameObject>();

    public Dictionary<int, GameObject> availableInputNodes = new Dictionary<int, GameObject>();

    private bool saveTangents = false;
    private GameObject controllerMidPointLocationGameObject;

    private int groupNumber;
    public int GroupNumber
    {
        get { return groupNumber; }
        set { groupNumber = value; }
    }

    private int objectIDNumber;
    public int ObjectIDNumber
    {
        get { return objectIDNumber; }
        set { objectIDNumber = value; }
    }

    private int scriptIDNumber;
    public int ScriptIDNumber
    {
        get { return scriptIDNumber; }
        set { scriptIDNumber = value; }
    }

    void Awake()
    {
        if (instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //LoadDataMesh();
    }

    void OnApplicationQuit()
    {
        //Now called from SCR_LevelEditorManager to ensure it is called in either play or edit mode
        //SaveDataMesh();
    }

    //mesh game objects
    public void AddGeometry(GameObject newGeometryGameObject)
    {
        geometryList.Add(newGeometryGameObject);
    }

    public void RemoveGeometry(GameObject geometryGameObjectToRemove)
    {
        int indexOfGameObject = geometryList.IndexOf(geometryGameObjectToRemove);
        geometryList.RemoveAt(indexOfGameObject);
    }

    //parent game objects
    public void AddParent(GameObject newParentGameObject)
    {
        parentList.Add(newParentGameObject);
    }

    public void RemoveParent(GameObject parentGameObjectToRemove)
    {
        int indexOfGameObject = parentList.IndexOf(parentGameObjectToRemove);
        parentList.RemoveAt(indexOfGameObject);
    }

    //prefab game objects
    public void AddPrefab(GameObject newPrefabGameObject)
    {
        prefabList.Add(newPrefabGameObject);
    }

    public void RemovePrefab(GameObject prefabGameObjectToRemove)
    {
        int indexOfGameObject = prefabList.IndexOf(prefabGameObjectToRemove);
        prefabList.RemoveAt(indexOfGameObject);
    }

    //script game objects
    public void AddScript(GameObject newScriptGameObject)
    {
        scriptList.Add(newScriptGameObject);
    }

    public void RemoveScript(GameObject scriptGameObjectToRemove)
    {
        int indexOfGameObject = scriptList.IndexOf(scriptGameObjectToRemove);
        scriptList.RemoveAt(indexOfGameObject);
    }

    public void SaveDataMesh()
    {
        BinaryFormatter bf = new BinaryFormatter();

        fileName = "EditorData_" + sceneReplicationScript.participantNumber + ".dat";

        FileStream saveFile = File.Create(Application.persistentDataPath + fileName);

        SaveData saveDataFile = new SaveData();

        if (sceneReplicationScript.groupGOOutline)
        {
            saveDataFile.controllerMidPointLocation = new SVector3(sceneReplicationScript.groupGOOutline.transform.position.x, sceneReplicationScript.groupGOOutline.transform.position.y, sceneReplicationScript.groupGOOutline.transform.position.z);
            saveDataFile.controllerMidPointRotation = new SVector3(sceneReplicationScript.groupGOOutline.transform.eulerAngles.x, sceneReplicationScript.groupGOOutline.transform.eulerAngles.y, sceneReplicationScript.groupGOOutline.transform.eulerAngles.z);
        }

        foreach (var item in geometryList)
        {
            GeometryGameObjectSave newSaveData = new GeometryGameObjectSave();
            newSaveData.transform = new SVector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            newSaveData.rotation = new SVector3(item.transform.eulerAngles.x, item.transform.eulerAngles.y, item.transform.eulerAngles.z);
            newSaveData.scale = new SVector3(item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.z);
            string materialNameString = item.GetComponent<Renderer>().sharedMaterial.name;
            materialNameString = materialNameString.Replace(" (Instance)", "");
            newSaveData.materialName = materialNameString;
            newSaveData.name = item.name;
            newSaveData.objectID = item.GetComponent<SCR_ObjectData>().objectID;
            newSaveData.parentID = item.GetComponent<SCR_ObjectData>().parentID;
            newSaveData.parentName = item.GetComponent<SCR_ObjectData>().parentName;

            Color tempRendererColor = item.GetComponent<MeshRenderer>().material.color;
            newSaveData.color = new SColor(tempRendererColor.r, tempRendererColor.g, tempRendererColor.b, tempRendererColor.a);

            newSaveData.meshData = SCR_MeshSerializer.WriteMesh(item.GetComponent<MeshFilter>().mesh, saveTangents);

            saveDataFile.geometryGameObjectsToSave.Add(newSaveData);
        }

        foreach (var item in parentList)
        {
            ParentGameObjectSave newParentSaveData = new ParentGameObjectSave();
            newParentSaveData.transform = new SVector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            newParentSaveData.rotation = new SVector3(item.transform.eulerAngles.x, item.transform.eulerAngles.y, item.transform.eulerAngles.z);
            newParentSaveData.scale = new SVector3(item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.z);
            newParentSaveData.name = item.name;
            newParentSaveData.ID = item.GetComponent<SCR_GroupParent>().ID;

            saveDataFile.parentGameObjectsToSave.Add(newParentSaveData);
        }

        foreach (var item in prefabList)
        {
            PrefabGameObjectSave newPrefabSaveData = new PrefabGameObjectSave();
            newPrefabSaveData.transform = new SVector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            newPrefabSaveData.rotation = new SVector3(item.transform.eulerAngles.x, item.transform.eulerAngles.y, item.transform.eulerAngles.z);
            newPrefabSaveData.scale = new SVector3(item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.z);
            newPrefabSaveData.name = item.name;
            newPrefabSaveData.prefabID = item.GetComponent<SCR_PrefabData>().prefabID;

            saveDataFile.prefabGameObjectsToSave.Add(newPrefabSaveData);
        }

        foreach (var item in scriptList)
        {
            ScriptGameObjectSave newScriptSaveData = new ScriptGameObjectSave();
            newScriptSaveData.transform = new SVector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            newScriptSaveData.rotation = new SVector3(item.transform.eulerAngles.x, item.transform.eulerAngles.y, item.transform.eulerAngles.z);
            newScriptSaveData.scale = new SVector3(item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.z);
            newScriptSaveData.name = item.name;
            newScriptSaveData.prefabID = item.GetComponent<SCR_PrefabData>().prefabID;
            newScriptSaveData.scriptData = item.transform.GetComponentInChildren<IScriptableData<ParentScriptData>>().GetData();

            saveDataFile.scriptGameObjectsToSave.Add(newScriptSaveData);
        }

        saveDataFile.currentGroupNumber = groupNumber;
        saveDataFile.currentObjectNumber = objectIDNumber;
        saveDataFile.currentScriptNumber = scriptIDNumber;

        bf.Serialize(saveFile, saveDataFile);

        saveFile.Close();

    }

    public void LoadDataMesh()
    {
        fileName = "EditorData_" + sceneReplicationScript.participantNumber + ".dat";

        if (bLoad)
        {
            if (File.Exists(Application.persistentDataPath + fileName))
            {
                print("in loading");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream saveFile = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
                SaveData saveData = new SaveData();
                saveData = (SaveData)bf.Deserialize(saveFile);
                saveFile.Close();

                groupNumber = saveData.currentGroupNumber;
                objectIDNumber = saveData.currentObjectNumber;
                scriptIDNumber = saveData.currentScriptNumber;

                controllerMidPointLocationGameObject = new GameObject("SavedMidPoint");
                controllerMidPointLocationGameObject.transform.position = new Vector3(saveData.controllerMidPointLocation.x, saveData.controllerMidPointLocation.y, saveData.controllerMidPointLocation.z);
                controllerMidPointLocationGameObject.transform.rotation = Quaternion.Euler(saveData.controllerMidPointRotation.x, saveData.controllerMidPointRotation.y, saveData.controllerMidPointRotation.z);
                //GameObject midpointMarkCube = Instantiate(prefabCube, controllerMidPointLocationGameObject.transform.position, controllerMidPointLocationGameObject.transform.rotation);
                //midpointMarkCube.transform.SetParent(controllerMidPointLocationGameObject.transform, true);

                foreach (var item in saveData.parentGameObjectsToSave)
                {
                    GameObject newParentGameObject = new GameObject("Group" + item.ID);
                    SCR_GroupParent newGroupParentScript = newParentGameObject.AddComponent<SCR_GroupParent>();
                    newParentGameObject.transform.position = new Vector3(item.transform.x, item.transform.y, item.transform.z);
                    newParentGameObject.transform.rotation = Quaternion.Euler(item.rotation.x, item.rotation.y, item.rotation.z);
                    newParentGameObject.transform.localScale = new Vector3(item.scale.x, item.scale.y, item.scale.z);
                    newParentGameObject.name = item.name;
                    newGroupParentScript.ID = item.ID;

                    parentList.Add(newParentGameObject);
                }

                foreach (var item in saveData.geometryGameObjectsToSave)
                {
                    GameObject newGameObject = new GameObject("Geometry");
                    newGameObject.layer = 8;
                    SCR_ObjectData newObjectDataScript = newGameObject.AddComponent<SCR_ObjectData>();
                    newGameObject.transform.position = new Vector3(item.transform.x, item.transform.y, item.transform.z);
                    newGameObject.transform.rotation = Quaternion.Euler(item.rotation.x, item.rotation.y, item.rotation.z);
                    newGameObject.transform.localScale = new Vector3(item.scale.x, item.scale.y, item.scale.z);
                    newGameObject.name = item.name;
                    newObjectDataScript.objectID = item.objectID;
                    newObjectDataScript.parentID = item.parentID;
                    newObjectDataScript.parentName = item.parentName;
                    newObjectDataScript.localScaleAgainstParent = new Vector3(item.scale.x, item.scale.y, item.scale.z);

                    if (item.meshData != null)
                    {
                        newGameObject.AddComponent<MeshFilter>();
                        Mesh tempMesh = SCR_MeshSerializer.ReadMesh(item.meshData);
                        newGameObject.GetComponent<MeshFilter>().mesh = tempMesh;
                        newGameObject.AddComponent<MeshRenderer>();

                        if (item.materialName == "M_1M_Grid_TwoSided")
                        {
                            newGameObject.GetComponent<MeshRenderer>().material = gridTwoSidedMaterial;
                        }
                        else
                        {
                            newGameObject.GetComponent<MeshRenderer>().material = Resources.Load(materialFolderLocation + item.materialName) as Material;
                        }

                        newGameObject.GetComponent<MeshRenderer>().material.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a);
                        newGameObject.AddComponent<MeshCollider>();
                        newGameObject.GetComponent<MeshCollider>().sharedMesh = tempMesh;
                    }

                    geometryList.Add(newGameObject);
                }

                if (saveData.prefabGameObjectsToSave.Count > 0)
                {
                    foreach (var item in saveData.prefabGameObjectsToSave)
                    {
                        GameObject newObject = Instantiate(SCR_LevelEditorManager.instance.availablePrefabs[item.prefabID], new Vector3(item.transform.x, item.transform.y, item.transform.z), Quaternion.Euler(item.rotation.x, item.rotation.y, item.rotation.z));
                        newObject.transform.localScale = new Vector3(item.scale.x, item.scale.y, item.scale.z);
                        newObject.name = item.name;

                        prefabList.Add(newObject);
                    }
                }

                if (saveData.scriptGameObjectsToSave.Count > 0)
                {
                    foreach (var item in saveData.scriptGameObjectsToSave)
                    {
                        GameObject newObject = Instantiate(SCR_LevelEditorManager.instance.availablePrefabs[item.prefabID], new Vector3(item.transform.x, item.transform.y, item.transform.z), Quaternion.Euler(item.rotation.x, item.rotation.y, item.rotation.z));
                        newObject.transform.localScale = new Vector3(item.scale.x, item.scale.y, item.scale.z);
                        newObject.name = item.name;

                        if (newObject.transform.GetComponentInChildren<IScriptableData<ParentScriptData>>() != null)
                        {
                            newObject.transform.GetComponentInChildren<IScriptableData<ParentScriptData>>().ReconfigureScriptableNode(item.scriptData);
                        }

                        scriptList.Add(newObject);
                    }

                    foreach (var item in scriptList)
                    {
                        item.GetComponent<IScriptableData<ParentScriptData>>().ReconfigureOutputNodes();
                    }

                    foreach (var item in scriptList)
                    {
                        item.GetComponent<IScriptableData<ParentScriptData>>().StartDataFlow();
                    }
                }

                foreach (var item in parentList)
                {
                    item.transform.SetParent(controllerMidPointLocationGameObject.transform, true);
                }

                foreach (var item in geometryList)
                {
                    item.transform.SetParent(controllerMidPointLocationGameObject.transform, true);
                }

                foreach (var item in prefabList)
                {
                    item.transform.SetParent(controllerMidPointLocationGameObject.transform, true);
                }

                foreach (var item in scriptList)
                {
                    item.transform.SetParent(controllerMidPointLocationGameObject.transform, true);
                }

                GameObject actualControllerLeft = GameObject.FindGameObjectWithTag(sceneReplicationScript.controllerMidPointLeftTag);
                GameObject actualControllerRight = GameObject.FindGameObjectWithTag(sceneReplicationScript.controllerMidPointRightTag);

                Vector3 actualControllerMidPoint = (actualControllerLeft.transform.position + actualControllerRight.transform.position) * 0.5f;
                controllerMidPointLocationGameObject.transform.position = actualControllerMidPoint;
                Vector3 tempControllerRotationVectorOutline = (actualControllerMidPoint - new Vector3(actualControllerRight.transform.position.x, actualControllerMidPoint.y, actualControllerRight.transform.position.z));
                controllerMidPointLocationGameObject.transform.forward = tempControllerRotationVectorOutline;

                if (bMovingSceneVersion)
                {
                    SCR_SceneMover moverScript = sceneMoverToolObject.GetComponent<SCR_SceneMover>();
                    moverScript.InitialiseMovement(this, controllerMidPointLocationGameObject);
                    sceneMoverToolObject.SetActive(true);
                }
                else
                {
                    controllerMidPointLocationGameObject.transform.DetachChildren();

                    foreach (var item in geometryList)
                    {
                        item.GetComponent<SCR_ObjectData>().Start();
                    }
                }
            }
        }

    }

    public void FinishSettingScene()
    {
        controllerMidPointLocationGameObject.transform.DetachChildren();

        foreach (var item in geometryList)
        {
            item.GetComponent<SCR_ObjectData>().Start();
        }
    }


}
