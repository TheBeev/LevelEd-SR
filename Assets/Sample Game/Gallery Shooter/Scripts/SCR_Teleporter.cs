using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Teleporter : MonoBehaviour, ILevelItemInteractive, ISSAcceptsBool
{

    [SerializeField] private bool bStartingTeleporter = false;
    [SerializeField] private Renderer teleporterEffectRenderer = null;

    [SerializeField] private Transform lineEndTransform;
    public Transform LineEndTransform
    {
        get { return lineEndTransform; }
    }

    private Transform playerTransform;
    private Transform headsetTransform;
    private Vector3 headsetOffset;
    private Color previousColour;
    private WaitForSeconds fadeDelay = new WaitForSeconds(0.25f);
    private bool bCurrentlyTeleporting;

    private bool bAddedToInteractiveList;

	// Use this for initialization
	void Start ()
    {
        //playerTransform = VRTK_DeviceFinder.PlayAreaTransform();
        //headsetTransform = VRTK_DeviceFinder.HeadsetTransform();
        //headsetFadeScript = GetComponent<VRTK_HeadsetFade>();
        previousColour = teleporterEffectRenderer.material.color;

        if (!bAddedToInteractiveList)
        {
            SCR_LevelEditorManager.instance.AddPrefabsToInteractive(this.gameObject);
            bAddedToInteractiveList = true;
        }
    }

    void OnDisable()
    {
        SCR_LevelEditorManager.instance.RemovePrefabsFromInteractive(this.gameObject);
    }

    //driven through the spatial scripting system.
    public void AcceptBool(int variableOrder, bool incomingVariable)
    {
        bStartingTeleporter = incomingVariable;
    }

    public void Enable()
    {
        Start();  

        if (SCR_ScoreboardManager.instance != null)
        {
            SCR_ScoreboardManager.instance.RegisterTeleporter(this.gameObject);
        }

        if (bStartingTeleporter)
        {
            TeleportHere();
        }
    }

    public void Disable()
    {
        Highlighted(false);
        TeleporterEffectEnabled(true);
    }

    public void TeleportHere()
    {
        if (!bCurrentlyTeleporting)
        {
            bCurrentlyTeleporting = true;
            StartCoroutine(TeleportHereRoutine());
        }
    }

    IEnumerator TeleportHereRoutine()
    {
        //headsetFadeScript.Fade(Color.black, 0.25f);
        headsetOffset = new Vector3(playerTransform.position.x - headsetTransform.position.x, 0f, playerTransform.position.z - headsetTransform.position.z);

        if (SCR_ScoreboardManager.instance)
        {
            SCR_ScoreboardManager.instance.EnableTeleporterEffects();
        }
        
        TeleporterEffectEnabled(false);

        yield return fadeDelay;

        playerTransform.localPosition = transform.position + headsetOffset;
        playerTransform.localRotation = transform.rotation;
        //headsetFadeScript.Unfade(0.25f);
        bCurrentlyTeleporting = false;
    }

    public void TeleporterEffectEnabled(bool bToggleOn)
    {
        if (bToggleOn)
        {
            teleporterEffectRenderer.gameObject.SetActive(true);
        }
        else
        {
            teleporterEffectRenderer.gameObject.SetActive(false);
        }
    }

    public void Highlighted(bool bHighlighted)
    {
        if (bHighlighted)
        {
            teleporterEffectRenderer.material.color = Color.blue;
            teleporterEffectRenderer.material.SetColor("_EmissionColor", Color.blue);
        }
        else
        {
            teleporterEffectRenderer.material.color = previousColour;
            teleporterEffectRenderer.material.SetColor("_EmissionColor", previousColour);
        }
    }

}
