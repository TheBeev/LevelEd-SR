using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_SimpleTeleport : MonoBehaviour {

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.LeftTrigger;
    [SerializeField] private GameObject teleporterEffectPrefab;
    [SerializeField] private Color teleportPointerColour;
    [SerializeField] private float pointerStartWidth = 0.02f;
    [SerializeField] private float pointerEndWidth = 0.02f;
    [SerializeField] private float maxTeleportDistance = 1000f;

    private Transform playerTransform;
    private Transform headsetTransform;
    private Vector3 headsetOffset;
    private Vector3 objectOffset; //based on normal of object raycast hits
    private Vector3 newTeleportPosition;
    private WaitForSeconds fadeDelay = new WaitForSeconds(0.25f);
    private bool bCurrentlyTeleporting;

    private bool bActivationButtonPressed;
    private bool bTeleportActivated;
    private IPointer variablePointer;
    private GameObject teleportEffectGameObject;

    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
        }
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);

        if (variablePointer != null)
        {
            variablePointer.VariablePointerActive = false;
            variablePointer.SnapPointerState(PointerStates.Short);
            variablePointer.SetPointerColourDefault();
            variablePointer.SetPointerWidthDefault();
            variablePointer.SetPointerEndSizeDefault();
        }

        if (teleportEffectGameObject)
        {
            teleportEffectGameObject.SetActive(false);
        }

        bTeleportActivated = false;
        bCurrentlyTeleporting = false;
        bActivationButtonPressed = false;
    }

    void DoSecondaryActivationButtonPressed(InputAction.CallbackContext context)
    {
        
    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor && !SCR_ToolOptions.instance.OptionsMenuOpen)
            {

                if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
                {
                    bActivationButtonPressed = true;

                    if (teleportEffectGameObject == null)
                    {
                        teleportEffectGameObject = Instantiate(teleporterEffectPrefab, variablePointer.PointerEndGameObject.transform.position, teleporterEffectPrefab.transform.rotation);
                    }
                    else
                    {
                        teleportEffectGameObject.SetActive(true);
                    }

                    variablePointer.SetPointerColour(teleportPointerColour);
                    variablePointer.SetPointerEndSize(0.1f);
                    variablePointer.SetPointerWidth(pointerStartWidth, pointerEndWidth);
                    variablePointer.VariablePointerActive = true;
                    variablePointer.SnapPointerState(PointerStates.Medium);
                }
            }
        }
        else if (context.canceled)
        {
            bActivationButtonPressed = false;

            if (bTeleportActivated && !SCR_ToolOptions.instance.OptionsMenuOpen)
            {
                if (!bCurrentlyTeleporting)
                {
                    StartCoroutine(TeleportHereRoutine());
                }
            }
            else
            {
                variablePointer.VariablePointerActive = false;
                variablePointer.SnapPointerState(PointerStates.Short);

                if (teleportEffectGameObject == null)
                {
                    teleportEffectGameObject = Instantiate(teleporterEffectPrefab, variablePointer.PointerEndGameObject.transform.position, teleporterEffectPrefab.transform.rotation);
                }
                else
                {
                    teleportEffectGameObject.SetActive(false);
                }

                variablePointer.SetPointerColourDefault();
                variablePointer.SetPointerWidthDefault();
                variablePointer.SetPointerEndSizeDefault();
                bTeleportActivated = false;
                bCurrentlyTeleporting = false;
            }
        }
        
    }

  

    IEnumerator TeleportHereRoutine()
    {
        bTeleportActivated = false;
        bCurrentlyTeleporting = true;
        SCR_HeadsetReferences.instance.screenFade.FadeOut();

        SCR_ToolMenuRadial.instance.TempCloseMenu();
        SCR_ToolOptions.instance.TempOptionsMenuClose();

        if (headsetTransform == null)
        {
            headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;
        }

        if (playerTransform == null)
        {
            playerTransform = SCR_HeadsetReferences.instance.playerSpace.transform;
        }


        //might not need to do he offset with the new OVR method.

        objectOffset = new Vector3(variablePointer.PointerHit.normal.x * 1f, 0f, variablePointer.PointerHit.normal.z * 1f);

        headsetOffset = new Vector3(playerTransform.position.x - headsetTransform.position.x, 0f, playerTransform.position.z - headsetTransform.position.z);

        newTeleportPosition = variablePointer.PointerPosition + headsetOffset + objectOffset;

        yield return fadeDelay;

        if (Vector3.Distance(playerTransform.localPosition, newTeleportPosition) <= maxTeleportDistance)
        {
            playerTransform.localPosition = newTeleportPosition;
        }

        //playerTransform.localRotation = variablePointer.PointerEndGameObject.transform.rotation;
        SCR_HeadsetReferences.instance.screenFade.FadeIn();

        variablePointer.VariablePointerActive = false;
        variablePointer.SnapPointerState(PointerStates.Short);
        teleportEffectGameObject.SetActive(false);
        variablePointer.SetPointerColourDefault();
        variablePointer.SetPointerWidthDefault();
        variablePointer.SetPointerEndSizeDefault();

        bCurrentlyTeleporting = false;
    }

    // Use this for initialization
    void Start ()
    {
        playerTransform = SCR_HeadsetReferences.instance.playerSpace.transform;
        headsetTransform = SCR_HeadsetReferences.instance.centerEye.transform;

        GameObject variableObject = GameObject.FindGameObjectWithTag("LeftVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            OnEnable();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (bActivationButtonPressed)
        {
            bTeleportActivated = true;

            teleportEffectGameObject.transform.position = variablePointer.PointerEndGameObject.transform.position;
            /*
            if (controllerEvents.GetTouchpadAxis().y > 0.5f)
            {
                
            }
            */
        }
        else
        {
            bTeleportActivated = false;
        }

        if (SCR_ToolOptions.instance.OptionsMenuOpen)
        {
            bTeleportActivated = false;
            bActivationButtonPressed = false;
            variablePointer.VariablePointerActive = false;
            variablePointer.SnapPointerState(PointerStates.Short);

            if (teleportEffectGameObject)
            {
                teleportEffectGameObject.SetActive(false);
            }

            variablePointer.SetPointerColourDefault();
            variablePointer.SetPointerWidthDefault();
            variablePointer.SetPointerEndSizeDefault();
            bTeleportActivated = false;
            bCurrentlyTeleporting = false;
        }
	}
}
