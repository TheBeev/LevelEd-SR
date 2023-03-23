using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Target : MonoBehaviour, IDamageable, IPhysicsAffected, ILevelItemInteractive
{
    [SerializeField] private int health = 100;
    [SerializeField] private AudioSource targetDestroyedAudioSource;

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
        bDestroyed = false;
        bStartTriggerState = targetMeshCollider.isTrigger;
        bStartGravityState = targetRB.useGravity;
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
            SCR_ScoreboardManager.instance.RegisterTarget();
            bRegistered = true;
        }
        
    }

    public void Disable()
    {
        Reset();
    }

    void Reset()
    {
        targetMeshCollider.isTrigger = bStartTriggerState;
        targetRB.useGravity = bStartGravityState;
        targetRB.constraints = RigidbodyConstraints.FreezeAll;
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
        targetRB.velocity = Vector3.zero;
        health = startHealth;
        
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0 && !bDestroyed)
        {
            targetDestroyedAudioSource.Play();
            Destroyed();
        }
    }

    public void ApplyPhysicsForce(float forceAmount, Vector3 forceDirection, Vector3 forcePosition)
    {
        if (targetRB)
        {
            targetRB.AddForceAtPosition(forceDirection * forceAmount, forcePosition, ForceMode.Impulse);
        }
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

        bDestroyed = true;

        if (SCR_ScoreboardManager.instance)
        {
            SCR_ScoreboardManager.instance.TargetDestroyed();
        }
        
    }

}
