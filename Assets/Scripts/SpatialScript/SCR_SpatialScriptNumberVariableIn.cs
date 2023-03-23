﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_SpatialScriptNumberVariableIn : MonoBehaviour, ISSAcceptsInt, ISSLineTarget, IInputNode
{

    [SerializeField] private int orderInNode = 0;
    [SerializeField] private GameObject outputDataLocationObject;
    [SerializeField] private int variableToPass = 10;
    [SerializeField] private Renderer inputRenderer;
    [SerializeField] private Color startingColour = Color.green;

    [SerializeField] private NodeType thisNodeType = NodeType.Number;
    public NodeType ThisNodeType
    {
        get { return thisNodeType; }
    }

    [SerializeField] private Transform lineEndTransform;
    public Transform LineEndTransform
    {
        get { return lineEndTransform; }
    }

    private int inputNodeID;
    public int InputNodeID
    {
        get { return inputNodeID; }
        set { inputNodeID = value; }
    }

    private string inputNodeName;
    public string InputNodeName
    {
        get { return inputNodeName; }
    }

    private GameObject outputNodeConnectedObject;
    public GameObject OutputNodeConnectedObject
    {
        get { return outputNodeConnectedObject; }
        set { outputNodeConnectedObject = value; }
    }

    private Transform targetTag = null;
    private Vector3 targetLocationCheck;
    private Vector3 thisLocationCheck;
    private ISSAcceptsInt outputDataLocation;

    // Use this for initialization
    void Awake ()
    {
        outputDataLocation = outputDataLocationObject.GetComponent<ISSAcceptsInt>();
        inputNodeName = gameObject.name;

        if (inputRenderer)
        {
            inputRenderer.material.color = startingColour;
        }

        thisLocationCheck = transform.position;
    }

    void OnDisable()
    {
        if (outputNodeConnectedObject)
        {
            if (outputNodeConnectedObject.activeSelf)
            {
                outputNodeConnectedObject.GetComponent<IOutputNode>().RemoveTarget();
            }
        }
    }

    public void SetUpInput()
    {
        SCR_SaveSystem.instance.availableInputNodes.Add(inputNodeID, this.gameObject);
    }

    public void UpdateLineOriginLocation(Transform lineOriginTransform)
    {
        targetTag = lineOriginTransform;
        targetLocationCheck = targetTag.position;
    }

    public void AcceptInt(int variableOrder, int incomingVariable)
    {
        switch (variableOrder)
        {
            case 0:
                {
                    variableToPass = incomingVariable;
                    if (outputDataLocation != null)
                    {
                        outputDataLocation.AcceptInt(orderInNode, variableToPass);

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
            if (targetLocationCheck != targetTag.position || thisLocationCheck != transform.position)
            {
                Vector3 targetAdjusted = targetTag.position - transform.position;

                targetAdjusted.y = 0f;

                Quaternion targetRotation = Quaternion.LookRotation(targetAdjusted);
                transform.rotation = targetRotation;
                targetLocationCheck = targetTag.position;
                thisLocationCheck = transform.position;
            }
            
        }
        
    }
}
