using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsAffected
{
    void ApplyPhysicsForce(float forceAmount, Vector3 forceDirection, Vector3 forcePosition);
}
