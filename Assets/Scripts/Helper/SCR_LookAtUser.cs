using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_LookAtUser : MonoBehaviour {

    [SerializeField] private Transform headsetCentre;
    [SerializeField] private bool bOnlyOnEnabled;
    [SerializeField] private bool bRestrictZRotation;

    private void OnEnable()
    {
        transform.LookAt(headsetCentre);

        if (bRestrictZRotation)
        {
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
        }
    }

    //adapated from Carter (2018) http://wiki.unity3d.com/index.php?title=CameraFacingBillboard
    // Update is called once per frame
    void LateUpdate ()
    {
        if (!bOnlyOnEnabled)
        {
            transform.LookAt(headsetCentre);
        }
        

        /*
        if (Vector3.Distance(transform.position, headsetCentre.position) >= 1f)
        {
            transform.position = headsetCentre.position + (Vector3.Normalize(transform.position - headsetCentre.position) * 1f);
        }
        */
        
    }
}
