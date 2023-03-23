using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SCR_GunLaser : MonoBehaviour {

    [SerializeField] private ControllerInputs activationButton = ControllerInputs.RightTrigger;
    [SerializeField] private Transform gunTip;
    [SerializeField] private Transform gunLaserTip;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LineRenderer laserSight;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI gameoverText;
    [SerializeField] private AudioSource laserGunSoundSource;
    [SerializeField] private Color laserSightStartDefaultColour = Color.red;
    [SerializeField] private Color laserSightEndDefaultColour = Color.white;
    [SerializeField] private Color laserSightStartHighlightColour = Color.green;
    [SerializeField] private Color laserSightEndHighlightColour = Color.white;
    [SerializeField] private float laserSightLength = 3f;
    [SerializeField] private float fireRateDelay = 0.5f;
    [SerializeField] private string controllerNameToHide;
    [SerializeField] private ControllerHand controllerHand = ControllerHand.LeftHand;

    private bool bActivationButtonPressed;
    private float fireRateTimer;
    private bool bTargetingTeleporter;
    private Ray laserRay;
    private Vector3 fwd;
    private RaycastHit hit;
    private GameObject currentHighlightedTeleporter;
    private GameObject controllerToHide;

    private bool bFirstTime = true;

    private void OnEnable()
    {
        if (!bFirstTime)
        {
            gameoverText.text = "";

            controllerToHide = GameObject.Find(controllerNameToHide);
            if (controllerToHide)
            {
                controllerToHide.SetActive(false);
            }

            if (SCR_ScoreboardManager.instance != null)
            {
                print("Registered Gun");
                SCR_ScoreboardManager.instance.RegisterGuns(gameObject, healthText, gameoverText);
            }

            SCR_InputDetection.instance.SubscribeToInput(activationButton, DoActivationButtonPressed);
        }
    }

    private void OnDisable()
    {
        SCR_InputDetection.instance.UnsubscribeFromInput(activationButton, DoActivationButtonPressed);

        if (controllerToHide)
        {
            controllerToHide.SetActive(true);
        }

    }

    void DoActivationButtonPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            bActivationButtonPressed = true;
        }
    }

    void DoActivationButtonDepressed(InputAction.CallbackContext context)
    {
        bActivationButtonPressed = false;
    }

    // Use this for initialization
    void Start ()
    {
        bFirstTime = false;

        if (gameObject.activeInHierarchy)
        {
            OnEnable();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.Game)
        {
            fireRateTimer += Time.deltaTime;

            laserRay.origin = gunLaserTip.position;

            laserRay.direction = gunLaserTip.forward;

            fwd = transform.TransformDirection(Vector3.forward);

            laserSight.SetPosition(0, gunLaserTip.position);

            if (Physics.Raycast(laserRay, out hit, 1000f))
            {
                if (hit.collider.gameObject.tag == "GGTeleporter")
                {
                    bTargetingTeleporter = true;
                    laserSight.startColor = laserSightStartHighlightColour;
                    laserSight.endColor = laserSightEndHighlightColour;
                    if (currentHighlightedTeleporter != hit.collider.gameObject)
                    {
                        currentHighlightedTeleporter = hit.collider.gameObject;
                        currentHighlightedTeleporter.transform.parent.GetComponent<SCR_Teleporter>().Highlighted(true);
                    }
                }
                else
                {
                    if (currentHighlightedTeleporter != null)
                    {
                        currentHighlightedTeleporter.transform.parent.GetComponent<SCR_Teleporter>().Highlighted(false);
                        currentHighlightedTeleporter = null;
                    }

                    laserSight.startColor = laserSightStartDefaultColour;
                    laserSight.endColor = laserSightEndDefaultColour;
                    bTargetingTeleporter = false;
                }

                if (Vector3.Distance(hit.point, gunLaserTip.position) <= laserSightLength)
                {
                    laserSight.SetPosition(1, hit.point);
                }
                else
                {
                    laserSight.SetPosition(1, gunLaserTip.position + (gunLaserTip.forward * laserSightLength));
                }
                
            }
            else
            {
                laserSight.startColor = laserSightStartDefaultColour;
                laserSight.endColor = laserSightEndDefaultColour;

                if (currentHighlightedTeleporter != null)
                {
                    currentHighlightedTeleporter.transform.parent.GetComponent<SCR_Teleporter>().Highlighted(false);
                    currentHighlightedTeleporter = null;
                }

                bTargetingTeleporter = false;
                laserSight.SetPosition(1, gunLaserTip.position + (gunLaserTip.forward * laserSightLength));
            }

            if (!bTargetingTeleporter)
            {
                if (bActivationButtonPressed && fireRateTimer >= fireRateDelay)
                {
                    SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.5f, controllerHand);

                    fireRateTimer = 0f;
                    bActivationButtonPressed = false;
                    laserGunSoundSource.Play();
                    Instantiate(bulletPrefab, gunTip.position, gunTip.rotation);
                }
            }
            else
            {
                if(bActivationButtonPressed)
                {
                    if (currentHighlightedTeleporter)
                    {
                        currentHighlightedTeleporter.transform.parent.GetComponent<SCR_Teleporter>().TeleportHere();
                    }
                    fireRateTimer = 0f;
                    bActivationButtonPressed = false;
                }
            }
        }
	}
}
