using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class SCR_SpatialScriptStartingTeleporterNode : MonoBehaviour, IScriptable, IScriptableData<ParentScriptData> {


    //[SerializeField] private TextMeshPro variableValueText;
    //[SerializeField] private GameObject outputDataLocationObject;
    //[SerializeField] private GameObject outputNode;

    [SerializeField] private GameObject[] listOfInputNodes;
    [SerializeField] private Renderer[] renderersToDisableOnPlay;
    [SerializeField] private Collider[] collidersToDisableOnPlay;
    [SerializeField] private LineRenderer[] lineRenderersToDisableOnPlay;

    [SerializeField] private Transform lineEndTransform;
    public Transform LineEndTransform
    {
        get { return lineEndTransform; }
    }

    private bool defaultValueToPass = true;
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
        if (listOfInputNodes.Length > 0)
        {
            for (int i = 0; i < listOfInputNodes.Length; i++)
            {
                SCR_SaveSystem.instance.ScriptIDNumber++;
                listOfInputNodes[i].name = "IONode_" + SCR_SaveSystem.instance.ScriptIDNumber;
                listOfInputNodes[i].GetComponent<IInputNode>().InputNodeID = SCR_SaveSystem.instance.ScriptIDNumber;
            }
        }
    }

    public void ReconfigureScriptableNode(ParentScriptData newData)
    {
        SSScoreManagerData newScoreManagerData = newData as SSScoreManagerData;

        if (listOfInputNodes.Length > 0)
        {
            for (int i = 0; i < listOfInputNodes.Length; i++)
            {
                IInputNode inputNodeScript = listOfInputNodes[i].GetComponent<IInputNode>();
                inputNodeScript.InputNodeID = newScoreManagerData.listOfInputNodeIDs[i];
                inputNodeScript.SetUpInput();
            }
        }
    }

    public void ReconfigureOutputNodes()
    {
        //no nodes
    }

    public void StartDataFlow()
    {
        //no data to pass on
    }

    public void ResetNode()
    {
        //no output nodes to reset
    }

    public ParentScriptData GetData()
    {
        SSScoreManagerData newScoreManagerData = new SSScoreManagerData();

        if (listOfInputNodes.Length > 0)
        {
            newScoreManagerData.listOfInputNodeIDs = new int[listOfInputNodes.Length];
            for (int i = 0; i < listOfInputNodes.Length; i++)
            {
                newScoreManagerData.listOfInputNodeIDs[i] = listOfInputNodes[i].GetComponent<IInputNode>().InputNodeID;
            }
        }

        return newScoreManagerData as ParentScriptData;
    }

}
