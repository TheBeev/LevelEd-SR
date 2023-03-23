using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_FloorFix : MonoBehaviour {

	[SerializeField] private Transform eyeLevelTransform;

	// Use this for initialization
	void Start () 
	{
		Invoke("DelayedPosition", 1f);
	}

	void DelayedPosition()
    {
		Vector3 newPosition = eyeLevelTransform.position;
		newPosition.y -= eyeLevelTransform.localPosition.y;

		transform.position = newPosition;
	}
	
}
