using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCR_TargetLaserBeam : MonoBehaviour, IDamageable, IPhysicsAffected, ILevelItemInteractive
{
    [SerializeField] private int health = 400;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float timeToCharge = 2.0f;
    [SerializeField] private float timeToDischarge = 1.5f;
    [SerializeField] private float angleOfViewCone = 90f;
    [SerializeField] private float firingDuration = 5f;
    [SerializeField] private Image chargeInsideImage;
    [SerializeField] private Image healthInsideImage;
    [SerializeField] private Transform beamPoint;
    [SerializeField] private LineRenderer laserBeamRenderer;
    [SerializeField] private AudioSource targetDestroyedAudioSource;
    [SerializeField] private AudioSource targetHitAudioSource;
    [SerializeField] private AudioSource laserChargingAudioSource;
    [SerializeField] private AudioSource laserFiringAudioSource;

    private Rigidbody targetRB = null;
    private MeshCollider targetMeshCollider = null;
    private bool bDestroyed = false;
    private bool bRegistered = false;

    //reset values
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;
    private int startHealth;
    private bool bStartTriggerState;
    private bool bStartGravityState;
    private bool bAddedToInteractiveList;
    private GameObject playerObject;

    private float currentChargeAmount;
    private bool bFiring;
    private LayerMask layerMaskToIgnore = 12;

    void Start()
    {
        targetRB = GetComponent<Rigidbody>();
        targetMeshCollider = GetComponent<MeshCollider>();
        targetRB.constraints = RigidbodyConstraints.None;

        if (!bAddedToInteractiveList)
        {
            SCR_LevelEditorManager.instance.AddPrefabsToInteractive(this.gameObject);
            bAddedToInteractiveList = true;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
        startHealth = health;
        currentChargeAmount = 0f;
        LayerMask tempLayerMask = 1 << 12;
        layerMaskToIgnore = ~tempLayerMask;
        bDestroyed = false;
        bFiring = false;
        bStartTriggerState = targetMeshCollider.isTrigger;
        bStartGravityState = targetRB.useGravity;

        playerObject = GameObject.Find("LaserTarget");
    }

    void OnDisable()
    {
        SCR_LevelEditorManager.instance.RemovePrefabsFromInteractive(this.gameObject);

        if (SCR_ScoreboardManager.instance && bRegistered)
        {
            SCR_ScoreboardManager.instance.DeregisterTarget();
            bRegistered = false;
        }
    }

    public void Enable()
    {
        Start();

        if (SCR_ScoreboardManager.instance)
        {
            bRegistered = true;
            SCR_ScoreboardManager.instance.RegisterTarget();
        }
        
    }

    public void Disable()
    {
        Reset();
    }

    void Reset()
    {

        StopAudio();

        targetMeshCollider.isTrigger = bStartTriggerState;
        targetRB.useGravity = bStartGravityState;
        targetRB.constraints = RigidbodyConstraints.FreezeAll;
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
        targetRB.velocity = Vector3.zero;
        health = startHealth;
        currentChargeAmount = 0f;
        UpdateHealthUI();
        chargeInsideImage.fillAmount = 1f;
        bDestroyed = false;
        bFiring = false;
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        UpdateHealthUI();

        print(health);

        if (health <= 0 && !bDestroyed)
        {
            bDestroyed = true;

            StopAudio();

            targetDestroyedAudioSource.Play();
            Destroyed();
        }
        else
        {
            targetHitAudioSource.Play();
        }
    }

    void StopAudio()
    {
        if (laserChargingAudioSource.isPlaying)
        {
            laserChargingAudioSource.Stop();
        }

        if (laserFiringAudioSource.isPlaying)
        {
            laserFiringAudioSource.Stop();
        }
    }

    public void ApplyPhysicsForce(float forceAmount, Vector3 forceDirection, Vector3 forcePosition)
    {
        if (targetRB && bDestroyed)
        {
            targetRB.AddForceAtPosition(forceDirection * forceAmount, forcePosition, ForceMode.Impulse);
        }
    }

    void UpdateHealthUI()
    {
        healthInsideImage.fillAmount = (float)health / (float)startHealth;
    }

    void UpdateChargeUI()
    {
        chargeInsideImage.fillAmount = currentChargeAmount / timeToCharge;
    }

    void Destroyed()
    {
        if (targetMeshCollider)
        {
            targetMeshCollider.isTrigger = false;
        }

        if (targetRB)
        {
            targetRB.useGravity = true;
        }

        if (SCR_ScoreboardManager.instance)
        {
            SCR_ScoreboardManager.instance.TargetDestroyed();
        }
        
    }

    void Update()
    {
        
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.Game && !bDestroyed)
        {
            if (SCR_ScoreboardManager.instance != null)
            {
                if (!SCR_ScoreboardManager.instance.GameOver)
                {
                    if (Physics.Linecast(beamPoint.position, playerObject.transform.position, layerMaskToIgnore) && !bFiring)
                    {
                        //Debug.Log("blocked");
                        if (laserChargingAudioSource.isPlaying)
                        {
                            laserChargingAudioSource.Pause();
                        }

                        currentChargeAmount -= Time.deltaTime;
                        currentChargeAmount = Mathf.Max(0, currentChargeAmount);
                        UpdateChargeUI();

                    }
                    else if (!bFiring)
                    {
                        //adapted from https://forum.unity.com/threads/check-if-a-gameobject-is-in-front-of-my-player-character.166432/
                        //checks to ensure the player is in front and within X angle
                        Vector3 tempPlayerPosition = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y, playerObject.transform.position.z);
                        Vector3 directionToPlayer = tempPlayerPosition - beamPoint.position;
                        float angle = Vector3.Angle(beamPoint.forward.normalized, directionToPlayer);

                        //print("Angle: " + angle);

                        if (Mathf.Abs(angle) < angleOfViewCone)
                        {
                            //Debug.Log("Visible And In Angle");
                            currentChargeAmount += Time.deltaTime;
                            UpdateChargeUI();

                            if (!laserChargingAudioSource.isPlaying)
                            {
                                laserChargingAudioSource.time = currentChargeAmount * 0.5f;
                                laserChargingAudioSource.Play();
                            }

                            if (currentChargeAmount >= timeToCharge)
                            {
                                if (laserChargingAudioSource.isPlaying)
                                {
                                    laserChargingAudioSource.Pause();
                                }

                                StartCoroutine(FiringLaser());
                            }
                        }
                        else
                        {
                            
                            currentChargeAmount -= Time.deltaTime;

                            if (laserChargingAudioSource.isPlaying)
                            {
                                laserChargingAudioSource.Pause();
                            }

                            currentChargeAmount = Mathf.Max(0, currentChargeAmount);
                            UpdateChargeUI();
                        }
                    }
                }
            }
        }    
    }

    IEnumerator FiringLaser()
    {
        //print("Firing");
        bFiring = true;
        float firingTimer = 0f;
        RaycastHit hitInfo;

        laserBeamRenderer.enabled = true;

        Vector3 beamDirection = playerObject.transform.position - beamPoint.position;

        if (!laserFiringAudioSource.isPlaying)
        {
            laserFiringAudioSource.Play();
        }
        
        while (firingTimer <= firingDuration && !bDestroyed)
        {
            firingTimer += Time.deltaTime;

            laserBeamRenderer.SetPosition(0, beamPoint.position);

            if (Physics.Raycast(beamPoint.position, beamDirection, out hitInfo, 100f))
            {
                laserBeamRenderer.SetPosition(1, hitInfo.point);
                if (hitInfo.collider.CompareTag("GGTakeDamage"))
                {
                    hitInfo.collider.gameObject.GetComponent<SCR_PlayerData>().TakeDamage(damageAmount);
                }
            }
            else
            {
                laserBeamRenderer.SetPosition(1, beamPoint.position + beamDirection * 100f);
            }

            

            yield return null;
        }

        laserBeamRenderer.SetPosition(0, beamPoint.position);
        laserBeamRenderer.SetPosition(1, beamPoint.position);

        if (laserFiringAudioSource.isPlaying)
        {
            laserFiringAudioSource.Stop();
        }

        laserBeamRenderer.enabled = false;
        bFiring = false;
        currentChargeAmount = 0f;
        yield return null; 
    }

}
