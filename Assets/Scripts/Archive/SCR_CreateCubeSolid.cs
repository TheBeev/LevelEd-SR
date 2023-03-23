using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_CreateCubeSolid : MonoBehaviour
{

    [SerializeField] private GameObject spawnObject;
    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;

    private IPointer antenna;
    private IPointer distance;

    private void OnEnable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started += DoActivationButtonPressed;
        //controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.playerInput.actions[activationButton.ToString()].started -= DoActivationButtonPressed;
        //controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        CreateCube();
    }

    void DoActivationButtonReleased(InputAction.CallbackContext context)
    {

    }

    void CreateCube()
    {
        Vector3 spawnPosition = Vector3.zero;

        if (antenna.Active && antenna.ValidTargetPosition)
        {
            spawnPosition = antenna.PointerPosition;
        }
        else if (distance.Active && distance.ValidTargetPosition)
        {
            spawnPosition = distance.PointerPosition;
        }
        else
        {
            return;
        }

        Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
        Instantiate(spawnObject, spawnPosition + offset, spawnObject.transform.rotation);
    }

    // Use this for initialization
    void Start ()
    {
        GameObject antennaObject = GameObject.FindGameObjectWithTag("LeftAntenna");

        if (antennaObject)
        {
            antenna = (IPointer)antennaObject.GetComponent(typeof(IPointer));
        }


        GameObject distanceObject = GameObject.FindGameObjectWithTag("LeftDistance");
        if (distanceObject)
        {
            distance = (IPointer)distanceObject.GetComponent(typeof(IPointer));
        }
    }
	
    
	// Update is called once per frame
	void Update ()
    {
        
        
    }
}
