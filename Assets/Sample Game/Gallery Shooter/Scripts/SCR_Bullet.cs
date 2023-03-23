using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Bullet : MonoBehaviour {

    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float bulletSpeed = 1f;
    [SerializeField] private int damageAmount = 100;
    [SerializeField] private float forceAmount = 10f;

    private IDamageable damageableScript;
    private IPhysicsAffected physicsScript;

    // Careful when setting this to true - it might cause double
    // events to be fired - but it won't pass through the trigger
    public bool sendTriggerMessage = false;

    public LayerMask layerMask = -1; //make sure we aren't in this layer 
    public float skinWidth = 0.1f; //probably doesn't need to be changed 

    private float minimumExtent;
    private float partialExtent;
    private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Rigidbody myRigidbody;
    private Collider myCollider;

    // Use this for initialization
    void Start ()
    {
        rb.velocity = transform.forward * bulletSpeed;
        Destroy(gameObject, 5f);

        //Adapted from Brauer (2015) http://wiki.unity3d.com/index.php?title=DontGoThroughThings
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        previousPosition = myRigidbody.position;
        minimumExtent = Mathf.Min(Mathf.Min(myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;
    }
	
    private void OnCollisionEnter(Collision other)
    {
        DamageCheck(other.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        DamageCheck(other);
    }

    void DamageCheck(Collider other)
    {
        if (other.CompareTag("GGDamageable"))
        {
            damageableScript = other.GetComponent<IDamageable>();
            physicsScript = other.GetComponent<IPhysicsAffected>();

            if (damageableScript != null)
            {
                damageableScript.TakeDamage(damageAmount);
            }

            if (physicsScript != null)
            {
                Vector3 forceDirection = transform.forward;
                Vector3 forcePosition = transform.position;
                physicsScript.ApplyPhysicsForce(forceAmount, forceDirection, forcePosition);
            }
        }

        Destroy(gameObject);
    }


    //Adapted from Brauer (2015) http://wiki.unity3d.com/index.php?title=DontGoThroughThings
    //This is used to ensure bullets don't appear to pass through other objects when moving too quickly
    void Update()
    {
        //have we moved more than our minimum extent? 
        Vector3 movementThisStep = myRigidbody.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > sqrMinimumExtent)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit hitInfo;

            //check for obstructions we might have missed 
            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementMagnitude, layerMask.value))
            {
                if (!hitInfo.collider)
                    return;

                if (hitInfo.collider.isTrigger)
                {
                    DamageCheck(hitInfo.collider);
                }

                if (!hitInfo.collider.isTrigger)
                    myRigidbody.position = hitInfo.point - (movementThisStep / movementMagnitude) * partialExtent;

            }
        }

        previousPosition = myRigidbody.position;

        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            Destroy(gameObject);
        }
        
    }
}
