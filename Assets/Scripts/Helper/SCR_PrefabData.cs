using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PrefabData
{
    public MeshRenderer individualObjectRenderer;
    public Material defaultMaterial;
    public Texture defaultTexture;
}

public class SCR_PrefabData : MonoBehaviour {

    [SerializeField] private List<GameObject> listOfChildren = new List<GameObject>();
    public int prefabID = 0;

    private List<PrefabData> currentPrefabData = new List<PrefabData>();

	// Use this for initialization
	void Start ()
    {
        for (int i = 0; i < listOfChildren.Count; i++)
        {
            if (listOfChildren[i].GetComponent<MeshFilter>() != null)
            {
                PrefabData newPrefabData = new PrefabData();
                newPrefabData.individualObjectRenderer = listOfChildren[i].GetComponent<MeshRenderer>();
                newPrefabData.defaultMaterial = newPrefabData.individualObjectRenderer.sharedMaterial;
                newPrefabData.defaultTexture = newPrefabData.individualObjectRenderer.sharedMaterial.mainTexture;

                currentPrefabData.Add(newPrefabData);
            }
        }
	}

    public void CurrentlySelected()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
            currentPrefabData[i].individualObjectRenderer.material.mainTexture = currentPrefabData[i].defaultTexture;
            currentPrefabData[i].individualObjectRenderer.gameObject.layer = 2;
        }
    }

    public void Deselected()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material = currentPrefabData[i].defaultMaterial;
            currentPrefabData[i].individualObjectRenderer.gameObject.layer = 8;
        }
    }

    public void CurrentlyHighlighted()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material = SCR_ToolMenuRadial.instance.highlightedObjectMaterial;
            currentPrefabData[i].individualObjectRenderer.material.mainTexture = currentPrefabData[i].defaultTexture;
        }
    }

    public void StopHighlighting()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material = currentPrefabData[i].defaultMaterial;
        }
    }

    public void CurrentlyGuiding(Material newMaterial, bool bChangeMaterial)
    {
        Start();

        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            if (bChangeMaterial)
            {
                Texture currentModelTexture = currentPrefabData[i].individualObjectRenderer.sharedMaterial.mainTexture;
                currentPrefabData[i].individualObjectRenderer.material = newMaterial;
                currentPrefabData[i].individualObjectRenderer.material.mainTexture = currentModelTexture;
            }
            
            gameObject.layer = 0; 
        }

        Transform[] listOfChildTransforms = GetComponentsInChildren<Transform>();

        foreach (Transform item in listOfChildTransforms)
        {
            item.gameObject.layer = 0;
        }
    }

    public void StopGuiding()
    {
        gameObject.layer = 8;

        Transform[] listOfChildTransforms = GetComponentsInChildren<Transform>();

        foreach (Transform item in listOfChildTransforms)
        {
            item.gameObject.layer = 8;
        }
    }

}
