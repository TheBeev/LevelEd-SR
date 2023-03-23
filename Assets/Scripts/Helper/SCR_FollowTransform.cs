using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_FollowTransform : MonoBehaviour {

	[SerializeField] private Transform transformToFollow;
	[SerializeField] private Vector3 offset;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = transformToFollow.position + offset;
	}
}
