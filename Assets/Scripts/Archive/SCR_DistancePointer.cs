using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_DistancePointer : MonoBehaviour
{
    [SerializeField] private LineRenderer distanceLineRender;
    [SerializeField] private GameObject pointerEnd;
    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightStickTouched;
    [SerializeField] private float pointerEndScaleMultiplier = 1.2f;
    [SerializeField] private float maxDistance = 1000f;

    private RaycastHit hit;
    private Vector3 pointerEndOriginalScale;
    private float distanceOfBeam;
    private Vector3 maximumPointerEndScale;

    private bool bToggleDistance = false;
    public bool ToggleDistance
    {
        get { return bToggleDistance; }
        set { bToggleDistance = value; }
    }

    private Vector3 pointerPosition;
    public Vector3 PointerPosition
    {
        get{ return pointerPosition; }
    }

    private bool bValidTargetPosition;
    public bool ValidTargetPosition
    {
        get { return bValidTargetPosition; }
    }

    private bool bActive;
    public bool Active
    {
        get { return bActive; }
        set { bActive = value; }
    }


    private void OnEnable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started += DoActivationButtonPressed;
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started -= DoActivationButtonPressed;
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        bActive = !bActive;
    }


    // Use this for initialization
    void Start ()
    {
        pointerEndOriginalScale = pointerEnd.transform.localScale;
        maximumPointerEndScale = pointerEndOriginalScale * 15f;
        pointerEnd.SetActive(false);
        distanceLineRender.enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (bActive)
        {
            distanceLineRender.SetPosition(0, transform.position);
            distanceLineRender.enabled = true;

            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
            {
                pointerEnd.SetActive(true);
                bValidTargetPosition = true;
                distanceLineRender.SetPosition(1, hit.point);
                pointerEnd.transform.position = hit.point;
                pointerPosition = hit.point;
                distanceOfBeam = Vector3.Distance(transform.position, hit.point) * pointerEndScaleMultiplier;
                pointerEnd.transform.localScale = Vector3.Min(pointerEndOriginalScale * distanceOfBeam, maximumPointerEndScale);
            }
            else
            {
                Vector3 distancePosition = transform.position + (transform.forward * maxDistance);
                bValidTargetPosition = true;
                pointerPosition = distancePosition;
                distanceLineRender.SetPosition(1, distancePosition);
                //bValidTargetPosition = false;
            }
        }
        else
        {
            distanceLineRender.enabled = false;
            pointerEnd.SetActive(false);
        }
	}
}
