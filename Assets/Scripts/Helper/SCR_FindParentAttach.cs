using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_FindParentAttach : MonoBehaviour {

    [SerializeField] private string parentToAttachToName;
    [SerializeField] private GameObject objectToAttach;
    [SerializeField] private bool bUseOffset;
    [SerializeField] private bool bUseQuestOffset;
    [SerializeField] private bool bSnapRotation;
    [SerializeField] private Vector3 manualOffset;

    private GameObject parentObject;
    private GameObject parentToAttachTo;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(FindController());
    }

    IEnumerator FindController()
    {
        while(parentToAttachTo == null)
        {
            parentToAttachTo = GameObject.Find(parentToAttachToName);
            //parentToAttachTo = parentObject.transform.Find(parentToAttachToName)
            print("looking");
            yield return null;
        }

        if (bUseOffset)
        {
            Vector3 offset = Vector3.zero + objectToAttach.transform.position;
            objectToAttach.transform.position = parentToAttachTo.transform.position + offset;
        }
        else if (!bUseOffset || bUseQuestOffset)
        {
            objectToAttach.transform.position = parentToAttachTo.transform.position;
        }

        if (bSnapRotation)
        {
            objectToAttach.transform.rotation = parentToAttachTo.transform.rotation;
        }

        objectToAttach.transform.SetParent(parentToAttachTo.transform);

        if (bUseQuestOffset)
        {
            objectToAttach.transform.localPosition += manualOffset;
        }

    }

}
