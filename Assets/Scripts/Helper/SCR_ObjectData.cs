using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ObjectData : MonoBehaviour
{
    public int objectID;
    public int parentID;
    public string parentName;
    public Vector3 localScaleAgainstParent;

    private GameObject parentObject;

    public void Start()
    {
        if (parentName != null && parentID != 0)
        {
            transform.localScale = Vector3.one;
            parentObject = GameObject.Find(parentName);
            transform.parent = parentObject.transform;
            transform.localScale = localScaleAgainstParent;

            if (!parentObject.GetComponent<SCR_GroupParent>().groupedObjectList.Contains(gameObject))
            {
                parentObject.GetComponent<SCR_GroupParent>().groupedObjectList.Add(gameObject);
            }
            
        }
    }
}
