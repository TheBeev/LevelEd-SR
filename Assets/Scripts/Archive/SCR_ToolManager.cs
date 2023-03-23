using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SCR_ToolManager : MonoBehaviour {

    private enum Tools { Group, CreateCube, Move, Grab, Rotate, Scale, Copy, Delete };
    private Tools currentTool = Tools.Group;

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightButtonOne;
    [SerializeField] private Text toolText;
    [SerializeField] private SCR_Group groupTool;
    [SerializeField] private SCR_CreateCubeCustomSteps createCubeTool;
    [SerializeField] private SCR_Move moveTool;
    [SerializeField] private SCR_Grab grabTool;
    [SerializeField] private SCR_Rotate rotateTool;
    [SerializeField] private SCR_Scale scaleTool;
    [SerializeField] private SCR_Copy copyTool;
    [SerializeField] private SCR_Delete deleteTool;

    private MonoBehaviour mb;

    private void OnEnable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started += DoActivationButtonPressed;
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started -= DoActivationButtonPressed;
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        switch (currentTool)
        {
            case Tools.Group:

                if (!groupTool.Busy)
                {
                    toolText.text = "Create";
                    groupTool.gameObject.SetActive(false);
                    createCubeTool.gameObject.SetActive(true);
                    currentTool = Tools.CreateCube;
                }
                
                break;
            case Tools.CreateCube:

                if (!createCubeTool.Busy)
                {
                    toolText.text = "Move";
                    createCubeTool.gameObject.SetActive(false);
                    moveTool.gameObject.SetActive(true);
                    currentTool = Tools.Move;
                }

                break;
            case Tools.Move:

                if (!moveTool.Busy)
                {
                    toolText.text = "Grab";
                    moveTool.gameObject.SetActive(false);
                    grabTool.gameObject.SetActive(true);
                    currentTool = Tools.Grab;
                }
                
                break;
            case Tools.Grab:

                if (!grabTool.Busy)
                {
                    toolText.text = "Rotate";
                    grabTool.gameObject.SetActive(false);
                    rotateTool.gameObject.SetActive(true);
                    currentTool = Tools.Rotate;
                }
                
                break;
            case Tools.Rotate:

                if (!rotateTool.Busy)
                {
                    toolText.text = "Scale";
                    rotateTool.gameObject.SetActive(false);
                    scaleTool.gameObject.SetActive(true);
                    currentTool = Tools.Scale;
                }

                break;
            case Tools.Scale:

                if (!scaleTool.Busy)
                {
                    toolText.text = "Copy";
                    scaleTool.gameObject.SetActive(false);
                    copyTool.gameObject.SetActive(true);
                    currentTool = Tools.Copy;
                }

                break;
            case Tools.Copy:

                if (!copyTool.Busy)
                {
                    toolText.text = "Delete";
                    copyTool.gameObject.SetActive(false);
                    deleteTool.gameObject.SetActive(true);
                    currentTool = Tools.Delete;
                }

                break;
            case Tools.Delete:

                if (!deleteTool.Busy)
                {
                    toolText.text = "Group";
                    deleteTool.gameObject.SetActive(false);
                    groupTool.gameObject.SetActive(true);
                    currentTool = Tools.Group;
                }

                break;
            default:
                break;
        }
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
