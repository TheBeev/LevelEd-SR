﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PositionV
{
	public float x = 0;
	public float y = 0;
	public float z = 0;

	public PositionV(float posX, float posY, float posZ)
	{
		x = posX;
		y = posY;
		z = posZ;
	}
}

//created because vectors can't be serialized 
[System.Serializable]
public class FreeformListV
{
	public List<PositionV> freeformList = new List<PositionV>();
    public string objectTag;
}

//created because vectors can't be serialized
[System.Serializable]
public class ContinuousListV
{
	public List<PositionV> continuousList = new List<PositionV> ();
    public string wallTag;
}

[System.Serializable]
public class FreeformListG
{
	public List<GameObject> freeformList = new List<GameObject>();
    public string objectTag;
}

[System.Serializable]
public class ContinuousListG
{
	public List<GameObject> continuousGList = new List<GameObject>();
    public string wallTag;
}

[System.Serializable]
public class ControllerListT
{
	public PositionV leftControllerLocation;
	public PositionV rightControllerLocation;
}

[System.Serializable]
public class MeshList
{
	public byte[] meshData;
}


[System.Serializable]
class WorldSaveData
{
	public List<PositionV> pointLocationList = new List<PositionV>();
	public List<PositionV> depthPointLocationList = new List<PositionV>();
	public List<ContinuousListV> continuousPointLocationList = new List<ContinuousListV> ();
	public List<FreeformListV> freeformPointLocationList = new List<FreeformListV>();
	public ControllerListT controllerPositions = new ControllerListT();
}

[System.Serializable]
class WorldSaveDataMesh
{
	//stores data on meshing
	public List<MeshList> listOfMeshData = new List<MeshList>();
	public ControllerListT controllerPositions = new ControllerListT();
}
