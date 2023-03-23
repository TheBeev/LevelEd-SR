using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SSBoolData: ParentScriptData
{
    public string inputNodeName;
    public int inputNodeID;
    public string outputNodeName;
    public int inputNodeIDToConnectTo;
    public bool currentVariableValue;
}

public class SCR_SpatialScriptBoolNode : MonoBehaviour, ISSAcceptsBool, IEditableBool, IScriptable, IScriptableData<ParentScriptData> {

    [SerializeField] private bool defaultValueToPass = false;
    [SerializeField] private TextMeshPro variableValueText;
    [SerializeField] private GameObject outputDataLocationObject;
    [SerializeField] private GameObject inputNode;
    [SerializeField] private GameObject outputNode;

    [SerializeField] private Renderer[] renderersToDisableOnPlay;
    [SerializeField] private Collider[] collidersToDisableOnPlay;
    [SerializeField] private LineRenderer[] lineRenderersToDisableOnPlay;

    [SerializeField] private Transform lineEndTransform;
    public Transform LineEndTransform
    {
        get { return lineEndTransform; }
    }

    private ISSAcceptsBool outputDataLocation;
    private bool lineRenderersStatus;


    void OnEnable()
    {
        SCR_LevelEditorManager.instance.AddScriptablesToToggleVisibilty(this as IScriptable);
    }

    void OnDisable()
    {
        SCR_LevelEditorManager.instance.RemoveScriptablesToToggleVisibilty(this as IScriptable);
    }

    public void Visible(bool toggleOn)
    {
        if (toggleOn)
        {
            if (renderersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < renderersToDisableOnPlay.Length; i++)
                {
                    renderersToDisableOnPlay[i].enabled = true;
                }
            }

            if (collidersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < collidersToDisableOnPlay.Length; i++)
                {
                    collidersToDisableOnPlay[i].enabled = true;
                }
            }

            if (lineRenderersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < lineRenderersToDisableOnPlay.Length; i++)
                {
                    lineRenderersToDisableOnPlay[i].enabled = lineRenderersStatus;
                }
            }
            
            //gameObject.SetActive(true);
        }
        else
        {
            
            if (renderersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < renderersToDisableOnPlay.Length; i++)
                {
                    renderersToDisableOnPlay[i].enabled = false;
                }
            }

            if (collidersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < collidersToDisableOnPlay.Length; i++)
                {
                    collidersToDisableOnPlay[i].enabled = false;
                }
            }

            if (lineRenderersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < lineRenderersToDisableOnPlay.Length; i++)
                {
                    lineRenderersStatus = lineRenderersToDisableOnPlay[i].enabled;
                    lineRenderersToDisableOnPlay[i].enabled = false;
                }
            }

            //gameObject.SetActive(false);
        }
    }

    public void ConfigureNewScriptable()
    {
        if (inputNode)
        {
            SCR_SaveSystem.instance.ScriptIDNumber++;
            inputNode.name = "IONode_" + SCR_SaveSystem.instance.ScriptIDNumber;
            inputNode.GetComponent<IInputNode>().InputNodeID = SCR_SaveSystem.instance.ScriptIDNumber;
        }

        if (outputNode)
        {
            SCR_SaveSystem.instance.ScriptIDNumber++;
            outputNode.name = "IONode_" + SCR_SaveSystem.instance.ScriptIDNumber;
        }

        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsBool>();
        variableValueText.text = defaultValueToPass.ToString();
        outputDataLocation.AcceptBool(0, defaultValueToPass);
    }

    public void ReconfigureScriptableNode(ParentScriptData newData)
    {
        SSBoolData newBoolData = newData as SSBoolData;
        IInputNode inputNodeScript = inputNode.GetComponent<IInputNode>();
        inputNode.name = newBoolData.inputNodeName;    
        inputNodeScript.InputNodeID = newBoolData.inputNodeID;
        outputNode.name = newBoolData.outputNodeName;
        defaultValueToPass = newBoolData.currentVariableValue;

        inputNodeScript.SetUpInput();

        outputNode.GetComponent<IOutputNode>().InputNodeIDToConnectTo = newBoolData.inputNodeIDToConnectTo;

        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsBool>();
        variableValueText.text = defaultValueToPass.ToString();
        outputDataLocation.AcceptBool(0, defaultValueToPass);
    }

    public void ReconfigureOutputNodes()
    {
        outputNode.GetComponent<IOutputNode>().SetUpOutput();
    }

    public void ResetNode()
    {
        outputNode.GetComponent<IOutputNode>().RemoveTarget();
    }

    public void StartDataFlow()
    {
        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsBool>();
        variableValueText.text = defaultValueToPass.ToString();
        outputDataLocation.AcceptBool(0, defaultValueToPass);
    }

    public ParentScriptData GetData()
    {
        SSBoolData newBoolData = new SSBoolData();
        newBoolData.inputNodeName = inputNode.name;
        newBoolData.inputNodeID = inputNode.GetComponent<IInputNode>().InputNodeID;
        newBoolData.outputNodeName = outputNode.name;
        newBoolData.inputNodeIDToConnectTo = outputNode.GetComponent<IOutputNode>().InputNodeIDToConnectTo;
        newBoolData.currentVariableValue = defaultValueToPass;

        return newBoolData as ParentScriptData;
    }

    
    public void SetNewBoolValue()
    {
        defaultValueToPass = !defaultValueToPass;
        variableValueText.text = defaultValueToPass.ToString();
        outputDataLocation.AcceptBool(0, defaultValueToPass);
    }
    

    public void AcceptBool(int variableOrder, bool incomingVariable)
    {
        switch (variableOrder)
        {
            case 0:
                {
                    defaultValueToPass = incomingVariable;
                    variableValueText.text = defaultValueToPass.ToString();
                    outputDataLocation.AcceptBool(0, defaultValueToPass);
                    break;
                }
            default:
                break;
        }
    }

}
