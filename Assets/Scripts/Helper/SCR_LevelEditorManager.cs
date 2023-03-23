using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EditorState { LevelEditor, Game }

[System.Serializable]
public struct AvailablePrefabsDictionary
{
    public int prefabID;
    public GameObject prefabGameObject;
}

public class SCR_LevelEditorManager : MonoBehaviour {

    public static SCR_LevelEditorManager instance;

    [SerializeField] private ControllerInputs activationButtonLeft = ControllerInputs.LeftStickClick;
    [SerializeField] private ControllerInputs activationButtonRight = ControllerInputs.RightStickClick;
    [SerializeField] private float playModeInputDelay = 0.25f;

    [SerializeField] private GameObject[] editorObjectsToTurnOff;
    [SerializeField] private GameObject[] playObjectsToTurnOff;
    [SerializeField] private List<IScriptable> scriptObjectsToToggleVisibilty = new List<IScriptable>();

    [SerializeField] private List<GameObject> levelObjectsToInteract = new List<GameObject>();
    [SerializeField] private AvailablePrefabsDictionary[] availablePrefabsArray;

    public Dictionary<int, GameObject> availablePrefabs = new Dictionary<int, GameObject>();

    private EditorState currentEditorState = EditorState.LevelEditor;
    public EditorState CurrentEditorState
    {
        get { return currentEditorState; }
    }

    //used to turn the AR generated meshes on or off when going into play.
    private GameObject meshingParentObject;
    private GameObject outlineParentObject;

    private float playModeInputTimer;
    private bool bPlayModeInputLeftActive;
    private bool bPlayModeInputRightActive;
    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            SCR_InputDetection.instance.SubscribeToInput(activationButtonLeft, DoActivationButtonLeftPressed);
            SCR_InputDetection.instance.SubscribeToInput(activationButtonRight, DoActivationButtonRightPressed);
        }
    }

    private void OnDisable()
    {
        //bActivationButtonPressed = false;
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonLeft, DoActivationButtonLeftPressed);
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButtonRight, DoActivationButtonRightPressed);
    }

    void DoActivationButtonLeftPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            bPlayModeInputLeftActive = true;
        }
        else if (context.canceled)
        {
            bPlayModeInputLeftActive = false;
        }
    }

    void DoActivationButtonRightPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            bPlayModeInputRightActive = true;
        }
        else if (context.canceled)
        {
            bPlayModeInputRightActive = false;
        } 
    }


    private void Awake()
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

        for (int i = 0; i < availablePrefabsArray.Length; i++)
        {
            availablePrefabs.Add(availablePrefabsArray[i].prefabID, availablePrefabsArray[i].prefabGameObject);
        }

    }

    // Use this for initialization
    void Start ()
    {
        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            OnEnable();
        }
    }

    void OnApplicationQuit()
    {
        SCR_SaveSystem.instance.SaveDataMesh();
    }
    void OnApplicationPause()
    {
        //SCR_SaveSystem.instance.SaveDataMesh();
    }

    // Update is called once per frame
    void Update ()
    {
        if (bPlayModeInputLeftActive && bPlayModeInputRightActive)
        {
            if (!SCR_ToolMenuRadial.instance.ToolBusy())
            {
                playModeInputTimer += Time.deltaTime;

                if (playModeInputTimer >= playModeInputDelay)
                {
                    ToggleEditorState();
                    bPlayModeInputRightActive = false;
                    bPlayModeInputLeftActive = false;
                    playModeInputTimer = 0f;
                }
            }
        }	
        else if(playModeInputTimer > 0)
        {
            playModeInputTimer = 0f;
        }
	}

    public void ToggleEditorState()
    {
        switch (currentEditorState)
        {
            case EditorState.LevelEditor:
                currentEditorState = EditorState.Game;
                StartPlayMode();
                break;
            case EditorState.Game:
                currentEditorState = EditorState.LevelEditor;
                StartLevelEditorMode();
                break;
        }

    }

    public void AddPrefabsToInteractive(GameObject prefabToAdd)
    {
        levelObjectsToInteract.Add(prefabToAdd);
    }

    public void RemovePrefabsFromInteractive(GameObject prefabToRemove)
    {
        levelObjectsToInteract.Remove(prefabToRemove);
    }

    public void AddScriptablesToToggleVisibilty(IScriptable scriptToAdd)
    {
        scriptObjectsToToggleVisibilty.Add(scriptToAdd);
    }

    public void RemoveScriptablesToToggleVisibilty(IScriptable scriptToAdd)
    {
        scriptObjectsToToggleVisibilty.Remove(scriptToAdd);
    }

    void StartLevelEditorMode()
    {
        foreach (var item in playObjectsToTurnOff)
        {
            item.SetActive(false);
        }

        foreach (var item in editorObjectsToTurnOff)
        {
            item.SetActive(true);
        }

        foreach (var item in scriptObjectsToToggleVisibilty)
        {
            item.Visible(true);
        }

        foreach (var item in levelObjectsToInteract)
        {
            item.GetComponent<ILevelItemInteractive>().Disable();
        }

        
    }

    void StartPlayMode()
    {
        foreach (var item in editorObjectsToTurnOff)
        {
            item.SetActive(false);
        }

        foreach (var item in playObjectsToTurnOff)
        {
            item.SetActive(true);
        }

        foreach (var item in levelObjectsToInteract)
        {
            item.GetComponent<ILevelItemInteractive>().Enable();
        }

        foreach (var item in scriptObjectsToToggleVisibilty)
        {
            item.Visible(false);
        }
    }

}
