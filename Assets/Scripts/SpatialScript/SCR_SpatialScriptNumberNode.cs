using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SSNumberData: ParentScriptData
{
    public string inputNodeName;
    public int inputNodeID;
    public string outputNodeName;
    public int inputNodeIDToConnectTo;
    public int currentVariableValue;
}

public class SCR_SpatialScriptNumberNode : MonoBehaviour, ISSAcceptsInt, IEditableNumber, IScriptable, IScriptableData<ParentScriptData> {

    [SerializeField] private int defaultNumberToPass;
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

    private ISSAcceptsInt outputDataLocation;
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

        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsInt>();
        variableValueText.text = defaultNumberToPass.ToString();
        outputDataLocation.AcceptInt(0, defaultNumberToPass);
    }

    public void ReconfigureScriptableNode(ParentScriptData newData)
    {
        SSNumberData newNumberData = newData as SSNumberData;
        IInputNode inputNodeScript = inputNode.GetComponent<IInputNode>();
        inputNode.name = newNumberData.inputNodeName;    
        inputNodeScript.InputNodeID = newNumberData.inputNodeID;
        outputNode.name = newNumberData.outputNodeName;
        defaultNumberToPass = newNumberData.currentVariableValue;

        inputNodeScript.SetUpInput();

        outputNode.GetComponent<IOutputNode>().InputNodeIDToConnectTo = newNumberData.inputNodeIDToConnectTo;

        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsInt>();
        variableValueText.text = defaultNumberToPass.ToString();
        outputDataLocation.AcceptInt(0, defaultNumberToPass);
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
        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsInt>();
        variableValueText.text = defaultNumberToPass.ToString();
        outputDataLocation.AcceptInt(0, defaultNumberToPass);
    }

    public ParentScriptData GetData()
    {
        SSNumberData newNumberData = new SSNumberData();
        newNumberData.inputNodeName = inputNode.name;
        newNumberData.inputNodeID = inputNode.GetComponent<IInputNode>().InputNodeID;
        newNumberData.outputNodeName = outputNode.name;
        newNumberData.inputNodeIDToConnectTo = outputNode.GetComponent<IOutputNode>().InputNodeIDToConnectTo;
        newNumberData.currentVariableValue = defaultNumberToPass;

        return newNumberData as ParentScriptData;
    }

    public void SetNewNumberValue(int newNumberValue)
    {
        defaultNumberToPass = newNumberValue;
        variableValueText.text = defaultNumberToPass.ToString();
        outputDataLocation.AcceptInt(0, defaultNumberToPass);
    }

    public void AcceptInt(int variableOrder, int incomingVariable)
    {
        switch (variableOrder)
        {
            case 0:
                {
                    defaultNumberToPass = incomingVariable;
                    variableValueText.text = defaultNumberToPass.ToString();
                    outputDataLocation.AcceptInt(0, defaultNumberToPass);
                    break;
                }
            default:
                break;
        }
    }

}
