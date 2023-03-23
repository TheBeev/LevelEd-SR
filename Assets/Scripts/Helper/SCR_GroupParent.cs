using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroupData
{
    public MeshRenderer individualObjectRenderer;
    public Material defaultMaterial;
    public Texture defaultTexture;
}

public class SCR_GroupParent : MonoBehaviour {

    public List<GameObject> groupedObjectList = new List<GameObject>();
    public int ID;
    public bool bMaterialsCached = false;

    public List<GroupData> currentGroupData = new List<GroupData>();

    public void CheckMaterialCache()
    {

        if (!bMaterialsCached)
        {
            currentGroupData.Clear();

            for (int i = 0; i < groupedObjectList.Count; i++)
            {
                if (groupedObjectList[i].GetComponent<MeshFilter>() != null)
                {
                    GroupData newGroupData = new GroupData();
                    newGroupData.individualObjectRenderer = groupedObjectList[i].GetComponent<MeshRenderer>();
                    newGroupData.defaultMaterial = newGroupData.individualObjectRenderer.sharedMaterial;
                    newGroupData.defaultTexture = newGroupData.individualObjectRenderer.sharedMaterial.mainTexture;

                    currentGroupData.Add(newGroupData);
                }
            }

            bMaterialsCached = true;
        }
    }

    public void UpdateCachedMaterials()
    {
        if (!bMaterialsCached)
        {
            CheckMaterialCache();
        }

        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].defaultMaterial = currentGroupData[i].individualObjectRenderer.sharedMaterial;
            currentGroupData[i].defaultTexture = currentGroupData[i].individualObjectRenderer.sharedMaterial.mainTexture;
        }
    }

    public void Reset()
    {
        bMaterialsCached = false;
        CheckMaterialCache();
    }

    public void CurrentlySelected()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.sharedMaterial = SCR_ToolMenuRadial.instance.selectedObjectMaterial;
            currentGroupData[i].individualObjectRenderer.material.mainTexture = currentGroupData[i].defaultTexture;
            currentGroupData[i].individualObjectRenderer.gameObject.layer = 2;
        }
    }

    public void Deselected()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material = currentGroupData[i].defaultMaterial;
            currentGroupData[i].individualObjectRenderer.gameObject.layer = 8;
        }
    }

    public void CurrentlyHighlighted()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material = SCR_ToolMenuRadial.instance.highlightedObjectMaterial;
            currentGroupData[i].individualObjectRenderer.material.mainTexture = currentGroupData[i].defaultTexture;
        }
    }

    public void StopHighlighting()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material = currentGroupData[i].defaultMaterial;
        }
    }

}
