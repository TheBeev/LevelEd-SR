using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ParentAttach : MonoBehaviour {

    [SerializeField] private GameObject parentToAttachTo;
    [SerializeField] private GameObject objectToAttach;
    [SerializeField] private bool bUseOffset;

	// Use this for initialization
	void Start ()
    {
        if (bUseOffset)
        {
            Vector3 offset = Vector3.zero + objectToAttach.transform.position;
            objectToAttach.transform.position = parentToAttachTo.transform.position + offset;
        }
        else
        {
            objectToAttach.transform.position = parentToAttachTo.transform.position;
        }
        
        objectToAttach.transform.SetParent(parentToAttachTo.transform);
	}
	
}
