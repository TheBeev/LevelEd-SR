using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_LookAtHeadset : MonoBehaviour {

    [SerializeField] private bool bXAxis = true;
    [SerializeField] private bool bYAxis = true;
    [SerializeField] private bool bZAxis = true;

    private Transform headsetTransform;

    private Vector3 targetToLookAt;
    private Quaternion targetRotation;
    private Vector3 targetAdjusted;

    // Use this for initialization
    void Start ()
    {

    }

    //Adapted from Rocket5tim (2009) https://forum.unity.com/threads/transform-lookat-or-quaternion-lookrotation-on-1-axis-only.36377/
    // Update is called once per frame
    void Update ()
    {
        if (headsetTransform == null)
        {
            headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;
        }
        else
        {
            targetAdjusted = transform.position - headsetTransform.position;

            if (!bXAxis)
            {
                targetAdjusted.x = 0f;
            }

            if (!bYAxis)
            {
                targetAdjusted.y = 0f;
            }

            if (!bZAxis)
            {
                targetAdjusted.z = 0f;
            }

            targetRotation = Quaternion.LookRotation(targetAdjusted);
            transform.rotation = targetRotation;
        }
        
    }
}
