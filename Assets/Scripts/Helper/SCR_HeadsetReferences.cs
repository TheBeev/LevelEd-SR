using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_HeadsetReferences : MonoBehaviour
{
    public static SCR_HeadsetReferences instance;

    public GameObject leftEye;
    public GameObject rightEye;
    public GameObject centerEye;

    public GameObject playerSpace;

    public OVRScreenFade screenFade;

    private void Awake()
    {
        instance = this;
    }

}
