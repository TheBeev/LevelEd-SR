using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_SpatialScriptStartingTeleporterOut : MonoBehaviour, ISSAcceptsBool, IOutputNode
{

    [SerializeField] private Transform currentTag;
    [SerializeField] private LineRenderer variableLine;
    [SerializeField] private int orderInNode = 0;
    [SerializeField] private Renderer outputRenderer;
    [SerializeField] private Color startingColour = Color.red;

    [SerializeField] NodeType nodeTypeRequired = NodeType.Teleporter;
    public NodeType NodeTypeRequired
    {
        get { return nodeTypeRequired; }
    }

    [SerializeField] private Transform lineEndTransform;
    public Transform LineEndTransform
    {
        get { return lineEndTransform; }
    }

    [SerializeField] private int inputNodeIDToConnectTo;
    public int InputNodeIDToConnectTo
    {
        get { return inputNodeIDToConnectTo; }
        set { inputNodeIDToConnectTo = value; }
    }

    private Transform targetTag;
    private bool bShouldHaveTarget;
    private Vector3 targetLocationCheck;
    private Vector3 thisLocationCheck;
    private GameObject targetObject;
    private bool variableToPass;
    private ISSAcceptsBool outputDataLocation;
    private ISSLineTarget lineTargetObject;
    private IInputNode connectedInputNode;

	// Use this for initialization
	void Start ()
    {
        if (outputRenderer)
        {
            outputRenderer.material.color = startingColour;
        }
        
        thisLocationCheck = transform.position;
    }

    public void Selected(bool bCurrentlySelected)
    {
        if (bCurrentlySelected && outputRenderer)
        {
            outputRenderer.material.color = Color.blue;
        }
        else if (!bCurrentlySelected && outputRenderer)
        {
            outputRenderer.material.color = startingColour;
        }
    }

    public void SetUpOutput()
    {
        bShouldHaveTarget = true;

        if (inputNodeIDToConnectTo > 0)
        {
            targetObject = SCR_SaveSystem.instance.availableInputNodes[inputNodeIDToConnectTo];
            outputDataLocation = targetObject.GetComponent<ISSAcceptsBool>();
            lineTargetObject = targetObject.GetComponent<ISSLineTarget>();
            connectedInputNode = targetObject.GetComponent<IInputNode>();
            connectedInputNode.OutputNodeConnectedObject = gameObject;
            targetTag = outputDataLocation.LineEndTransform;
            lineTargetObject.UpdateLineOriginLocation(currentTag);
            variableLine.enabled = true;
            variableLine.SetPosition(0, currentTag.position);
            variableLine.SetPosition(1, targetTag.position);
        }
        else
        {
            RemoveTarget();
        }
 
    }

    public void UpdateInputNodeToConnectTo(int newID, GameObject newTarget, bool newShouldHaveTarget)
    {
        inputNodeIDToConnectTo = newID;
        targetObject = newTarget;
        bShouldHaveTarget = true;
        outputDataLocation = targetObject.GetComponent<ISSAcceptsBool>();
        lineTargetObject = targetObject.GetComponent<ISSLineTarget>();
        targetTag = outputDataLocation.LineEndTransform;
        lineTargetObject.UpdateLineOriginLocation(currentTag);

        AcceptBool(0, variableToPass);

        variableLine.enabled = true;
        variableLine.SetPosition(0, currentTag.position);
        variableLine.SetPosition(1, targetTag.position);
    }

    public void RemoveTarget()
    {
        inputNodeIDToConnectTo = 0;

        if (lineTargetObject != null)
        {
            lineTargetObject.UpdateLineOriginLocation(outputDataLocation.LineEndTransform);
        }


        //set the previous teleporter to not be the default teleporter
        if (outputDataLocation != null)
        {
            outputDataLocation.AcceptBool(0, false);
        }
        
        targetObject = null;
        targetTag = null;
        outputDataLocation = null;
        bShouldHaveTarget = false;
        variableLine.enabled = false;
        variableLine.SetPosition(0, currentTag.position);
        variableLine.SetPosition(1, currentTag.position);
    }

    public void AcceptBool(int variableOrder, bool incomingVariable)
    {
        switch (variableOrder)
        {
            case 0:
                {
                    variableToPass = incomingVariable;

                    if (outputDataLocation != null)
                    {
                        outputDataLocation.AcceptBool(orderInNode, variableToPass);
                    }
                    
                    break;
                }
            default:
                break;
        } 
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (targetTag)
        {
            if (targetTag.position != targetLocationCheck || thisLocationCheck != transform.position)
            {
                /*
                if (lineTargetObject != null)
                {
                    lineTargetObject.UpdateLineOriginLocation(currentTag);
                }*/

                Vector3 targetAdjusted = targetTag.position - transform.position;
                targetAdjusted.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(targetAdjusted);
                transform.rotation = targetRotation;

                variableLine.SetPosition(0, currentTag.position);
                variableLine.SetPosition(1, targetTag.position);
                targetLocationCheck = targetTag.position;
                thisLocationCheck = transform.position;
            }
        } 
	}
}
